using System.Text;
using System.Net.Sockets;
using System.Text.Json;

using static MyDateLib;

public class MyClient 
{
    Socket realSocket;
    public event Action<string>? onMessage;
    public event Action? onClose;
    public event Action? onConnect;
    List<MessageWaiter> waitList = new List<MessageWaiter>();


    public MyClient(){
        realSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }


    public async void sendMessage(string message){
        ConnectionMessage willJson = 
        new ConnectionMessage() {
            messageID = getDateMilisec(),
            waitResponse = false,
            data = message
        };

        string messageForServer = JsonSerializer.Serialize<ConnectionMessage>(willJson);
        await privatSendMessage(messageForServer);
    }
    public async void sendRequest(string message, Action<string> onResponse){
        long messID = getDateMilisec();

        ConnectionMessage willJson = 
        new ConnectionMessage() {
            messageID = messID,
            waitResponse = true,
            data = message
        };

        string messageForServer = JsonSerializer.Serialize<ConnectionMessage>(willJson);

        waitList.Add(new MessageWaiter(){
            messageID = messID,
            handler = onResponse
        });
    
        await privatSendMessage(messageForServer);
    }
    async Task privatSendMessage(string message){
        AsyncAction action = async ()=>{

        byte[] bufferMessage = Encoding.UTF8.GetBytes(message);

        Int32 lenght = bufferMessage.Length;

        byte[] bufferLenght = BitConverter.GetBytes(lenght);

        await realSocket.SendAsync(bufferLenght);
        await realSocket.SendAsync(bufferMessage);
        };

        try{
            await action();
        }catch{
            Console.WriteLine("ошибка при отправке сообщения");
        }
    }
    void privatOnMessage(string message){
        ConnectionMessage? serverMessage = 
        JsonSerializer.Deserialize<ConnectionMessage>(message);

        if (serverMessage==null) return;

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


    public void Close(){
        realSocket.Close();
    }
    void privatClose(){
        realSocket.Close();
        onClose?.Invoke();
    }
    public void connect(string url, int port){
        AsyncAction action = async ()=>{
            await this.realSocket.ConnectAsync(url, port);
            onConnect?.Invoke();
        };

        try{
            action().Wait();
            waitMessages();
        }catch{
            Console.WriteLine("Ошибка при подключении к серверу");
        }
    }
    async void waitMessages(){
        while(true){
            byte[] bufferLenght = new byte[4];
            int count=1000;
            int size = 0;

            try{
                count = await realSocket.ReceiveAsync(bufferLenght);
                size = BitConverter.ToInt32(bufferLenght, 0);
            }catch{
                privatClose();
                break;
            }
            if (count == 0) {
                privatClose();
                break;
            } 

            byte[] bufferData = new byte[size];
            string message;

            try{
                count = await realSocket.ReceiveAsync(bufferData);
                message = Encoding.UTF8.GetString(bufferData, 0, count);
            }catch{
                privatClose();
                break;
            }

            if (count == 0) {
                privatClose();
                break;
            }
            
            privatOnMessage(message);  
        }
    }


    delegate Task AsyncAction();
    record ConnectionMessage(){
        public long messageID { get; init; }
        public bool waitResponse { get; init; }
        public string data { get; init; } = "";
    }
    record MessageWaiter(){
        public long messageID { get; init; }
        public Action<string>? handler { get; init; }
    }
}