using System.Collections.Concurrent;
using System.Net;
using Engine.Network.Client;
using Engine.Network.Server;
using Engine.Network.Shared.Command;
using Engine.Network.Shared.Packet;
using Engine.Network.Shared.Packet.Packets;
using Microsoft.Xna.Framework;

namespace Engine.Network.Shared.Session.Sessions;

public class MultiplayerHostSession : GameSession
{
    private NetServer _netServer;
    public readonly Dictionary<int, uint> _lastProcessedCommandSeq = new();
    private const float _snapshotRate = 20f;
    private float _snapshotTimer = 1f / _snapshotRate;

    private MultiplayerClientSession _clientSession;

    private readonly ConcurrentQueue<KeyValuePair<PacketType, Packet.Packet>> _packetQueue = new();

    private List<int> _playersToSpawn = new();
    
    public MultiplayerHostSession() : base(SessionType.MultiplayerHost)
    {
        _netServer = new NetServer(
            maxPlayers: 2
        );
        
        _clientSession = new MultiplayerClientSession();
    }
    
    public override async void Initialize()
    {
        base.Initialize();
        
        _netServer.ClientConnected += OnClientConnected;
        _netServer.ClientDisconnected += OnClientDisconnected;
        _netServer.PacketReceived += OnPacketReceived;
        _netServer.UPnPFailed += OnUPnPFailed;
        _netServer.Start();
        
        _clientSession.Initialize();

        bool ok = await _clientSession.ConnectAsync("127.0.0.1");
        if (!ok) throw new Exception("NetClient couldn't connect to local NetServer");
        LocalClientId = _clientSession.LocalClientId;
        
        GameState.Instance.CurrentScene?.RegisterExistingWorldObjects();
    }

    public override async void Reset()
    {
        base.Reset();
        
        _netServer.ClientConnected -= OnClientConnected;
        _netServer.ClientDisconnected -= OnClientDisconnected;
        _netServer.PacketReceived -= OnPacketReceived;
        _netServer.UPnPFailed -= OnUPnPFailed;
        await _netServer.Stop();

        _clientSession.Reset();

        await _clientSession.DisconnectAsync("Server Shut Down.");
    }

    public override void HandleInput(List<NetCommand> commands)
    {
        if (!IsInitialized) return;
        
        _clientSession.HandleInput(commands);
    }

    public override void Update(GameTime gameTime)
    {
        if (!IsInitialized) return;
        
        GameState.Instance.SetServerTick(GameState.Instance.ClientTick);
        
        _clientSession.Update(gameTime);
        
        // Spawn players
        foreach (int playerClientId in _playersToSpawn.Distinct())
        {
            GameState.Instance.CurrentScene?.AddPlayer(playerClientId);
        }
        _playersToSpawn.Clear();
        
        // Handle packets
        while (_packetQueue.TryDequeue(out var packetData))
        {
            switch (packetData.Key)
            {
                case PacketType.CommandPacket:
                    HandleCommandPacket((CommandPacket)packetData.Value);
                    break;
                default:
                    break;
            }
        }
        
        // Snapshot send loop
        _snapshotTimer -= gameTime.DeltaSeconds();
        if (_snapshotTimer <= 0f)
        {
            SendSnapshot();
            _snapshotTimer = 1f / _snapshotRate;
        }
    }

    public override void OnSwitchScene(uint sceneEpoch, string sceneKey)
    {
        if (!IsInitialized) return;
        
        GameState.Instance.CurrentScene?.RegisterExistingWorldObjects();
        
        // Broadcast the new scene 
        var packet = new SceneChangePacket
        {
            SceneEpoch = sceneEpoch,
            SceneKey = sceneKey,
        };
        _ = _netServer.BroadcastTcp(packet.ToBytes());
        
        // Spawn all players
        foreach (var client in _netServer.Clients.Values)
        {
            _playersToSpawn.Add(client.ClientId);
        }
    }
    
    private void SendSnapshot()
    {
        var scene = GameState.Instance.CurrentScene;
        if (scene == null) return;
        
        scene.UpdateDirty();

        var replicatedGameObjects = scene.SceneRoot.Children
            .Where(c => c.ReplicatesOverNetwork)
            .ToList();
        
        var packet = new SnapshotPacket
        {
            Tick = GameState.Instance.ServerTick,
            SceneEpoch = GameState.Instance.SceneEpoch,
            LastProcessedSequencePerClient = _lastProcessedCommandSeq,
            ReplicatedObjects = replicatedGameObjects 
                .Select(ConvertToReplicatedData)
                .ToList(),
        };
        
        Console.WriteLine(packet.ToBytes().Length);
        
        _netServer.BroadcastUdp(packet.ToBytes());

        scene.ClearDirty();
    }

    private ReplicatedObjectData ConvertToReplicatedData(GameObject obj)
    {
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);
        
        obj.Serialize(bw);

        var replicatedChildren = obj.Children
            .Where(c => c.ReplicatesOverNetwork)
            .Select(ConvertToReplicatedData)
            .ToList();

        return new ReplicatedObjectData
        {
            TypeId = obj.TypeId,
            NetworkId = obj.NetworkId,
            OwningClientId = obj.OwningClientId,
            NetPropertyBlob = ms.ToArray(),
            Children = replicatedChildren,
        };
    }
    
    private void OnClientConnected(ClientConnection cc)
    {
        _playersToSpawn.Add(cc.ClientId);
        
        Console.WriteLine("CONNECT");
    }

    private void OnClientDisconnected(ClientConnection cc)
    {
        _lastProcessedCommandSeq.Remove(cc.ClientId);
        GameState.Instance.CurrentScene?.RemoveClientObjects(cc.ClientId);
        Console.WriteLine("DISCONNECT");
    }

    private void OnPacketReceived(PacketType type, Packet.Packet packet)
    {
        // Queue the packet for processing
        _packetQueue.Enqueue(new KeyValuePair<PacketType, Packet.Packet>(type, packet));
    }

    private void OnUPnPFailed()
    {
        
    }
    
    private void HandleCommandPacket(CommandPacket packet)
    {
        Scene.Scene? scene = GameState.Instance.CurrentScene;
        if (scene == null) return;
        
        uint lastSeq = _lastProcessedCommandSeq.TryGetValue(packet.ClientId, out uint v) ? v : 0;
        
        foreach (var c in packet.Commands.OrderBy(c => c.SequenceNumber))
        {
            if (c.SequenceNumber <= lastSeq)
                continue;
            
            c.Apply(scene!, packet.ClientId);
            lastSeq = c.SequenceNumber;
        }

        _lastProcessedCommandSeq[packet.ClientId] = lastSeq;
    }
}