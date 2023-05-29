using System.Net;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;


public class ChatManager : MonoBehaviour
{
    public GameObject LoginWindow;
    public GameObject ErrorWindow;
    public GameObject ChatWindow;
    public GameObject SelectIpWindow;


    public bool IsConnect = false;


    public ChatClient chatClient;
    public static ChatManager Self;

    public void Awake() {
        Self=this;
    }


    public void Connect(string ip){
        try{
            EndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), 6000);

            chatClient = new ChatClient();
            chatClient.onConnect += ()=>{
                showSelectIpWindow(false);
                showLoginWindow(true);
                showChatWindow(true);
            };
            chatClient.onCloseConnection += ()=>{
                showErrorWindow(true);
            };

            chatClient.Connect(endpoint);
        }catch{
            showErrorWindow(true);
        }
    }


    private void showChatWindow(bool show){
        UnityMainThread.wkr.AddJob(()=>{
            if (LoginWindow!=null)
            ChatWindow.SetActive(show);
        });
    }
    private void showSelectIpWindow(bool show){
        UnityMainThread.wkr.AddJob(()=>{
            if (LoginWindow!=null)
            SelectIpWindow.SetActive(show);
        });
    }
    private void showLoginWindow(bool show){
        UnityMainThread.wkr.AddJob(()=>{
            if (LoginWindow!=null)
            LoginWindow.SetActive(show);
        });
    }
    private void showErrorWindow(bool show){
        UnityMainThread.wkr.AddJob(()=>{
            if (LoginWindow!=null)
            ErrorWindow.SetActive(show);
        });
    }



    private void onAuth(){
        chatClient.onGetDataChat += ()=>{
            showLoginWindow(false);
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
        if (chatClient!=null) chatClient.Close();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("Я переподключаюсь");
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
