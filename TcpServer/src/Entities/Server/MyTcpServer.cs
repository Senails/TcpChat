using System.Net;
using System.Net.Sockets;

using static AsyncLib;

public class MyTcpServer 
{
    bool isClosed = false;
    Socket realSocket;
    public event Action? onClose;
    public event Action<MyTcpSocket>? onConnection;

    public MyTcpServer(){
        realSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }
    public void Close(){
        isClosed = true;
        realSocket.Close();
        onClose?.Invoke();;
    }
    public void listen(int port, Action? onStart = null){
        AsyncAction action = async ()=>{
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, port);
            realSocket.Bind(ipPoint);
            realSocket.Listen();

            if (onStart!=null) onStart();
            while (true){
                if(isClosed) return;

                try {
                    Socket socket = await realSocket.AcceptAsync();
                    MyTcpSocket clientSocket = new MyTcpSocket(socket);

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