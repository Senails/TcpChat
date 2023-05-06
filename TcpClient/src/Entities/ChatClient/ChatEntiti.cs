using System.Text.Json;
using DBTypes;
using MyTypes;
using static MyRSAcoder;

public class ChatEntiti {
    string userName = "";
    KeyRSA? openkey = null;
    MyTcpClient? netClient;

    public List<string> UsersList= new List<string>();
    public List<DBMessage> MessagesList = new List<DBMessage>();
    public event Action<Status>? onTryConnect;
    public event Action<Status>? onTryAuth;
    public event Action<Status>? onOpenChat;

    public event Action<string>? onNewMessage;
    public event Action<string>? onUserEnterInChat;
    public event Action<string>? onUserLeaveFromChat;
    public event Action? onCloseConnection;

    public event Action? onChangeUsersList;
    public event Action? onChangeMessagesList;


    public void Connect(string url,int port){
        try{
            netClient = new MyTcpClient();
            netClient.onMessage += messageHandler;
            netClient.onClose += closeHandler;
            netClient.connect(url,port);
            getKeyFromServer();
        }catch{
            onTryConnect?.Invoke(Status.succes);
        }
    }
    public void getKeyFromServer(){
        Message message = new Message{
            typeMessage = messageType.getKeyRequest
        };

        string text = JsonSerializer.Serialize(message);

        netClient!.sendRequest(text,(string res)=>{
            Message message = JsonSerializer.Deserialize<Message>(res)!;
            openkey = JsonSerializer.Deserialize<KeyRSA>(message.data!)!;
            onTryConnect?.Invoke(Status.succes);
        });
    }
    public void Auth(string login ,string password){
        try{
            loginData dat  = new loginData{
                login = login,
                criptedPassword = encodeText(password,openkey!.num,openkey.mod)
            };

            string datatext = JsonSerializer.Serialize<loginData>(dat);

            Message message = new Message{
                typeMessage = messageType.authMeRequest,
                data = datatext
            };

            string text = JsonSerializer.Serialize<Message>(message);

            netClient!.sendRequest(text,(string res)=>{
                Message message = JsonSerializer.Deserialize<Message>(res)!;
                if (message.data=="ok"){
                    onTryAuth?.Invoke(Status.succes);
                }else{
                    onTryAuth?.Invoke(Status.error);
                }
            });
        }catch{
            onTryAuth?.Invoke(Status.error);
        }
    }
    public void Register(string login ,string password){
        try{
            registerData dat  = new registerData{
                login = login,
                name = login,
                criptedPassword = encodeText(password,openkey!.num,openkey.mod)
            };

            string datatext = JsonSerializer.Serialize<registerData>(dat);

            Message message = new Message{
                typeMessage = messageType.regMeRequest,
                data = datatext
            };

            string text = JsonSerializer.Serialize<Message>(message);

            netClient!.sendRequest(text,(string res)=>{
                Message message = JsonSerializer.Deserialize<Message>(res)!;
                if (message.data=="ok"){
                    onTryAuth?.Invoke(Status.succes);
                }else{
                    onTryAuth?.Invoke(Status.error);
                }
            });
        }catch{
            onTryAuth?.Invoke(Status.error);
        }
    }
    public void enterInChat(){
        try{
            Message mess = new Message{
                typeMessage = messageType.getPrevInfo,
            };

            string datatext = JsonSerializer.Serialize<Message>(mess);

            netClient!.sendRequest(datatext,(string res)=>{
                Message mess = JsonSerializer.Deserialize<Message>(res)!;
                if (mess==null) return;
                PrevInfo prevInfo = JsonSerializer.Deserialize<PrevInfo>(mess.data!)!;

                UsersList.Add(this.userName);
                foreach(string name in prevInfo.userNames!){
                    UsersList.Add(name);
                }

                foreach(DBMessage name in prevInfo.messages!.Reverse()){
                    MessagesList.Add(name);
                }

                onOpenChat?.Invoke(Status.succes);
            });
        }catch{
            onOpenChat?.Invoke(Status.error);
        }
    }


    void messageHandler(string message){
        Message obj = JsonSerializer.Deserialize<Message>(message)!;
        if (obj.data==null) return;

        switch (obj.typeMessage)
        {
            case messageType.newMessage:
                newMessageHandler(obj.data);
                break;
            case messageType.openChat:
                openChatHandler(obj.data);
                break;
            case messageType.leaveFromChat:
                leaveFromChatHandler(obj.data);
                break;
        }
    }

    void newMessageHandler(string data){}
    void openChatHandler(string data){}
    void leaveFromChatHandler(string data){}

    void closeHandler(){
        onCloseConnection?.Invoke();
    }
}