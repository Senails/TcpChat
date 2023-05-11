using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

class UdpSocket{
    private bool _isClosed = false;
    private Socket _udpSocket;


    public event Action onClose;
    public event Action<byte[],EndPoint> onGetDgram = null;


    public UdpSocket(){
        _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }


    public void SendDramm(byte[] Dgram,EndPoint endPoint){
        try{
            _udpSocket.SendTo(Dgram, endPoint);
        }catch{
        }
    }
    

    private void StartLissening(){
        EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

        while (true){
            if (_isClosed) break;

            byte[] data = new byte[60000]; 
            int result;
            try{
                result = _udpSocket.ReceiveFrom(data,ref remoteIp);
            }catch{
                continue;
            }

            onGetDgram?.Invoke(data,remoteIp);
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