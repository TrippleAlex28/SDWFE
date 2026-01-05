using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Xna.Framework;

namespace Engine.Network;

public static class NetSerializationExtensions
{
    public static void Write(this BinaryWriter w, Vector2 value)
    {
        w.Write(value.X);
        w.Write(value.Y);
    }

    public static Vector2 ReadVector2(this BinaryReader r)
    {
        float x = r.ReadSingle();
        float y = r.ReadSingle();
        return new Vector2(x, y);
    }
}

public static class NetworkUtils
{
    public static IPAddress GetServerBindAddress()
    {
        return GetPrefferedOutboundIPv4() ?? IPAddress.Any;
    }
    
    /// <summary>
    /// Returns the IP address that the OS would use to reach the internet
    /// This is what you want to show players so they can connect
    /// </summary>
    /// <returns></returns>
    public static IPAddress? GetPrefferedOutboundIPv4()
    {
        try
        {
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, 0);

            socket.Connect("8.8.8.8", 65530);

            return (socket.LocalEndPoint as IPEndPoint)?.Address;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NetworkUtils.GetPrefferedOutboundIPv4 Exception: {ex.Message}");
            return null;
        }
    }

    public static async Task<string?> GetPublicIPAsync()
    {
        try
        {
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(5);
            string ip = await client.GetStringAsync("https://api.ipify.org"); 
            return string.IsNullOrWhiteSpace(ip) ? null : ip.Trim();
        }
        catch  (Exception ex)
        {
            Console.WriteLine($"NetworkUtils.GetPublicIPAsync Exception: {ex.Message}");
            return null;
        }
    }
}