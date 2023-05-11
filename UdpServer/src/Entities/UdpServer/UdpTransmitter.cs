using System.Net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using TypesForUdp;
using static AsyncLib;

class UdpTransmitter{
    private bool _isClosed = false;
    private UdpSocket _socket;
    private object _lockerSendingList = new(); 
    private List<DgramForList> _listForSend = new List<DgramForList>();
    private List<DgramForList> _listForConfirmation = new List<DgramForList>();


    public event Action? onClose;
    public event Action<Dgram,EndPoint>? onGetDgram;


    public UdpTransmitter(){
        _socket = new UdpSocket();
    }


    public void SendDgram(Dgram dgram,EndPoint endpoint){
        dgram.type = DgramInfoType.TransportInfo;
        AddDgramToSendingList(dgram,endpoint);
    }


    private void SendDgramsFromSendingList(){
        lock(_lockerSendingList){
            foreach (DgramForList dg in _listForSend){
                _socket.SendDramm(dg.serializedDgram!,dg.endPoint!);
            }
        }
    }
    private void AddDgramToSendingList(Dgram dgram,EndPoint endpoint){
        DgramForList dgrmForList = new DgramForList{
            dgram = dgram,
            serializedDgram = dgram.ToBson(),
            endPoint = endpoint,
        };

        lock(_lockerSendingList){
            _listForSend.Add(dgrmForList); 
        }  
        setTimeout(()=>{
            lock(_lockerSendingList){
                _listForSend.Remove(dgrmForList);
            }
        },10000);
    }
    private void RemoveDgramFromSendingList(long dgramID,EndPoint endpoint){
        lock(_lockerSendingList){
            DgramForList? dgrmForList = _listForSend.Find((elem)=>
            elem.dgram!.ID == dgramID && 
            elem.endPoint!.Equals(endpoint));

            if (dgrmForList!=null) _listForSend.Remove(dgrmForList);
        }
    }



    private void AddDgramToConfirmationList(Dgram dgrama,EndPoint endpoint){
        DgramForList dgrmForList = new DgramForList{
            dgram = dgrama,
            endPoint = endpoint,
        };
        _listForConfirmation.Add(dgrmForList);
    }
    private bool CheckDgramInConfirmationList(Dgram dgrama,EndPoint endpoint){
        DgramForList? dgrmForList = _listForConfirmation.Find((elem)=>{

            if (dgrama.ID==null || endpoint==null) Console.WriteLine("bag");
            
            return elem.dgram!.ID == dgrama.ID && elem.endPoint!.Equals(endpoint);
        });
        return (dgrmForList!=null);
    }
    private void RemoveDgramFromConfirmationList(Dgram dgrama,EndPoint endpoint){
        DgramForList? dgrmForList = _listForConfirmation.Find((elem)=>{

            if (dgrama.ID==null || endpoint==null) Console.WriteLine("bag");

            
            return elem.dgram!.ID == dgrama.ID && elem.endPoint!.Equals(endpoint);
        });
        if (dgrmForList!=null) _listForConfirmation.Remove(dgrmForList);
    }
    private void SendConfirmationDgram(Dgram dgrama,EndPoint endpoint){
        Dgram myDgram = new Dgram{
            type = DgramInfoType.ConfirmationTransport,
            ID = dgrama.ID,
        };

        _socket.SendDramm(myDgram.ToBson(),endpoint);
    }



    private void WorkOnInfoDgram(Dgram dgrama,EndPoint endpoint){
        bool inList = CheckDgramInConfirmationList(dgrama,endpoint);
        if (!inList) {
            AddDgramToConfirmationList(dgrama,endpoint);
            onGetDgram?.Invoke(dgrama,endpoint);
            setTimeout(()=>{
                RemoveDgramFromConfirmationList(dgrama,endpoint);
            },10000);
        } 
        SendConfirmationDgram(dgrama,endpoint);
    }
    private void WorkOnConfirmationDgram(Dgram dgrama,EndPoint endpoint){
        RemoveDgramFromSendingList((long)dgrama.ID!,endpoint);
    }
    private void WorkOnDgram(Dgram dgrama,EndPoint endpoint){
        if (dgrama.type==DgramInfoType.TransportInfo){
            WorkOnInfoDgram(dgrama,endpoint);
        }else{
            WorkOnConfirmationDgram(dgrama,endpoint);
        }
    }


    private void RunSendingDgram(){
        Thread newThread = new Thread(async()=>{
            while(true){
                if (_isClosed) break;
                SendDgramsFromSendingList();
                await Task.Delay(10);
            }
        });

        newThread.Start();
    }


    public void Listen(EndPoint listenEndPoint){
        _socket.onGetDgram+=(byte[] bytes,EndPoint endpoint)=>{
            Dgram dgrama = BsonSerializer.Deserialize<Dgram>(bytes);
            WorkOnDgram(dgrama,endpoint);
        };
        _socket.onClose += ()=>{
            this.onClose?.Invoke();
        };

        RunSendingDgram();
        _socket.Listen(listenEndPoint);
    }
    public void Close(){
        _socket.Close();
    }
} 