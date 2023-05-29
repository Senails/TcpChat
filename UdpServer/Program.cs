using System.Net;


using static AsyncLib;

dontCloseRecord r = dontCloseProcces();

ChatServer server = new ChatServer();
IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 6000);
// IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4000);
server.Start(endpoint);




await r.closeTask;