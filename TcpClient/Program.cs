using System.Text.Json;

using MyTypes;

var dontClose = AsyncLib.dontCloseProcces();


ChatClient client = new ChatClient();

client.onTryConnect += (Status status)=>{
    if (status==Status.succes){
        Console.WriteLine("соеденение установлено");

        client.onTryAuth += (Status status)=>{
            if (status==Status.succes){
                Console.WriteLine("авторизация произошла");
            }else{
                Console.WriteLine("авторизация не произошла");
            }
        };

        client.Auth("dsfs2d","fsdfdh2fd1");
    }else{
        Console.WriteLine("ошибка при подключени");
    }
};

client.Connect("localhost",4000);








await dontClose.closeTask;