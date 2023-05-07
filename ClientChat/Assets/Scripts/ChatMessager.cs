using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatMessager : MonoBehaviour
{
    ChatEntiti chat;
    private void Start() {
        chat = new ChatEntiti();

        chat.onTryConnect+= (status)=>{
            Debug.Log(status);
        };


        chat.Connect("localhost",4000);
    }

}
