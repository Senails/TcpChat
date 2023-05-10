using System.Net;



ChatServer server = new ChatServer();
IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4000);
server.Start(endpoint);




await Task.Delay(1000000000);