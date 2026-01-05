using System.Net.Sockets;

namespace Engine.Network;

public class FramedTcp
{
    private readonly TcpClient _tcp;
    private readonly NetworkStream _stream;
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    private readonly byte[] _lenBuffer = new byte[4];

    public bool Connected => _tcp.Connected;
    
    public FramedTcp(TcpClient tcp)
    {
        _tcp = tcp;
        _stream = tcp.GetStream();
    }

    // TODO: ADD CT
    public async Task SendAsync(byte[] payload)
    {
        if (!Connected)
            return;

        await _sendLock.WaitAsync(); // TODO: ADD CT
        try
        {
            // Get amount of bytes to send and create a buffer
            int length = payload.Length;
            byte[] buffer = new byte[4 + length];

            // Copy length and payload into the buffer
            BitConverter.GetBytes(length).CopyTo(buffer, 0);
            payload.CopyTo(buffer, 4);

            // Send over network
            await _stream.WriteAsync(buffer, 0, buffer.Length); // TODO: ADD CT
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FramedTcp.SendAsync Exception: {ex.Message}");
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public async Task<byte[]?> ReceiveAsync(CancellationToken ct)
    {
        // Try to read length bytes
        if (!await ReadExactAsync(_lenBuffer, 4, ct))
            return null;

        // Get length of received data
        int length = BitConverter.ToInt32(_lenBuffer);
        if (length <= 0 || length > 1024 * 1024)
            return null;

        // Read the received data
        byte[] data = new byte[length];
        if (!await ReadExactAsync(data, length, ct))
            return null;

        return data;
    }

    private async Task<bool> ReadExactAsync(byte[] buffer, int count, CancellationToken ct)
    {
        int offset = 0;
        while (offset < count)
        {
            int read;
            try
            {
                read = await _stream.ReadAsync(buffer, offset, count - offset, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FramedTcp.ReadExactAsync Exception: {ex.Message}");
                return false;
            }

            if (read <= 0)
                return false;

            offset += read;
        }

        return true;
    }

    public void Close()
    {
        _stream.Dispose();
        _tcp.Close();
    }
}