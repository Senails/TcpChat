using System.Net;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using DBTypes;
using ChatTypes;
using static MyRSAcoder;


class ChatClient{
    private string _userName = "";
    private KeyRSA? _openkey = null;
    private UdpClient? _udpClient;


    public List<string> UsersList= new List<string>();
    public List<DBMessage> MessagesList = new List<DBMessage>();


    public event Action? onConnect;
    public event Action? onGetDataChat;
    public event Action? onCloseConnection;


    public event Action? onChangeUsersList;
    public event Action? onChangeMessagesList;


    public void Connect(EndPoint serverEndPoint){
        _udpClient = new UdpClient();
        _udpClient.onMessage += MessageHandler;
        _udpClient.onClose += CloseHandler;
        _udpClient.Connect(serverEndPoint);
        GetKeyFromServer();
    }


    private void GetKeyFromServer(){
        Message message = new Message{
            typeMessage = messageType.getKeyRequest
        };

        byte[] req = message.ToBson();

        _udpClient!.SendRequest(req,(byte[] res)=>{
            Message message = BsonSerializer.Deserialize<Message>(res);
            _openkey = BsonSerializer.Deserialize<KeyRSA>(message.data);
            this.onConnect?.Invoke();
        });
    }
    public async Task<bool> Auth(string login ,string password){
        TaskCompletionSource<bool> endEmiter = new TaskCompletionSource<bool>();
        LoginData dat  = new LoginData{
            login = login,
            criptPass = MyRSAcoder.EncodeText(password,_openkey!.num,_openkey.mod)
        };

        Message message = new Message{
            typeMessage = messageType.authMeRequest,
            data = dat.ToBson()
        };

        _udpClient!.SendRequest(message.ToBson(),(byte[] res)=>{
            Message message = BsonSerializer.Deserialize<Message>(res)!;
            bool isSuccess = BitConverter.ToBoolean(message.data);
            endEmiter.SetResult(isSuccess);
        });

        return await endEmiter.Task;
    }
    public async Task<bool> Register(string login ,string password){
        TaskCompletionSource<bool> endEmiter = new TaskCompletionSource<bool>();
        RegisterData dat  = new RegisterData{
            login = login,
            name = login,
            criptPass = MyRSAcoder.EncodeText(password,_openkey!.num,_openkey.mod)
        };
        Message message = new Message{
            typeMessage = messageType.regMeRequest,
            data = dat.ToBson()
        };

        _udpClient!.SendRequest(message.ToBson(),(byte[] res)=>{
            Message message = BsonSerializer.Deserialize<Message>(res)!;
            bool isSuccess = BitConverter.ToBoolean(message.data);
            endEmiter.SetResult(isSuccess);
        });
        return await endEmiter.Task;
    }
    public void GetDataForChat(){
        Message mess = new Message{
            typeMessage = messageType.getPrevInfo,
        };

        _udpClient!.SendRequest(mess.ToBson(),(byte[] res)=>{
            Message mess = BsonSerializer.Deserialize<Message>(res)!;
            if (mess==null) return;
            PrevInfo prevInfo = BsonSerializer.Deserialize<PrevInfo>(mess.data)!;

            foreach(string name in prevInfo.userNames!){
                UsersList.Add(name);
            }
            foreach(DBMessage name in prevInfo.messages!.Reverse()){
                MessagesList.Add(name);
            }

            onGetDataChat?.Invoke();
        });
    }


    private void MessageHandler(byte[] message){
        Message obj = BsonSerializer.Deserialize<Message>(message)!;
        if (obj.data==null) return;

        switch (obj.typeMessage)
        {
            case messageType.newMessage:
                NewMessageHandler(obj.data);
                break;
            case messageType.openChat:
                UserEnterInChatHandler(obj.data);
                break;
            case messageType.leaveFromChat:
                UserLeaveFromChatHandler(obj.data);
                break;
        }
    }
    private void NewMessageHandler(byte[] data){
        DBMessage dbMessage = BsonSerializer.Deserialize<DBMessage>(data)!;
        MessagesList.Add(dbMessage);
        onChangeMessagesList?.Invoke();
    }
    private void UserEnterInChatHandler(byte[] data){
        string name = Encoding.UTF8.GetString(data);
        UsersList.Add(name);
        onChangeUsersList?.Invoke();
        
        DBMessage dbMessage = new DBMessage{
            id = -100,
            date = 0,
            authtor = "",
            text = $"{data} вошел в чатик"
        };

        MessagesList.Add(dbMessage);
        onChangeMessagesList?.Invoke();
    }
    private void UserLeaveFromChatHandler(byte[] data){
        string name = Encoding.UTF8.GetString(data);
        UsersList.Remove(name);
        onChangeUsersList?.Invoke();

        DBMessage dbMessage = new DBMessage{
            id = -100,
            date = 0,
            authtor = "",
            text = $"{data} вышел из чатика"
        };

        MessagesList.Add(dbMessage);
        onChangeMessagesList?.Invoke();
    }


    public void SendMessage(string dataMes){
        Message mess = new Message{
            typeMessage = messageType.iSendMessage,
            data = Encoding.UTF8.GetBytes(dataMes)
        };

        _udpClient!.SendMessage(mess.ToBson());
    }

    void CloseHandler(){
        onCloseConnection?.Invoke();
    }
}