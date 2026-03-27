using System.Net;
using System.Net.Sockets;
using MyRedis.Commands;
using MyRedis.Core;

namespace MyRedis.Server
{
    public class TcpServer
    {
        private readonly TcpListener _listener;
        private readonly RedisStore _redisStore;
        private readonly CommandHandler _commandHandler;

        public TcpServer(int port = 6379)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _redisStore = new();
            _redisStore.StartExpiryBackgroundTask();
            _commandHandler = new CommandHandler(_redisStore);
        }
        
        public void Start()
        {
            _listener.Start();
            Console.WriteLine("MyRedis Listening on port 6379");

            while (true)
            {
                TcpClient client = _listener.AcceptTcpClient();
                Console.WriteLine("Client Connected");

                Thread thread = new(() => HandleClient(client));
                thread.Start();
            }
        }

        public void Stop() 
        {
            _listener.Stop();
        }

        private void HandleClient(TcpClient client)
        {
            using ClientHandler handler = new(client, _commandHandler);
            handler.Handle();
        }
        
    }
}
