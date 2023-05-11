using UnityEngine;
using TMPro;

public class UserName : MonoBehaviour
{
    public TMP_Text tmp;
    void Start()
    {
        ChatManager.Self.chatClient.onGetDataChat +=()=>{
            UnityMainThread.wkr.AddJob(()=>{
                tmp.text=ChatManager.Self.chatClient.UsersName;
            });
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
