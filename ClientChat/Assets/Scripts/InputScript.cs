using UnityEngine;
using TMPro;

public class InputScript : MonoBehaviour
{   
    private TMP_InputField Self;
    public TMP_InputField nextInput;


    private void Start() {
        Self = transform.GetComponent<TMP_InputField>();
        Self.onEndEdit.AddListener((text)=>{
            if (nextInput!=null) nextInput.ActivateInputField();
        });
    }

}