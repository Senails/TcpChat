
using System.Text.Json;
using DBTypes;
using MyTypes;
using static MyRSAcoder;

public class ChatClient {
    bool isAuth = false;
    KeyRSA? openkey = null;
    MyClient? netClient;

    public List<string> peopleNameList= new List<string>();
    public List<DBMessage> listOfMessages = new List<DBMessage>();
    public event Action<Status>? onTryConnect;
    public event Action<Status>? onTryAuth;
    public event Action<string>? onNewMessage;
    public event Action<string>? onEnterInChat;
    public event Action<string>? onLeaveFromChat;
    public event Action? onCloseConnection;

    public void Connect(string url,int port){
        try{
            netClient = new MyClient();
            netClient.onMessage += messageHandler;
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

    
    void messageHandler(string message){

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
        Message message = new Message{
            typeMessage = messageType.regMeRequest,
            data = ""
        };

        string text = JsonSerializer.Serialize(message);
    }


    public void getPrevInfo(){}
}