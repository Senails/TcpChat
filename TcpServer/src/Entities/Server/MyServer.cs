using System.Net;
using System.Net.Sockets;

using static AsyncLib;

public class MyServer 
{
    bool isClosed = false;
    Socket realSocket;
    public event Action? onClose;
    public event Action<MySocket>? onConnection;

    public MyServer(){
        realSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }
    public void Close(){
        isClosed = true;
        realSocket.Close();
        onClose?.Invoke();;
    }
    public void listen(int port){
        AsyncAction action = async ()=>{
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, port);
            realSocket.Bind(ipPoint);
            realSocket.Listen();

            while (true){
                if(isClosed) return;

                try {
                    Socket socket = await realSocket.AcceptAsync();
                    MySocket clientSocket = new MySocket(socket);

                    Task task = Task.Run(()=>{
                        onConnection?.Invoke(clientSocket);
                    });
                }catch{
                    Close();
                }
            }
        };
        
        action().Wait();
    }
}