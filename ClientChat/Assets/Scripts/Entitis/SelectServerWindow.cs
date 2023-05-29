using UnityEngine;
using TMPro;

public class SelectServerWindow : MonoBehaviour
{
    public static SelectServerWindow Self;
    public TMP_InputField ipInput;


    private bool isTry = false;

    public void Start()
    {
        Self=this;
    }

    public void Connect(){
        if (isTry) return;
        ChatManager.Self.Connect(ipInput.text);
        isTry = true;
    }
}
