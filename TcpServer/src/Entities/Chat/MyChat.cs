using MyTypes;

public class MyChat{
    MyTcpServer server = new MyTcpServer();
    public List<ChatConnection> ConnectionList = new List<ChatConnection>();

    public MyChat(){
        MyDataBase.connect("sqlLite.db");
    }
    public void start(int port){
        server.onConnection += (MyTcpSocket clientSocket)=>{
            ChatConnection chatConnection = 
            new ChatConnection(clientSocket,this) {
                onWantToSendMessage = sendMessageEnyone,
                onLeave = removeFromList,
            };

            ConnectionList.Add(chatConnection);
        };
        server.onClose += ()=>{
            Console.WriteLine("сервер чатика перестал работать");
        };

        server.listen(port, ()=>{
            Console.WriteLine("сервер чатика запустился");
        });
    }
    void sendMessageEnyone(Message messageObject){
        var actionList = ConnectionList
        .Where<ChatConnection>((connection)=>connection.isAuth && connection.isOpen)
        .Select<ChatConnection,Action>((connection)=>{
            return ()=>{
                Task task = connection.sendMessage(messageObject);
            };
        });

        Action[] actionArray = actionList.ToArray();
        Parallel.Invoke(actionArray);
    }
    void removeFromList(ChatConnection chatConnection){
        ConnectionList.Remove(chatConnection);
    }

    public void Close(){
        server.Close();
    }
}