// MyServer server = new MyServer();

// server.onConnection += (MySocket clientSocket)=>{
//     Console.WriteLine("получил соеденение");


//     clientSocket.onMessage += (string message)=>{
//         Console.WriteLine($"я получил : сообщение");
//     };

//     clientSocket.onRequest += (string message, Action<string> Response)=>{
//         Console.WriteLine($"я получил : запрос");

//         Response("мой ответ на запрос");

//         Console.WriteLine($"ответил на запрос");
//     };

//     clientSocket.onClose += ()=>{
//         Console.WriteLine("закрыл соеденение");
//     };
//     clientSocket.startListen();
// };

MyChat chatik = new MyChat();
chatik.start(4000);

// server.listen(4000);

