using System.Text;
using System.Net.Sockets;
using System.Text.Json;

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
        ConnectionMessage willJson = 
        new ConnectionMessage() {
            messageID = getDateMilisec(),
            waitResponse = false,
            data = message
        };

        string json = JsonSerializer.Serialize<ConnectionMessage>(willJson);
        await privatSendMessage(json);
    }
    public void startListen(){
        AsyncAction action = async ()=>{
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
                string message;

                try{
                    count = await realSocket.ReceiveAsync(bufferData);
                    message = Encoding.UTF8.GetString(bufferData, 0, count);
                }catch{
                    Close();
                    break;
                }

                if (count == 0) {
                    Close();
                    break;
                }
                
                privatOnMessage(message);     
            }
        };

        action();
    }
    public void Close(){
        realSocket.Close();
        onClose?.Invoke();;
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
        bool needWait = serverMessage.waitResponse;
        string realMessage = serverMessage.data;

        if (!needWait) { 
            onMessage?.Invoke(realMessage);
            return;
        }

        long messID = serverMessage.messageID;
        onRequest?.Invoke(realMessage,createResponseAction(messID));
    }

    Action<string> createResponseAction(long messageID){
        Action<string> action = (string message) =>{
            ConnectionMessage willJson = 
            new ConnectionMessage() {
                messageID = messageID,
                waitResponse = false,
                data = message
            };

            string messageForClient = JsonSerializer.Serialize<ConnectionMessage>(willJson);

            Task task = privatSendMessage(messageForClient);
            task.Wait();
        };

        return action;
    }

    record ConnectionMessage(){
        public long messageID { get; init; }
        public bool waitResponse { get; init; }
        public string data { get; init; } = "";
    }
}