using UnityEngine;
using TMPro;

public class InpunFied : MonoBehaviour
{
    public TMP_Text tmp;
    TMP_InputField loginInput;
    void Start()
    {
        loginInput = transform.GetComponent<TMP_InputField>();
        loginInput.onEndEdit.AddListener(OnEndEdit);
    }

    void OnEndEdit(string text)
    {
        if (Input.GetKey(KeyCode.Return))
        {
            if (text.Length>0){
                ChatManager.Self.SendMessageInChat(text);
            }
            loginInput.text="";
        }
    }
}
