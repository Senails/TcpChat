using System.Net;
using UnityEngine;
using System.Threading.Tasks;


using ChatTypes;

public class ChatManager : MonoBehaviour
{
    public GameObject LoginWindow;
    public GameObject ErrorWindow;
    public GameObject ChatWindow;


    private bool _isConnect;


    public ChatClient chatClient;
    public static ChatManager Self;

    public void Awake() {
        EndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4000);
        Self=this;
        Connect(endpoint);
    }


    private void Connect(EndPoint serverEndPoint){
        chatClient = new ChatClient();
        chatClient.onConnect += ()=>{
            showLoginWindow(true);
        };
        chatClient.onCloseConnection += ()=>{
            showErrorWindow(true);
            showLoginWindow(false);
            showChatWindow(false);
        };

        chatClient.Connect(serverEndPoint);
    }
    private void showLoginWindow(bool show){
        UnityMainThread.wkr.AddJob(()=>{
            LoginWindow.SetActive(show);
        });
    }
    private void showErrorWindow(bool show){
        UnityMainThread.wkr.AddJob(()=>{
            ErrorWindow.SetActive(show);
        });
    }
    private void showChatWindow(bool show){
        UnityMainThread.wkr.AddJob(()=>{
            ChatWindow.SetActive(show);
        });
    }


    private void onAuth(){
        chatClient.onGetDataChat += ()=>{
            Debug.Log("onGetDataChat");
            showLoginWindow(false);
            showChatWindow(true);
        };
        chatClient.GetDataForChat();
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
    public void SendMessageInChat(string text){
        chatClient.SendMessage(text);
    }
    

    public void reconnection(){
        Debug.Log("Я я переподключаюсь");
    }
    public void Close(){
        if (chatClient!=null) chatClient.Close();
        Application.Quit();
    }
}

namespace System.Runtime.CompilerServices
{
        internal static class IsExternalInit {}
}
