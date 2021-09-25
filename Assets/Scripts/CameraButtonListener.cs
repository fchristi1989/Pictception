using UnityEngine;
using UnityEngine.EventSystems;


// Class attached to buttons to listen for pressing and releasing
public class CameraButtonListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Called when button pressed
    public void OnPointerDown(PointerEventData eventData)
    {
        UIBehaviour uib = FindObjectOfType<UIBehaviour>();
        uib.OnCameraPressed();
    }

    // Called when button released
    public void OnPointerUp(PointerEventData eventData)
    {
        UIBehaviour uib = FindObjectOfType<UIBehaviour>();
        uib.OnCameraReleased();
    }

}