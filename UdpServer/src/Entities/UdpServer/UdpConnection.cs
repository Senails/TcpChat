using System.Net;
using System.Net.Sockets;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using TypesForUdp;
using static AsyncLib;
using static MyDateLib;

class UdpConnection{
    private UdpRout _rout;
    private object _lockerWaiterList = new(); 
    private List<DgramWaiter> _dgramWaiterList = new List<DgramWaiter>();


    public EndPoint clientEndPoint { get{
        return _rout.EndPointRout;
    }}
    public event Action<byte[]>? onMessage;
    public event Action<byte[],Action<byte[]>>? onRequest;
    public event Action? onClose;


    public UdpConnection(UdpRout rout){
        _rout = rout;
        ListenDataFromRout();
    }


    public void SendMessage(byte[] bytes){
        Dgram newDgrm = new Dgram{
            meanType= DgramMeanType.simpleMessage,
            ID = DateTime.Now.Ticks,
            data = bytes
        };

        _rout.SendDgram(newDgrm);
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
        _rout.SendDgram(newDgrm);

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

        _rout.SendDgram(newDgrm);
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

            _rout.SendDgram(resDgrm);
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
    private void ListenDataFromRout(){
        _rout.onNewDgram+=(Dgram dgrm)=>{
            new Thread(()=>{
                OnDgramHandler(dgrm);
            }).Start();
        };
        Action? cancel=null;
        _rout.onClose+=()=>{
            this.onClose?.Invoke();
            cancel?.Invoke();
        };

        cancel = setInterval(()=>{
            SendILiveMessage();
        },100);
    }


    public void Close(){
        _rout.Close();
    }
}