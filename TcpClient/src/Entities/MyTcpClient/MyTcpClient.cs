using System.Net.Sockets;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using static MyDateLib;

public class MyTcpClient 
{
    Socket realSocket;
    public event Action<string>? onMessage;
    public event Action? onClose;
    public event Action? onConnect;
    List<MessageWaiter> waitList = new List<MessageWaiter>();


    public MyTcpClient(){
        realSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }


    public async void sendMessage(string message){
        ConnectionMessage willBson = 
        new ConnectionMessage() {
            messageID = getDateMilisec(),
            waitResponse = false,
            data = message
        };

        byte[] messageForServer = willBson.ToBson();
        await internalSendMessage(messageForServer);
    }
    public async void sendRequest(string message, Action<string> onResponse){
        long messID = getDateMilisec();

        ConnectionMessage willBson = 
        new ConnectionMessage() {
            messageID = messID,
            waitResponse = true,
            data = message
        };

        byte[] messageForServer = willBson.ToBson();

        waitList.Add(new MessageWaiter(){
            messageID = messID,
            handler = onResponse
        });
    
        await internalSendMessage(messageForServer);
    }
    public void Close(){
        realSocket.Close();
    }
    
    
    async Task internalSendMessage(byte[] bson){
        Int32 lenght = bson.Length;

        byte[] bufferLenght = BitConverter.GetBytes(lenght);

        await realSocket.SendAsync(bufferLenght);
        await realSocket.SendAsync(bson);
    }
    void onGetFromServer(byte[] bson){
        ConnectionMessage? serverMessage = 
        BsonSerializer.Deserialize<ConnectionMessage>(bson);

        long messID = serverMessage.messageID;
        string? realMessage = serverMessage.data;

        MessageWaiter? needWaiter = 
        waitList.Find((waiter)=>waiter.messageID==messID);

        if (needWaiter!=null){
            waitList.Remove(needWaiter);

            if (needWaiter.handler!=null && realMessage!=null)
            needWaiter.handler(realMessage);
        }else{
            if (realMessage!=null)
            onMessage?.Invoke(realMessage); 
        }
    }
    
    
    void internalClose(){
        realSocket.Close();
        onClose?.Invoke();
    }
    public async void connect(string url, int port){
        await this.realSocket.ConnectAsync(url, port);
        onConnect?.Invoke();

        Thread newTread = new Thread(()=>{
            waitMessages();
        });
        newTread.Start();
    }
    void waitMessages(){
        while(true){
            byte[] bufferLenght = new byte[4];
            int count=1000;
            int size = 0;

            byte[] bufferData = new byte[0];
            try{
                count = realSocket.Receive(bufferLenght);
                size = BitConverter.ToInt32(bufferLenght, 0);

                bufferData = new byte[size];
                count = realSocket.Receive(bufferData);
            }catch{
                internalClose();
                break;
            }

            if (count == 0) {
                internalClose();
                break;
            } 
            onGetFromServer(bufferData);  
        }
    }


    delegate Task AsyncAction();
    record ConnectionMessage(){
        public long messageID { get; init; }
        public bool waitResponse { get; init; }
        public string? data { get; init; } 
    }
    record MessageWaiter(){
        public long messageID { get; init; }
        public Action<string>? handler { get; init; }
    }
}