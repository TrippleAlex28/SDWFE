using System.Net;
using System.Net.Sockets;
using Engine.Network.Shared.Packet;
using Engine.Network.Shared.Packet.Packets;

namespace Engine.Network.Client;

public class NetClient
{
    public bool Connected { get; private set; } = false;
    public int ClientId { get; private set; } = -1;
    
    // Events
    public event Action<string>? Disconnected;
    public event Action<PacketType, Packet>? PacketReceived;
    
    // TCP Data
    private TcpClient? _tcp;
    private FramedTcp? _framed;

    // UDP Data
    private UdpClient? _udp;
    private IPEndPoint? _serverUdpEndPoint;
    
    private CancellationTokenSource? _cts;
    
    #region Connection

    public async Task<bool> ConnectAsync(string host, int hostTcpPort = 7777, int localUdpPort = 0)
    {
        try
        {
            // Resolve target IP address
            IPAddress targetIp;
            if (!IPAddress.TryParse(host, out targetIp!))
            {
                IPAddress[] addresses = await Dns.GetHostAddressesAsync(host);
                targetIp = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                           ?? throw new Exception($"No IPv4 address found for host: {host}");
            }
            
            // Setup TCP
            _tcp = new TcpClient(AddressFamily.InterNetwork);
            await _tcp.ConnectAsync(host, hostTcpPort);
            _framed = new FramedTcp(_tcp);

            // Setup UDP
            _udp = new(localUdpPort);
            localUdpPort = ((IPEndPoint)_udp.Client.LocalEndPoint!).Port;

            // Send Connection Request
            ConnectionRequestPacket crPacket = new ConnectionRequestPacket
            {
                ClientName = "Client Name",
                ClientUdpPort = localUdpPort,
            };
            await _framed.SendAsync(crPacket.ToBytes());

            // Read response for either ConnectionAccept or Disconnect
            using CancellationTokenSource tempCts = new CancellationTokenSource();
            tempCts.CancelAfter(TimeSpan.FromSeconds(10));
            
            byte[]? data = await _framed.ReceiveAsync(tempCts.Token);
            if (data == null)
            {
                throw new Exception("Server closed connection before responding");
            }

            Packet packet = PacketRegistry.Deserialize(data);
            switch (packet.Type)
            {
                case PacketType.ConnectionAcceptPacket:
                    StartRunning((ConnectionAcceptPacket)packet, host);
                    return true;
                case PacketType.DisconnectPacket:
                    throw new Exception($"Server refused connection: {((DisconnectPacket)packet).Reason}");
                default:
                    throw new Exception($"Unexpected first packet from server: {packet.Type}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NetClient.ConnectAsync Exception: {ex.Message}");
            CleanupFailedConnect();
            return false;
        }
    }

    public async Task DisconnectAsync(string reason)
    {
        if (_framed == null || !Connected) return;

        try
        {
            if (_framed.Connected)
            {
                DisconnectPacket packet = new DisconnectPacket
                {
                    Reason = reason,
                };
                await _framed.SendAsync(packet.ToBytes());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NetClient.DisconnectAsync Exception: {ex.Message}");
        }
        finally
        {
            Disconnected?.Invoke(reason);
            StopRunning();
        }
    }  
    
    #endregion
    
    #region Loops

    private async Task TcpReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && Connected && _framed!.Connected)
            {
                byte[]? data = await _framed.ReceiveAsync(ct);
                if (data == null)
                    break;
                HandlePacket(data);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NetClient.TcpReceiveLoopAsync Exception: ${ex.Message}");
        }
        finally
        {
            await DisconnectAsync("Server closed the TCP connection");
        }
    }

    private async Task UdpReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                UdpReceiveResult result = await _udp!.ReceiveAsync(ct);
                if (!result.RemoteEndPoint.Equals(_serverUdpEndPoint))
                    continue;

                byte[] data = result.Buffer;

                if (data[0] < 1 || data[0] > 8)
                {
                    Console.WriteLine($"Ignoring UDP datagram: from={result.RemoteEndPoint} - firstByte={data[0]}");
                    continue;
                }

                HandlePacket(data);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NetClient.UdpReceiveLoopAsync Exception: {ex.Message}");
        }
        finally
        {
            await DisconnectAsync("Server closed UDP connection");
        }
    }
    
    #endregion
    
    #region Packets

    public void SendCommandPacket(CommandPacket packet)
    {
        if (_udp == null || _serverUdpEndPoint == null)
            return;

        byte[] payload = packet.ToBytes();
        _udp.Send(payload, payload.Length, _serverUdpEndPoint);
    }
    
    private void HandlePacket(byte[] data)
    {
        Packet packet = PacketRegistry.Deserialize(data);

        switch (packet.Type)
        {
            case PacketType.DisconnectPacket:
                _ = DisconnectAsync(((DisconnectPacket)packet).Reason);
                break;
            case PacketType.ChatPacket:
                PacketReceived?.Invoke(packet.Type, packet);
                break;
            case PacketType.SnapshotPacket:
                PacketReceived?.Invoke(packet.Type, packet);
                break;
            case PacketType.SceneChangePacket:
                PacketReceived?.Invoke(packet.Type, packet);
                break;
            default:
                break;
        }
    }
    
    #endregion
    
    #region Helpers

    private void StartRunning(ConnectionAcceptPacket packet, string host)
    {
        Connected = true;
        
        this.ClientId = packet.ClientId;
        var serverIp = ((IPEndPoint)_tcp!.Client.RemoteEndPoint!).Address;
        this._serverUdpEndPoint = new IPEndPoint(serverIp, packet.ServerUdpPort);

        // Send empty UDP packet to somewhat prevent windows defender from acting up
        byte[] payload = new EmptyUdpPacket().ToBytes();
        _udp!.Send(payload, payload.Length, _serverUdpEndPoint);
        
        _cts = new CancellationTokenSource();
        _ = TcpReceiveLoopAsync(_cts.Token);
        _ = UdpReceiveLoopAsync(_cts.Token);

        // Set the Scene & SceneEpoch to be correct
        if (!GameState.Instance.SessionManager.IsHost)
            GameState.Instance.SwitchSceneClient(packet.CurrentSceneEpoch, packet.CurrentSceneKey, packet.LevelIndex);
    }

    private void StopRunning()
    {
        Connected = false;

        _cts?.Cancel();
        _framed?.Close();
        try { _udp?.Close(); } catch { }

        _framed = null;
        _tcp = null;
        _udp = null;
    }

    private void CleanupFailedConnect()
    {
        _framed?.Close();
        try { _udp?.Dispose(); } catch {}

        _framed =  null;
        _tcp = null;
        _udp = null;
    }
    
    #endregion
}