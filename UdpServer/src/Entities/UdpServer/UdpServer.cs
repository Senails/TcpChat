using System.Net;


class UdpServer{
    private UdpEndPointRouter _endPointRouter;


    public event Action? onStart;
    public event Action? onClose;
    public event Action<UdpConnection>? onConnection;


    public UdpServer(){
        _endPointRouter = new UdpEndPointRouter();
    }


    public void Listen(EndPoint endpoint){
        _endPointRouter.onNewRout+=(UdpRout rout)=>{
            UdpConnection connection = new UdpConnection(rout);
            this.onConnection?.Invoke(connection);
        };
        _endPointRouter.onClose+=()=>{
            this.onClose?.Invoke();
        };

        _endPointRouter.Listen(endpoint);
        this.onStart?.Invoke();
    }

    public void Close(){
        _endPointRouter.Close();
    }
}