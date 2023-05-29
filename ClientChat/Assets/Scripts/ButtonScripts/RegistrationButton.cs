using UnityEngine;
using UnityEngine.EventSystems;

public class RegistrationButton : MonoBehaviour, IPointerClickHandler
{
    // Start is called before the first frame update
    public void OnPointerClick(PointerEventData eventData)
	{
		LoginWindow.Self.registration();
	}
}
