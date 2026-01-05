using System.Net;
using System.Net.Sockets;
using Engine.Network.Shared.Packet;
using Engine.Network.Shared.Packet.Packets;
using Open.Nat;

namespace Engine.Network.Server;

/// <summary>
/// The data saved about every client
/// </summary>
public sealed class ClientConnection
{
    // Client Information
    public int ClientId;
    public string ClientName;

    // TCP Data
    public FramedTcp Framed;
    public bool Connected => Framed.Connected;
    
    // UDP Data
    public IPEndPoint UdpEndPoint;
}

public class NetServer
{
    public bool Running { get; private set; }
    public IPAddress BindAddress { get; private set; }

    // Events
    public event Action? UPnPFailed;
    
    public event Action<ClientConnection>? ClientConnected;
    public event Action<ClientConnection>? ClientDisconnected;

    public event Action<PacketType, Packet>? PacketReceived;
    
    // Server Information
    private readonly int _maxPlayers;
    
    // Client Management
    private int _nextClientId = 1;
    private readonly Dictionary<int, ClientConnection> _clients = new();
    public IReadOnlyDictionary<int, ClientConnection> Clients => _clients;
    
    // TCP Data
    private TcpListener _tcpListener;
    private int _tcpPort;
    public int TcpPort => _tcpPort;
    
    // UDP Data
    private UdpClient _udp;
    private int _udpPort;
    public int UdpPort => _udpPort;

    // UPnP Data
    private Mapping? _UPnPUdpMapping;
    private Mapping? _UPnPTcpMapping;

    private CancellationTokenSource? _cts;
    
    public NetServer(
        int maxPlayers = 2, 
        int tcpPort = 7777, 
        int udpPort = 0)
    {
        _maxPlayers = maxPlayers;
        _tcpPort = tcpPort;
        _udpPort = udpPort;
    }
    
    #region Connection

    public bool Start(bool bindToAllInterfaces = true, bool usePnP = true)
    {
        // Get Server IP Address
        if (bindToAllInterfaces)
        {
            BindAddress = NetworkUtils.GetPrefferedOutboundIPv4() ?? IPAddress.Loopback;
            _tcpListener = new(IPAddress.Any, TcpPort);
        }
        else
        {
            BindAddress = NetworkUtils.GetServerBindAddress();
            _tcpListener = new TcpListener(BindAddress, TcpPort);
        }
        
        // Start TCP Listener
        try
        {
            _tcpListener.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NetServer.Start Exception: {ex.Message}");
            return false;
        }

        // Setup UDP
        IPAddress udpBindAddress = bindToAllInterfaces ? IPAddress.Any : BindAddress;
        _udp = new(new IPEndPoint(udpBindAddress, _udpPort));
        _udpPort = ((IPEndPoint)_udp.Client.LocalEndPoint!).Port;
        
        // Create CancellationTokenSource
        _cts = new();
        
        // Setup UPnP
        if (usePnP)
        {
            _ = Task.Run(async () =>
            {
                _UPnPTcpMapping = await UPnPHelper.TryForwardTcpAsync(_tcpPort, "TCP", _cts.Token);
                _UPnPUdpMapping = await UPnPHelper.TryForwardUdpAsync(_udpPort, "UDP", _cts.Token);
                
                if (_UPnPTcpMapping == null || _UPnPUdpMapping == null)
                {
                    UPnPFailed?.Invoke();
                }
            });
        }
        
        // Start Server Loops
        _ = AcceptLoopAsync(_cts.Token);
        _ = UdpReceiveLoopAsync(_cts.Token);
        
        Running = true;
        return true;
    }

    public async Task Stop()
    {
        if (!Running)
            return;

        // Disconnect all clients
        try
        {
            await BroadcastTcp(new DisconnectPacket
            {
                Reason = "Server Shutting Down"
            }.ToBytes());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NetServer.Stop Exception: {ex.Message}");
        }
        _clients.Clear();

        Running = false;

        // Stop Server Loops
        _cts?.Cancel();
        _tcpListener.Stop();
        _udp.Close();

        // Remove UPnP Mappings
        if (_UPnPTcpMapping != null) _ = Task.Run(async () => { await UPnPHelper.TryRemoveAsync(_UPnPTcpMapping); });
        if (_UPnPUdpMapping != null) _ = Task.Run(async () => { await UPnPHelper.TryRemoveAsync(_UPnPUdpMapping); });
    }
    
    #endregion
    
    #region Loops

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync(ct);

