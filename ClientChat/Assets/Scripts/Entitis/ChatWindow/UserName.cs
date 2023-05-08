using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UserName : MonoBehaviour
{
    public TMP_Text tmp;

    private void Start() {
        ChatManager.Self.chatClient.onEnterInChat+=(s)=>{
            UnityMainThread.wkr.AddJob(()=>{
                tmp.text = ChatManager.Self.chatClient.userName;
            });
        };
    }
}
