using System.Net.Sockets;

namespace MyRedis.Server
{
    internal class TcpClient
    {
        private readonly TcpListener listener = new(System.Net.IPAddress.Any, 6379);
        listener.Start();
    }
}
