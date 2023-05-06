using System.Text.Json;

using MyTypes;

var dontClose = AsyncLib.dontCloseProcces();


ChatEntiti client = new ChatEntiti();

client.onTryConnect += (Status status)=>{
    if (status==Status.succes){
        Console.WriteLine("соеденение установлено");

        client.onTryAuth += (Status status)=>{
            if (status==Status.succes){
                Console.WriteLine("авторизация произошла");

                client.onOpenChat+=(Status status)=>{
                    if (status==Status.succes){
                        Console.WriteLine("предыдущие сообщения получены");
                    }else{
                        Console.WriteLine("предыдущие сообщения не получены");
                    }
                };

                client.enterInChat();
            }else{
                Console.WriteLine("авторизация не произошла");
            }
        };

        client.Auth("dsf54s32d","fsdf2d34h2fd1");
    }else{
        Console.WriteLine("ошибка при подключени");
    }
};

client.Connect("localhost",4000);








await dontClose.closeTask;