using System.Text.Json;

using DBTypes;
using MyTypes;

using static MyDateLib;
using static MyRSAcoder;
using static AsyncLib;
using static MyDataBase;

public class ChatConnection{
    public bool isAuth = false;
    public bool isOpen = false;


    int userId;
    string? userName;
    KeyRSA? openRSAkey;
    KeyRSA? secretRSAkey;


    MyTcpSocket clientSocket;
    MyChat chatObject;
    public Action<Message>? onWantToSendMessage;
    public Action<ChatConnection>? onLeave;


    public ChatConnection(MyTcpSocket clientSocket, MyChat chatObject){
        this.chatObject=chatObject;
        this.clientSocket=clientSocket;

        clientSocket.onMessage += messageHandler;
        clientSocket.onRequest += requestHandler;
        clientSocket.onClose += closeHandler;
        setTimeout(()=>{clientSocket.startListen();},100);
    }


    void messageHandler(string mess){
        Message obj = getMessageFromString(mess);
        switch (obj.typeMessage)
        {
            case messageType.iSendMessage:
                if (obj.data==null) return;
                iWantSendMessage(obj.data);
                break;
        }
    }
    async void requestHandler(string mess,Action<string> res){
        Message obj = getMessageFromString(mess);

        string resText="";
        switch (obj.typeMessage)
        {
            case messageType.getKeyRequest:
                resText = getKeyRequest();
                break;
            case messageType.authMeRequest:
                if (obj.data==null) return;
                resText = await authMeRequest(obj.data);
                break;
            case messageType.regMeRequest:
                if (obj.data==null) return;
                resText = await regMeRequest(obj.data);
                break;
            case messageType.getPrevInfo:
                resText = await getPrevInfo();
                break;
        }

        Message resMess = new Message(){
            typeMessage = messageType.resultMessage,
            data = resText,
        };

        string messString = getStringFromMessage(resMess);
        res(messString);

        if (obj.typeMessage != messageType.getPrevInfo) return;
        if (messString=="error") return;

        isOpen=true;
        openHandler();
    }


    string getKeyRequest(){
        if (this.openRSAkey==null || this.secretRSAkey==null){
            createKeys(out this.openRSAkey,out this.secretRSAkey);
        }
        return JsonSerializer.Serialize<KeyRSA>(this.openRSAkey);
    }
    async Task<string> authMeRequest(string data){
        try{
            loginData loginData = JsonSerializer.Deserialize<loginData>(data)!;
            string login = loginData.login!;
            string criptedPassword = loginData.criptedPassword!;

            if (secretRSAkey==null) return "error";
            string Password = decodeText(criptedPassword,secretRSAkey.num,secretRSAkey.mod);

            DBUser user = await getUser(login);


            if (Password!=user.password) return "error";
            this.userId = user.id;
            this.userName = user.name;
            isAuth = true;

            return "ok";
        }catch{
            return "error";
        }
    }
    async Task<string> regMeRequest(string data){
        try{
            registerData regData = JsonSerializer.Deserialize<registerData>(data)!;
            if (regData==null) return "error";
            if (secretRSAkey==null) return "error";
            
            string criptedPassword = regData.criptedPassword!;
            string password = decodeText(criptedPassword,secretRSAkey.num,secretRSAkey.mod);

            await addUser(regData.login!,regData.name!,password);
            DBUser user = await getUser(regData.login!);

            this.userId = user.id;
            this.userName = user.name;
            isAuth = true;

            return "ok";
        }catch{
            return "error";
        }
    }
    async Task<string> getPrevInfo(){
        try{
            DBMessage[] prevMessages = await getMessages();

            string[] userNames = chatObject.ConnectionList
            .Where((connection)=>connection.isAuth && connection.isOpen)
            .Select((connection)=>connection.userName).ToArray()!;

            PrevInfo previnfo = new PrevInfo{
                userNames = userNames,
                messages = prevMessages
            };
            
            string text = JsonSerializer.Serialize<PrevInfo>(previnfo);
            return text;
        }catch{
            return "error";
        }
    }
    
    
    async void iWantSendMessage(string mess){
        DBMessage messForSend = new DBMessage{
            date = getDateMilisec(),
            authtor = userName!,
            text = mess
        };

        string text = JsonSerializer.Serialize<DBMessage>(messForSend);

        await MyDataBase.addMessage(userId,mess);

        Message messObj = new Message(){
            typeMessage = messageType.newMessage,
            data = text
        };
        onWantToSendMessage?.Invoke(messObj);
    }
    void closeHandler(){
        this.onLeave?.Invoke(this);
        if (!isOpen) return;

        Message mess = new Message(){
            typeMessage = messageType.leaveFromChat,
            data = userName
        };

        onWantToSendMessage?.Invoke(mess);
    }
    void openHandler(){
        Message mess = new Message(){
            typeMessage = messageType.openChat,
            data = userName
        };

        onWantToSendMessage?.Invoke(mess);
    }


    Message getMessageFromString(string str){
        Message? obj = JsonSerializer.Deserialize<Message>(str);
        if (obj==null) throw new Exception("фиговый обьект сообщеия");
        return obj;
    }
    string getStringFromMessage(Message mess){
        return JsonSerializer.Serialize(mess);
    }


    public async Task sendMessage(Message messageObject){
        if (clientSocket==null) return;
        string Json = getStringFromMessage(messageObject);
        await clientSocket.sendMessage(Json);
    }
}