using static AsyncLib;
var d =  dontCloseProcces();


ChatEntiti chatClient = new ChatEntiti();

chatClient.onTryConnect += (status)=>{
    Console.WriteLine(status);
    chatClient.Auth("Senails","rtyrfvrty");
};

chatClient.onTryAuth += (status)=>{
    Console.WriteLine(status);
    chatClient.EnterInChat();
};

chatClient.onEnterInChat += (status)=>{
    Console.WriteLine(status);
    chatClient.sendMessage("пишу сообщение в чатик");
};

chatClient.onNewMessage += (message)=>{
    Console.WriteLine(message.text);
};

chatClient.Connect("localhost",4000);

await d.closeTask;