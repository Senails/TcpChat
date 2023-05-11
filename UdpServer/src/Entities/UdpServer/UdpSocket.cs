using System.Net;
using System.Net.Sockets;

class UdpSocket{
    private bool _isClosed = false;
    private Socket _udpSocket;


    public event Action? onClose;
    public event Action<byte[],EndPoint>? onGetDgram = null;


    public UdpSocket(){
        _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }


    public async void SendDramm(byte[] Dgram,EndPoint endPoint){
        try{
            await _udpSocket.SendToAsync(Dgram, endPoint);
        }catch{
        }
    }
    

    private async void StartLissening(){
        EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

        while (true){
            if (_isClosed) break;

            byte[] data = new byte[60000]; 
            SocketReceiveFromResult result;
            try{
                result = await _udpSocket.ReceiveFromAsync(data, remoteIp);
            }catch{
                continue;
            }

            onGetDgram?.Invoke(data,result.RemoteEndPoint);
        }
    }
    public void Listen(EndPoint endPoint){
        _udpSocket.Bind(endPoint);

        Thread newThread = new Thread(()=>{
            StartLissening();
        });
        newThread.Start();
    }
    public void Close(){
        _isClosed=true;
        _udpSocket.Close();
        onClose?.Invoke();
    }
}