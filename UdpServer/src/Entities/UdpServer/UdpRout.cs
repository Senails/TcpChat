using System.Net;

using TypesForUdp;
using static AsyncLib;
using static MyDateLib;

class UdpRout{
    private long _lastDgramTime;
    public EndPoint EndPointRout;


    public event Action<Dgram>? onNewDgram;
    public event Action? onClose;


    public event Action<Dgram>? onWantToSendDgram;
    public event Action? onWantToRemoveSelf;


    public UdpRout(EndPoint endpoint){
        EndPointRout = endpoint;
        StartCloseCheker();
    }

    public void SendDgram(Dgram dgrama){
        this.onWantToSendDgram?.Invoke(dgrama);
    }
    public void GiveDataForRoutFromRemoteEndPoint(Dgram dgrama){
        _lastDgramTime = getDateMilisec();
        this.onNewDgram?.Invoke(dgrama);
    }
    public void Close(){
        this.onWantToRemoveSelf?.Invoke();
        this.onClose?.Invoke();
    }


    private void StartCloseCheker(){
        Action? caneler =null;
        caneler = setInterval(()=>{
            long now = getDateMilisec();
            if (now - _lastDgramTime >1000){
                if (caneler!=null) caneler.Invoke();
                Close();
            }
        },1000);
    }
}