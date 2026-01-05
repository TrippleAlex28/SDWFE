using System.Collections.Concurrent;
using Engine.Network.Client;
using Engine.Network.Shared.Command;
using Engine.Network.Shared.Object;
using Engine.Network.Shared.Packet;
using Engine.Network.Shared.Packet.Packets;
using Microsoft.Xna.Framework;

namespace Engine.Network.Shared.Session.Sessions;

public class MultiplayerClientSession : GameSession
{
    private NetClient _netClient;
    
    private List<NetCommand> _frameCommands = new();
    private List<NetCommand> _pendingCommands = new();
    private uint _nextSequenceNumber = 1;

    private readonly ConcurrentQueue<KeyValuePair<PacketType, Packet.Packet>> _packetQueue = new();
    private SnapshotPacket? _latestFutureSnapshot;

    public MultiplayerClientSession() : base(SessionType.MultiplayerClient)
    {
        _netClient = new NetClient();
    }

    public override void Initialize()
    {
        _netClient.Disconnected += OnDisconnected;
        _netClient.PacketReceived += OnPacketReceived;
        
        base.Initialize();
    }
    
    public override void Reset()
    {
        _netClient.Disconnected -= OnDisconnected;
        _netClient.PacketReceived -= OnPacketReceived;
        
        base.Reset();
    }

    /// <summary>
    /// Try connecting to a host address
    /// </summary>
    public async Task<bool> ConnectAsync(string host)
    {
        if (!IsInitialized) return false;
        
        bool ok = await _netClient.ConnectAsync(host);
        if (ok)
            LocalClientId = _netClient.ClientId;

        return ok;
    }

    /// <summary>
    /// Disconnect gracefully from the server
    /// </summary>
    public async Task DisconnectAsync(string reason)
    {
        if (!IsInitialized) return;
        
        await _netClient.DisconnectAsync(reason);
    }
    
