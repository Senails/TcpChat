using System.Net;
using ChatTypes;


class ChatServer{
    private UdpServer _server = new UdpServer();
    public List<ChatConnection> ConnectionList = new List<ChatConnection>();


    public ChatServer(){
        MyDataBase.connect("sqlLite.db");
    }


    public void Start(EndPoint endpoint){
        _server.onConnection += (UdpConnection connection)=>{
            ChatConnection chatConnection = 
            new ChatConnection(connection,this) {
                onWantToSendMessage = sendMessageEnyone,
                onLeave = removeFromList,
            };

            ConnectionList.Add(chatConnection);
        };
        _server.onClose += ()=>{
            Console.WriteLine("сервер чатика перестал работать");
        };
        _server.onStart += ()=>{
            Console.WriteLine("сервер чатика начал работать");
        };

        _server.Listen(endpoint);
    }
    private void sendMessageEnyone(Message message){
        var actionList = ConnectionList
        .Where<ChatConnection>((connection)=>connection.IsAuth && connection.IsOpen)
        .Select<ChatConnection,Action>((connection)=>{
            return ()=>{
                connection.SendMessage(message);
            };
        });

        Action[] actionArray = actionList.ToArray();
        Parallel.Invoke(actionArray);
    }
    private void removeFromList(ChatConnection chatConnection){
        ConnectionList.Remove(chatConnection);
    }

    public void Close(){
        _server.Close();
    }
}