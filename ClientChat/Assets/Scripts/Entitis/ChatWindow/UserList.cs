using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UserList : MonoBehaviour
{
    public GameObject Content;
    public GameObject userNameinListPrefab;

    // Start is called before the first frame update
    void Start()
    {
        ChatManager.Self.chatClient.onConnect+=()=>{
            UnityMainThread.wkr.AddJob(()=>{
                RenderUsersNames();
            });
        };

        ChatManager.Self.chatClient.onChangeUsersList+=()=>{
            UnityMainThread.wkr.AddJob(()=>{
                RenderUsersNames();
            });
        };
    }

    // Update is called once per frame
    void RenderUsersNames()
    {
        Content.transform.DetachChildren();
        List<string> list = ChatManager.Self.chatClient.UsersList;

        foreach(string name in list){
            GameObject child = Object.Instantiate(userNameinListPrefab,Content.transform);
            TMP_Text text = child.GetComponentInChildren<TMP_Text>();
            text.text = name;
        }
    }
}
