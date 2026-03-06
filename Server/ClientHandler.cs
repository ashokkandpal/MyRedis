using System.Net.Sockets;
using MyRedis.Commands;
using MyRedis.Protocol;

namespace MyRedis.Server
{
    public class ClientHandler : IDisposable
    {
        private readonly TcpClient _client;
        private readonly CommandHandler _commandHandler;
        public ClientHandler(TcpClient client, CommandHandler commandHandler)
        {
            _client = client;
            _commandHandler = commandHandler;
        }

        public void Dispose()
        {
            _client.Close();
            _client.Dispose();
        }

        public void Handle()
        {
            NetworkStream stream = _client.GetStream();
            StreamReader reader = new(stream);
            StreamWriter writer = new(stream) { AutoFlush = false };
            RespParser parser = new(reader);

            while (true)
            {
                List<string>? args = parser.Parse();
                
                if (args == null)
                    break;

                string command = String.Join(" ", args);
                string result = _commandHandler.Handle(command);

                string respResponse = RespWriter.WriteResponse(result);

                writer.Write(respResponse);
                writer.Flush();

            }

        }
    }
}
