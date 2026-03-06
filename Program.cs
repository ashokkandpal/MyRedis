using MyRedis.Server;

Console.WriteLine("MyRedis Server Starting...");

TcpServer server = new TcpServer();
server.Start();