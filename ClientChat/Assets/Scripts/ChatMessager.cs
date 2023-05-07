using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatMessager : MonoBehaviour
{
    ChatEntiti chatClient;
    public void Start() {
        chatClient = new ChatEntiti();

        chatClient.onTryConnect += (status)=>{
            Debug.Log(status);
            chatClient.Auth("Senails","rtyrfvrty");
        };

        chatClient.onTryAuth += (status)=>{
            Debug.Log(status);
            chatClient.EnterInChat();
        };

        chatClient.onEnterInChat += (status)=>{
            Debug.Log(status);
            chatClient.sendMessage("пишу сообщение в чатик");
        };

        chatClient.onNewMessage += (message)=>{
            Debug.Log(message.text);
        };

        chatClient.Connect("localhost",4000);
    }

}