                _ = TcpReceiveLoopAsync(tcpClient, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"NetServer.AcceptLoopAsync Exception: {ex.Message}");
            }
        }
    }

    private async Task TcpReceiveLoopAsync(TcpClient tcpClient, CancellationToken ct)
    {
        FramedTcp framed = new(tcpClient);
        
        // Read received data
        byte[]? data = await framed.ReceiveAsync(ct);
        if (data == null)
            return;
        
        // Check if a connection request packet was sent
        ConnectionRequestPacket connReq;
        try
        {
            connReq = (ConnectionRequestPacket)PacketRegistry.Deserialize(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NetServer.TcpReceiveLoopAsync Exception: {ex.Message}");
            framed.Close();
            return;
        }
        
        // Max players check
        if (_clients.Count >= _maxPlayers)
        {
            DisconnectPacket packet = new()
            {
                Reason = "Server is full"
            };
            await framed.SendAsync(packet.ToBytes());
            return;
        }
        
        // Add client to list
        int clientId = _nextClientId++;
        
        IPAddress remoteIp = ((IPEndPoint)tcpClient.Client.RemoteEndPoint!).Address;
        IPEndPoint udpEndPoint = new(remoteIp, connReq.ClientUdpPort);

        ClientConnection cc = new()
        {
            ClientId = clientId,
            ClientName = connReq.ClientName,
            
            Framed = framed,
            
            UdpEndPoint = udpEndPoint
        };
        _clients.Add(clientId, cc);

        // Send accept packet
        var connAcc = new ConnectionAcceptPacket
        {
            ClientId = cc.ClientId,
            ServerUdpPort = _udpPort,
            CurrentSceneEpoch = GameState.Instance.SceneEpoch,
            CurrentSceneKey = GameState.Instance.CurrentScene!.Name,
        };
        await framed.SendAsync(connAcc.ToBytes()); 
        
        // Raise client connected event
        ClientConnected?.Invoke(cc);

        try
        {
            while (!ct.IsCancellationRequested && framed.Connected)
            {
                byte[]? receivedPayload;

                try
                {
                    receivedPayload = await framed.ReceiveAsync(ct);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"NetServer.TcpReceiveLoopAsync Exception: {ex.Message}");
                    break;
                }

                if (receivedPayload == null)
                    break;

                HandlePacket(cc, receivedPayload);
            }
        }
        finally
        {
            OnClientConnectionLost(cc);
        }
    }

    private async Task UdpReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                UdpReceiveResult result = await _udp.ReceiveAsync(ct);

                ClientConnection? cc =
                    _clients.Values.FirstOrDefault((c) => c.UdpEndPoint.Equals(result.RemoteEndPoint));
                if (cc == null)
                    continue;

                byte[] data = result.Buffer;

                if (data[0] < 1 || data[0] > 8)
                {
                    Console.WriteLine($"Ignoring UDP datagram: from={result.RemoteEndPoint} - firstByte={data[0]}");
                    continue;
                }
                
                HandlePacket(cc, data);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NetServer.UdpReceiveLoopAsync Exception: {ex.Message}");
        }
    }
    
    #endregion 
    
    #region Packet Handlers

    private void HandlePacket(ClientConnection cc, byte[] payload)
    {
        try
        {
            Packet packet = PacketRegistry.Deserialize(payload);
            switch (packet.Type)
            {
                case PacketType.DisconnectPacket:
                    HandleDisconnectPacket(cc, (DisconnectPacket)packet);
                    break;
                case PacketType.ChatPacket:
                    HandleChatPacket((ChatPacket)packet);
                    break;
                case PacketType.CommandPacket:
                    HandleCommandPacket((CommandPacket)packet);
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NetServer.HandlePacket Exception: {ex.Message}");
        }
    }
    
    private void HandleDisconnectPacket(ClientConnection cc, DisconnectPacket packet)
    {
        OnClientConnectionLost(cc);
    }

    private void HandleChatPacket(ChatPacket packet)
    {
        BroadcastTcp(packet.ToBytes());
    }

    private void HandleCommandPacket(CommandPacket packet)
    {
        PacketReceived?.Invoke(packet.Type, packet);
    }
    
    #endregion
    
    #region Helpers

    /// <summary>
    /// Broadcast a payload to call clients using TCP 
    /// </summary>
    public async Task BroadcastTcp(byte[] payload, int? exceptClientId = null)
    {
        foreach (var kvp in _clients.Where(kvp => !exceptClientId.HasValue || kvp.Key != exceptClientId.Value))
        {
            try
            {
                if (kvp.Value.Framed.Connected)
                {
                    await kvp.Value.Framed.SendAsync(payload);
                }
                else
                {
                    Console.WriteLine($"NetServer.BroadcastTcp1 removed client");
                    RemoveClient(kvp.Value);
                }
            }
            catch (IOException)
            {
                Console.WriteLine($"NetServer.BroadcastTcp2 removed client");
                RemoveClient(kvp.Value);
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine($"NetServer.BroadcastTcp3 removed client");
                RemoveClient(kvp.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"NetServer.BroadcastTcp Exception: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Broadcast a payload to all clients using UDP
    /// </summary>
    public void BroadcastUdp(byte[] payload, int? exceptClientId = null)
    {
        foreach (var kvp in _clients.Where(kvp => !exceptClientId.HasValue || kvp.Key != exceptClientId.Value))
        {
            try
            {
                _udp.Send(payload, payload.Length, kvp.Value.UdpEndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"NetServer.BroadcastUdp Exception: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Disconnect a client from the server and handles removal
    /// </summary>
    private async Task DisconnectClientAsync(ClientConnection cc, string reason)
    {
        if (!Running || !_clients.ContainsKey(cc.ClientId))
            return;

        DisconnectPacket packet = new()
        {
            Reason = reason
        };
        await cc.Framed.SendAsync(packet.ToBytes());

        OnClientConnectionLost(cc);
    }

    private void OnClientConnectionLost(ClientConnection cc)
    {
        ClientDisconnected?.Invoke(cc);
        RemoveClient(cc);
    }
    
    /// <summary>
    /// Handles removing a client from the
    /// </summary>
    private void RemoveClient(ClientConnection cc)
    {
        if (!_clients.ContainsKey(cc.ClientId))
            return;

        try
        {
            cc.Framed.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NetServer.RemoveClient Exception: {ex.Message}");
        }

        _clients.Remove(cc.ClientId);
    }
    
    #endregion
}

