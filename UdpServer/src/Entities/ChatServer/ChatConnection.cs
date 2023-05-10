using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using DBTypes;
using ChatTypes;

using static MyDateLib;
using static MyRSAcoder;

class ChatConnection{
    public bool IsAuth = false;
    public bool IsOpen = false;


    private int _userId;
    private string? _userName;
    private KeyRSA? _openRSAkey;
    private KeyRSA? _secretRSAkey;


    private UdpConnection _connection;
    private ChatServer _server;
    public Action<Message>? onWantToSendMessage;
    public Action<ChatConnection>? onLeave;


    public ChatConnection(UdpConnection connection, ChatServer server){
        this._server=server;
        this._connection=connection;

        connection.onMessage += MessageHandler;
        connection.onRequest += RequestHandler;
        connection.onClose += CloseHandler;
    }


    private void MessageHandler(byte[] message){
        Message obj = BsonSerializer.Deserialize<Message>(message);
        switch (obj.typeMessage)
        {
            case messageType.iSendMessage:
                if (obj.data==null) return;
                IWantSendMessage(obj.data);
                break;
        }
    }
    private async void RequestHandler(byte[] message,Action<byte[]> res){
        Message obj = BsonSerializer.Deserialize<Message>(message);

        byte[]? response = new byte[0];
        switch (obj.typeMessage)
        {
            case messageType.getKeyRequest:
                response = GetKeyRequest();
                break;
            case messageType.authMeRequest:
                if (obj.data==null) return;
                response = await AuthMeRequest(obj.data);
                break;
            case messageType.regMeRequest:
                if (obj.data==null) return;
                response = await RegMeRequest(obj.data);
                break;
            case messageType.getPrevInfo:
                response = await GetPrevInfo();
                break;
        }

        Message resMess = new Message(){
            typeMessage = messageType.resultMessage,
            data = response,
        };

        res(resMess.ToBson());

        if (obj.typeMessage != messageType.getPrevInfo) return;
        if (response.Length==0) return;

        IsOpen=true;
        OpenHandler();
    }


    private byte[] GetKeyRequest(){
        if (_openRSAkey==null || _secretRSAkey==null){
            MyRSAcoder.CreateKeys(out _openRSAkey,out _secretRSAkey);
        }
        return _openRSAkey.ToBson();
    }
    private async Task<byte[]> AuthMeRequest(byte[] data){
        try{
            LoginData loginData = BsonSerializer.Deserialize<LoginData>(data);
            string login = loginData.login!;
            string criptedPassword = loginData.criptPass!;

            if (_secretRSAkey==null) return BitConverter.GetBytes(false);
            string Password = MyRSAcoder.DecodeText(criptedPassword,_secretRSAkey.num,_secretRSAkey.mod);

            DBUser user = await MyDataBase.getUser(login);

            bool passwordsMatch = BCrypt.Net.BCrypt.Verify(Password, user.password);

            if (!passwordsMatch) return BitConverter.GetBytes(false);
            _userId = user.id;
            _userName = user.name;
            IsAuth = true;

            return BitConverter.GetBytes(true);
        }catch{
            return BitConverter.GetBytes(false);
        }
    }
    private async Task<byte[]> RegMeRequest(byte[] data){
        try{
            RegisterData regData = BsonSerializer.Deserialize<RegisterData>(data);
            if (regData==null) return BitConverter.GetBytes(false);
            if (_secretRSAkey==null) return BitConverter.GetBytes(false);
            
            string criptedPassword = regData.criptPass!;
            string password = MyRSAcoder.DecodeText(criptedPassword,_secretRSAkey.num,_secretRSAkey.mod);

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            await MyDataBase.addUser(regData.login!,regData.name!,hashedPassword);
            DBUser user = await MyDataBase.getUser(regData.login!);

            _userId = user.id;
            _userName = user.name;
            IsAuth = true;

            return BitConverter.GetBytes(true);
        }catch{
            return BitConverter.GetBytes(false);
        }
    }
    private async Task<byte[]> GetPrevInfo(){
        try{
            DBMessage[] prevMessages = await MyDataBase.getMessages();

            string[] userNames = _server.ConnectionList
            .Where((connection)=>connection.IsAuth && connection.IsOpen)
            .Select((connection)=>connection._userName).ToArray()!;

            PrevInfo previnfo = new PrevInfo{
                userNames = userNames,
                messages = prevMessages
            };
            
            return previnfo.ToBson();
        }catch{
            return new byte[0];
        }
    }
    
    
    private async void IWantSendMessage(byte[] bytes){
        string text = Encoding.UTF8.GetString(bytes);;
        DBMessage messForSend = new DBMessage{
            date = getDateMilisec(),
            authtor = _userName!,
            text = text
        };

        await MyDataBase.addMessage(_userId,text);

        Message messObj = new Message(){
            typeMessage = messageType.newMessage,
            data = messForSend.ToBson()
        };
        onWantToSendMessage?.Invoke(messObj);
    }
    private void OpenHandler(){
        Message mess = new Message(){
            typeMessage = messageType.openChat,
            data = Encoding.UTF8.GetBytes(_userName!)
        };

        onWantToSendMessage?.Invoke(mess);
    }



    private void CloseHandler(){
        this.onLeave?.Invoke(this);
        if (!IsOpen) return;

        Message mess = new Message(){
            typeMessage = messageType.leaveFromChat,
            data = Encoding.UTF8.GetBytes(_userName!)
        };

        onWantToSendMessage?.Invoke(mess);
    }
    public void SendMessage(Message message){
        if (_connection==null) return;
        _connection.SendMessage(message.ToBson());
    }
}