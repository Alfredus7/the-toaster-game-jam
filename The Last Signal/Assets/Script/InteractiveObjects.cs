using UnityEngine;
using UnityEngine.Events;

public class InteractiveObject : MonoBehaviour
{
    [Header("Configuración Outline")]
    [SerializeField] private Color highlightColor = Color.yellow;

    [Header("Evento al Interactuar")]
    public UnityEvent OnInteract; // Evento a ejecutar

    private Renderer rend;
    private MaterialPropertyBlock propBlock;
    private Color originalColor;
    private bool isPlayerInside = false;

    public bool canInteract = true;

    void Awake()
    {
        // Buscar Renderer en el objeto o sus padres
        rend = GetComponent<Renderer>() ?? GetComponentInParent<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning($"[InteractiveObject] No se encontró Renderer en {name} ni en sus padres.");
            return;
        }

        propBlock = new MaterialPropertyBlock();
        rend.GetPropertyBlock(propBlock);

        originalColor = rend.sharedMaterial.HasProperty("_OutlineColor")
            ? rend.sharedMaterial.GetColor("_OutlineColor")
            : Color.white;
    }

    void OnTriggerEnter(Collider other)
    {
        if (canInteract && other.CompareTag("Player"))
        {
            isPlayerInside = true;
            SetOutlineColor(highlightColor);
            other.SendMessage("SetInteractable", this, SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (canInteract && other.CompareTag("Player"))
        {
            isPlayerInside = false;
            SetOutlineColor(originalColor);
        }
    }

    public void Interact()
    {
        if (!isPlayerInside || !canInteract) return;
        // Ejecutar el evento en vez de abrir UI
        OnInteract?.Invoke();
        // Opcional: deshabilitar interacción después de usar
        canInteract = false;
        SetOutlineColor(originalColor);
    }

    public void SetCanInteract(bool canInteract)
    {
        this.canInteract = canInteract;
    }

    private void SetOutlineColor(Color color)
    {
        if (rend == null) return;

        rend.GetPropertyBlock(propBlock);
        propBlock.SetColor("_OutlineColor", color);
        rend.SetPropertyBlock(propBlock);
    }
}