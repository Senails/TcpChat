using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageArray : MonoBehaviour
{
    public GameObject Content;
    public GameObject userMessagePrefab;
    public GameObject eventMessagePrefab;

    // Start is called before the first frame update
    void Start()
    {
        ChatManager.Self.chatClient.onGetDataChat +=()=>{
            UnityMainThread.wkr.AddJob(()=>{
                RenderMessages();
            });
        };
        ChatManager.Self.chatClient.onChangeMessagesList +=()=>{
            UnityMainThread.wkr.AddJob(()=>{
                RenderMessages();
            });
        };
    }

    // Update is called once per frame
    void RenderMessages()
    {
        Content.transform.DetachChildren();
        List<DBTypes.DBMessage> list = ChatManager.Self.chatClient.MessagesList;

        foreach(var message in list){
            GameObject child;
            if (message.id<0){
                child = Object.Instantiate(eventMessagePrefab,Content.transform);
                TMP_Text text = child.GetComponentInChildren<TMP_Text>();
                text.text = message.text;
            }else{
                child = Object.Instantiate(userMessagePrefab,Content.transform);
                TMP_Text text = child.GetComponentInChildren<TMP_Text>();
                text.text = $"{message.authtor} : {message.text}";
            }
        }
    }
}
