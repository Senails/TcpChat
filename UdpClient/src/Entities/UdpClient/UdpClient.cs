using System.Net;

using TypesForUdp;
using static AsyncLib;
using static MyDateLib;

class UdpClient{
    private long _lastDgramTime;
    private UdpTransmitter _transmitter;
    private EndPoint? _serverEndPoint;
    private object _lockerWaiterList = new(); 
    private List<DgramWaiter> _dgramWaiterList = new List<DgramWaiter>();
    

    public event Action<byte[]>? onMessage;
    public event Action<byte[],Action<byte[]>>? onRequest;
    public event Action? onClose;


    public UdpClient(){
        _transmitter = new UdpTransmitter();
    }


    public void SendMessage(byte[] bytes){
        Dgram newDgrm = new Dgram{
            meanType= DgramMeanType.simpleMessage,
            ID = DateTime.Now.Ticks,
            data = bytes
        };

        _transmitter.SendDgram(newDgrm,_serverEndPoint!);
    }
    public void SendRequest(byte[] bytes,Action<byte[]> onResponse){
        long DgramID = DateTime.Now.Ticks;

        Dgram newDgrm = new Dgram{
            meanType= DgramMeanType.pequestMessage,
            ID = DgramID,
            data = bytes
        };

        DgramWaiter waiter = new DgramWaiter{
            dgramID = DgramID,
            handler = onResponse
        };

        lock(_lockerWaiterList){
            _dgramWaiterList.Add(waiter);
        }
        _transmitter.SendDgram(newDgrm,_serverEndPoint!);

        setTimeout(()=>{
            lock(_lockerWaiterList){
                _dgramWaiterList.Remove(waiter);
            }
        },5000);
    }
    private void SendILiveMessage(){
        Dgram newDgrm = new Dgram{
            meanType= DgramMeanType.iLiveMessage,
            ID = DateTime.Now.Ticks,
        };

        _transmitter.SendDgram(newDgrm,_serverEndPoint!);
    }


    private DgramWaiter? FindWaiterInList(Dgram dgrm){
        return _dgramWaiterList.Find((wtr)=>wtr.dgramID==dgrm.ID);
    }
    private void OnRequestHandler(Dgram dgrm){
        Action<byte[]> action = (byte[] bytes)=>{
            Dgram resDgrm = new Dgram{
                meanType= DgramMeanType.responseMessage,
                ID = dgrm.ID,
                data = bytes
            };

            _transmitter.SendDgram(resDgrm,_serverEndPoint!);
        };

        this.onRequest?.Invoke(dgrm.data!,action);
    }
    private void OnDgramHandler(Dgram dgrm){
        if (dgrm.meanType == DgramMeanType.iLiveMessage) return;
        if (dgrm.meanType == DgramMeanType.simpleMessage){
            this.onMessage?.Invoke(dgrm.data!);
            return;
        }
        if (dgrm.meanType == DgramMeanType.responseMessage){
            lock(_lockerWaiterList){
                DgramWaiter? waiter = FindWaiterInList(dgrm);
                waiter?.handler!(dgrm.data!);
                if (waiter!=null) _dgramWaiterList.Remove(waiter);
            }
            return;
        }
        OnRequestHandler(dgrm);
    }



    public void Connect(EndPoint serverEndPoint){
        EndPoint myEndPoint = new IPEndPoint(IPAddress.Any, 0);
        this._serverEndPoint=serverEndPoint;
        this._transmitter.onGetDgram += (Dgram dgam, EndPoint endpoint)=>{
            _lastDgramTime = getDateMilisec();
            new Thread(()=>{
                OnDgramHandler(dgam);
            }).Start();
        };
        Action? cancel=null;
        this._transmitter.onClose += ()=>{
            this.onClose?.Invoke();
            cancel?.Invoke();
        };

        cancel = setInterval(()=>{
            SendILiveMessage();
        },100);
        StartCloseCheker();
        _transmitter.Listen(myEndPoint);
    }
    public void Close(){
        _transmitter.Close();
    }
    private void StartCloseCheker(){
        Action? canceler =null;
        canceler = setInterval(()=>{
            long now = getDateMilisec();
            if (now - _lastDgramTime >1000){
                if (canceler!=null) canceler.Invoke();
                Close();
            }
        },1000);
    }
}