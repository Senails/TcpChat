using System.Net;

using static AsyncLib;


ChatClient client = new ChatClient();
EndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4000);

client.onConnect +=()=>{
    Console.WriteLine("connected");
};




client.Connect(endpoint);

await Task.Delay(1000000000);
//