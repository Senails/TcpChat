using UnityEngine;
using UnityEngine.EventSystems;

public class SelectServerButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
	{
		SelectServerWindow.Self.Connect();
	}
}
