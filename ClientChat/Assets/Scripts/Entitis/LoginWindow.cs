using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class LoginWindow : MonoBehaviour
{
    static public LoginWindow Self;

    public TMP_Text tmp;
    public GameObject loginInputObj;
    public GameObject passwordInputObj;
    TMP_InputField loginInput;
    TMP_InputField passwordInput;

    bool sendingData = false;

    void Start()
    {
        Self=this;
        loginInput = loginInputObj.GetComponent<TMP_InputField>();
        passwordInput = passwordInputObj.GetComponent<TMP_InputField>();
    }

    public async void login(){
        if (sendingData) return;
        sendingData = true;
        bool res = await ChatManager.Self.login(loginInput.text,passwordInput.text);
        if (res){
            showErrorText("");
        }else{
            showErrorText("пользователь не найден");
        }
        sendingData = false;
    }
    public async void registration(){
        if (sendingData) return;
        sendingData = true;
        if (validateInputs()){
            bool res = await ChatManager.Self.registration(loginInput.text,passwordInput.text);
            if (res){
                showErrorText("");
            }else{
                showErrorText("похоже этот логин занят");
            }
        }else{
            showErrorText("используйте хотябы 6 символов");
        }
        sendingData = false;
    }

    bool validateInputs(){
       return (loginInput.text.Length>=6 && passwordInput.text.Length>=6);
    }

    public void showErrorText(string mesage){
        tmp.text = mesage;
    }
}