    public override void HandleInput(List<NetCommand> commands)
    {
        if (!IsInitialized || !_netClient.Connected) return;

        Scene.Scene? scene = GameState.Instance.CurrentScene;
        if (scene == null) return;
        
        _frameCommands = commands;

        // Stamp & Apply commands before sending them
        foreach (var c in _frameCommands)
        {
            c.SequenceNumber = _nextSequenceNumber++;
            c.Tick = GameState.Instance.ClientTick;
            _pendingCommands.Add(c);
            
            // Apply immediately
            c.Apply(scene, LocalClientId);
        }
        
        if (_frameCommands.Count > 0)
        {
            CommandPacket packet = new CommandPacket
            {
                ClientId = LocalClientId,
                Commands = _frameCommands,
            };

            _netClient.SendCommandPacket(packet);
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (!IsInitialized || !_netClient.Connected) return;
        
        // Process packets
        while (_packetQueue.TryDequeue(out var packetData))
        {
            switch (packetData.Key)
            {
                case PacketType.ChatPacket:
                    HandleChatPacket((ChatPacket)packetData.Value);
                    break;
                case PacketType.SceneChangePacket:
                    HandleSceneChangePacket((SceneChangePacket)packetData.Value);
                    break;
                case PacketType.SnapshotPacket:
                    HandleSnapshotPacket((SnapshotPacket)packetData.Value);
                    break;
            }
        }
    }

    /// <summary>
    /// Handles this client's disconnect from the server
    /// </summary>
    /// <param name="reason"></param>
    private void OnDisconnected(string reason)
    {
        GameState.Instance.OnDisconnect(reason);
    }

    /// <summary>
    /// Event handlers that filters received packets to packets that need an action performed
    /// </summary>
    private void OnPacketReceived(PacketType type, Packet.Packet packet)
    {
        _packetQueue.Enqueue(new KeyValuePair<PacketType, Packet.Packet>(type, packet));
    }

    /// <summary>
    /// Handle a received ChatPacket
    /// </summary>
    private void HandleChatPacket(ChatPacket packet)
    {
        
    }

    /// <summary>
    /// Handle a received SceneChangePacket
    /// </summary>
    private void HandleSceneChangePacket(SceneChangePacket packet)
    {
        // Outdated packet
        if (packet.SceneEpoch < GameState.Instance.SceneEpoch)
        {
            return;
        }
        
        // Stop prediction + clear buffers 
        _frameCommands.Clear();
        _pendingCommands.Clear();

        // If this is the local client of a host, don't change scenes as host has already done that
        if (GameState.Instance.SessionManager.IsHost) return;
        
        // Clear queued snapshot packets
        List<KeyValuePair<PacketType, Packet.Packet>> validPackets = new();
        while (_packetQueue.TryDequeue(out var packetData))
        {
            if (packetData.Key == PacketType.SnapshotPacket)
            {
                // Keep all future snapshot packets & non snapshot packets
                if (((SnapshotPacket)packetData.Value).SceneEpoch >= GameState.Instance.SceneEpoch)
                {
                    validPackets.Add(packetData);
                }
            }
            else
            {
                validPackets.Add(packetData);       
            }
        }
        // Requeue valid packets
        foreach (var p in validPackets)
        {
            _packetQueue.Enqueue(p);
        }
        
        GameState.Instance.SwitchSceneClient(packet.SceneEpoch, packet.SceneKey);
        
        // Apply stored future snapshot
        if (_latestFutureSnapshot != null)
        {
            HandleSnapshotPacket(_latestFutureSnapshot);
            _latestFutureSnapshot = null;
        }
    }
    
    /// <summary>
    /// Handle a received SnapshotPacket
    /// </summary>
    private void HandleSnapshotPacket(SnapshotPacket packet)
    {
        Scene.Scene? scene = GameState.Instance.CurrentScene;
        if (scene == null) return;
        
        // Snapshot packet from old scene
        if (packet.SceneEpoch < GameState.Instance.SceneEpoch) return;
        
        // We are behind, queue this packet
        if (packet.SceneEpoch > GameState.Instance.SceneEpoch)
        {
            _latestFutureSnapshot = packet;
            return;
        }
        
        // Set our tick to match the server
        GameState.Instance.SetServerTick(packet.Tick);

        // If this is the local client of a host, don't apply snapshot as host has already applied commands
        // We do update our pendingscommands list to prevent infinite growth of that list
        if (GameState.Instance.SessionManager.IsHost)
        {
            uint lastProcessedCommandSeqSigmaBoy =
                packet.LastProcessedSequencePerClient.TryGetValue(LocalClientId, out uint sigmaBoy) ? sigmaBoy : 0;
            _pendingCommands.RemoveAll(c => c.SequenceNumber <= lastProcessedCommandSeqSigmaBoy);
            
            return;
        }
        
        // Sync local scene root with sent scene root
        SyncReplicatedObjects(scene, packet.ReplicatedObjects);

        // Drop all pending actions that the server has already processed
        uint lastProcessedCommandSeq =
            packet.LastProcessedSequencePerClient.TryGetValue(LocalClientId, out uint v) ? v : 0;
        _pendingCommands.RemoveAll(c => c.SequenceNumber <= lastProcessedCommandSeq);

        // Reapply remaining pending actions (client-side prediction reconciliation)
        foreach (var c in _pendingCommands)
            c.Apply(scene, LocalClientId);
    }
    
    #region Sync Scene Root

    /// <summary>
    /// Sync the Scene SceneRoot of the passed in scene with the received snapshot root node
    /// </summary>
    /// <param name="currentScene"></param>
    /// <param name="snapshotRoot"></param>
    private void SyncReplicatedObjects(Scene.Scene currentScene, List<ReplicatedObjectData> snapshotObjects)
    {
        GameObject liveRoot = currentScene.SceneRoot;
        
        // Create a lookup of existing replicated objects 
        var replicatedMap = new Dictionary<int, GameObject>();
        BuildReplicatedMapRecursive(liveRoot, replicatedMap);
        
        // Tracks which replicated IDs are still alive in this snapshot
        HashSet<int> aliveIds = new();
        
        // Sync the replicated subtree into the local scene tree
        foreach (var snapshotObj in snapshotObjects)
        {
            SyncSnapshotNodeRecursive(
                snapshotObj,
                liveRoot,
                replicatedMap,
                aliveIds
            );
        }
        
        // Remove replicated objects that existed locally, but disappeared from the snapshot
        RemoveDespawnedReplicated(liveRoot, aliveIds);
    }

    private void BuildReplicatedMapRecursive(GameObject node, Dictionary<int, GameObject> replicatedMap)
    {
        if (node.ReplicatesOverNetwork)
            replicatedMap[node.NetworkId] = node;
        
        foreach (var child in node.Children)
            BuildReplicatedMapRecursive(child, replicatedMap);
    }

    private void SyncSnapshotNodeRecursive(
        ReplicatedObjectData snapshotNode,
        GameObject liveParent,
        Dictionary<int, GameObject> replicatedMap,
        HashSet<int> aliveIds
    )
    {
        var liveNode = GetOrCreateLiveNode(snapshotNode, liveParent, replicatedMap);

        if (liveNode.ReplicatesOverNetwork)
            aliveIds.Add(liveNode.NetworkId);

        CopyNetProperties(liveNode, snapshotNode);

        foreach (var childNode in snapshotNode.Children)
        {
            SyncSnapshotNodeRecursive(
                childNode,
                liveNode,
                replicatedMap,
                aliveIds
            );
        }
    }

    private GameObject GetOrCreateLiveNode(ReplicatedObjectData snapshotNode, GameObject liveParent,
        Dictionary<int, GameObject> replicatedMap)
    {
        GameObject liveNode;

        if (replicatedMap.TryGetValue(snapshotNode.NetworkId, out var existing))
        {
            liveNode = existing;
            
            // Ensure the parent matches the snapshot parent
            if (liveNode.Parent != null)
            {
                if (!ReferenceEquals(liveNode.Parent, liveParent))
                {
                    liveNode.RemoveFromParent();
                    liveParent.AddChild(liveNode);
                }
            }
        }
        else
        {
            // This cast always succeeds, if it doesn't someone created a child of NetObject that isn't a child of GameObject and should get a whooping
            liveNode = (GameObject)NetObjectRegistry.Create(snapshotNode.TypeId);

            liveNode.NetworkId = snapshotNode.NetworkId;
            liveNode.OwningClientId = snapshotNode.OwningClientId;

            liveParent.AddChild(liveNode);

            replicatedMap[liveNode.NetworkId] = liveNode;
        }
        
        return liveNode;
    }

    private void CopyNetProperties(NetObject target, ReplicatedObjectData source)
    {
        using MemoryStream ms = new(source.NetPropertyBlob);
        using BinaryReader br = new(ms);
        target.Deserialize(br);
    }
    
    private void RemoveDespawnedReplicated(GameObject node, HashSet<int> aliveIds)
    {
        for (int i = node.Children.Count - 1; i >= 0; --i)
        {
            GameObject child = node.Children[i];

            bool isReplicated = child.ReplicatesOverNetwork;
            bool stillAlive =  isReplicated && aliveIds.Contains(child.NetworkId);

            if (isReplicated && !stillAlive)
            {
                child.RemoveFromParent();
                continue;
            }

            RemoveDespawnedReplicated(child, aliveIds);
        }
    }
    
    #endregion 
}