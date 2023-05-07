using System.Text;
using System.Net.Sockets;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using static MyDateLib;
using static AsyncLib;

public class MyTcpSocket
{
    public Socket realSocket;
    public event Action<string>? onMessage;
    public event Action<string,Action<string>>? onRequest;
    public event Action? onClose;

    public MyTcpSocket(Socket socket){
        realSocket = socket;
    }
    public async Task sendMessage(string message){
        ConnectionMessage mess = 
        new ConnectionMessage() {
            messageID = getDateMilisec(),
            waitResponse = false,
            data = message
        };

        byte[] bson = mess.ToBson();
        await sendToClient(bson);
    }
    public async void startListen(){
        while(true){
                byte[] bufferLenght = new byte[4];
                int count=1000;
                int size = 0;

                try{
                    count = await realSocket.ReceiveAsync(bufferLenght);
                    size = BitConverter.ToInt32(bufferLenght, 0);
                }catch{
                    Close();
                    break;
                }
                if (count == 0) {
                    Close();
                    break;
                } 

                byte[] bufferData = new byte[size];

                try{
                    count = await realSocket.ReceiveAsync(bufferData);
                }catch{
                    Close();
                    break;
                }

                if (count == 0) {
                    Close();
                    break;
                }
                
                onGetFromClient(bufferData);     
            }
    }
    public void Close(){
        realSocket.Close();
        onClose?.Invoke();;
    }


    async Task sendToClient(byte[] message){
        try{
            Int32 lenght = message.Length;

            byte[] bufferLenght = BitConverter.GetBytes(lenght);

            await realSocket.SendAsync(bufferLenght);
            await realSocket.SendAsync(message);
        }catch{
            Console.WriteLine("ошибка при отправке сообщения");
        }
    }
    void onGetFromClient(byte[] message){
        ConnectionMessage? serverMessage = 
        BsonSerializer.Deserialize<ConnectionMessage>(message);

        if (serverMessage==null) return;
        bool needWait = serverMessage.waitResponse;
        string realMessage = serverMessage.data!;

        if (!needWait) { 
            onMessage?.Invoke(realMessage);
            return;
        }

        long messID = serverMessage.messageID;
        onRequest?.Invoke(realMessage,createResponseAction(messID));
    }

    Action<string> createResponseAction(long messageID){
        Action<string> action = (string message) =>{
            ConnectionMessage mess = 
            new ConnectionMessage() {
                messageID = messageID,
                waitResponse = false,
                data = message
            };

            byte[] messageForClient = mess.ToBson<ConnectionMessage>();

            Task task = sendToClient(messageForClient);
            task.Wait();
        };

        return action;
    }

    record ConnectionMessage(){
        public long messageID { get; init; }
        public bool waitResponse { get; init; }
        public string? data { get; init; } 
    }
}