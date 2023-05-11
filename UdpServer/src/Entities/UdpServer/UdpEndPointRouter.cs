using System.Net;
using System.Net.Sockets;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using TypesForUdp;
using static AsyncLib;

class UdpEndPointRouter{
    private UdpTransmitter _transmitter;
    private List<UdpRout> _listRouts = new List<UdpRout>();


    public event Action? onClose;
    public event Action<UdpRout>? onNewRout;


    public UdpEndPointRouter(){
        _transmitter = new UdpTransmitter();
    }


    private UdpRout? GetUdpRoutFromList(EndPoint endpoint){
        return _listRouts.Find((rt)=>rt.EndPointRout.Equals(endpoint));
    }
    private void OpenNewUdpRout(Dgram dgrama,EndPoint endpoint){
        UdpRout newRout = new UdpRout(endpoint);
        newRout.onWantToSendDgram += (Dgram dgrm)=>{
            _transmitter.SendDgram(dgrm,endpoint);
        };
        newRout.onWantToRemoveSelf += ()=>{
            _listRouts.Remove(newRout);
        };

        _listRouts.Add(newRout);
        this.onNewRout?.Invoke(newRout);
        newRout.GiveDataForRoutFromRemoteEndPoint(dgrama);
    }
    private void WorkOnDgram(Dgram dgrama,EndPoint endpoint){
        UdpRout? rout = GetUdpRoutFromList(endpoint);
        if (rout!=null){
            rout.GiveDataForRoutFromRemoteEndPoint(dgrama);
            return;
        }
        OpenNewUdpRout(dgrama,endpoint);
    }


    public void Listen(EndPoint listenEndPoint){
        _transmitter.onGetDgram += WorkOnDgram;
        _transmitter.onClose+=()=>{
            this.onClose?.Invoke();
        };

        _transmitter.Listen(listenEndPoint);
    }
    public void Close(){
        _transmitter.Close();
    }
}