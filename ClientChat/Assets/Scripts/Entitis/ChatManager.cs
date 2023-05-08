using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

using MyTypes;

public class ChatManager : MonoBehaviour
{
    public GameObject LoginWindow;
    public GameObject ErrorWindow;
    public GameObject ChatWindow;


    bool isConnect;
    public ChatEntiti chatClient;
    public static ChatManager Self;

    public void Awake() {
        Self=this;
        Connect("localhost",4000);
    }

    void Connect(string adress,int port){
        chatClient = new ChatEntiti();
        chatClient.onTryConnect += (status)=>{
            if (status==Status.succes){
                showLoginWindow(true);
            }else{
                showErrorWindow(true);
            }
        };
        chatClient.onCloseConnection += ()=>{
            showErrorWindow(true);
            showLoginWindow(false);
            showChatWindow(false);
        };

        chatClient.Connect(adress,port);
    }
    void showLoginWindow(bool show){
        UnityMainThread.wkr.AddJob(()=>{
            LoginWindow.SetActive(show);
        });
    }
    void showErrorWindow(bool show){
        UnityMainThread.wkr.AddJob(()=>{
            ErrorWindow.SetActive(show);
        });
    }
    void showChatWindow(bool show){
        UnityMainThread.wkr.AddJob(()=>{
            ChatWindow.SetActive(show);
        });
    }



    public async Task<bool> registration(string login,string password){
        bool res = await chatClient.Register(login,password);
        if (res) onAuth();
        return res;
    }
    public async Task<bool> login(string login,string password){
        bool res = await chatClient.Auth(login,password);
        if (res) onAuth();
        return res;
    }
    
    
    void onAuth(){
        showLoginWindow(false);
        showChatWindow(true);
        chatClient.EnterInChat();
    }


    public void SendMessageInChat(string text){
        chatClient.sendMessage(text);
    }
    
    public void reconnection(){
        Debug.Log("Я я переподключаюсь");
    }
    public void Close(){
        if (chatClient!=null) chatClient.Close();
        Application.Quit();
    }

}
