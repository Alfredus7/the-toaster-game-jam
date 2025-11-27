using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    public InteractiveObject currentObject;

    // 🔹 Llamado automáticamente por el objeto interactuable (SendMessage)
    public void SetInteractable(InteractiveObject obj)
    {
        currentObject = obj;
    }

    // 🔹 Llamado desde el Input System (acción "Interact")
    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            Interact();
        }
    }
    //event externo button UI
    public void Interact()
    {
        if (!currentObject) return;
        currentObject.Interact();
    }
}
