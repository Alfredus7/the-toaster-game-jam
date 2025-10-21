using Unity.VisualScripting;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    [Header("Configuración Outline")]
    [SerializeField] private Color highlightColor = Color.yellow;

    [Header("Puzzle o Panel UI")]
    [SerializeField] private GameObject puzzleUI; // Panel a abrir

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

        if (puzzleUI != null)
        {
            canInteract = false;
            puzzleUI.SetActive(true);
            SetOutlineColor(originalColor);
        }
        else
        {
            Debug.LogWarning($"[InteractiveObject] No hay puzzle UI asignado en {name}");
        }
    }

    public void setCanInteract(bool caninteract)
    {
        canInteract = caninteract;
    }

    private void SetOutlineColor(Color color)
    {
        if (rend == null) return;

        rend.GetPropertyBlock(propBlock);
        propBlock.SetColor("_OutlineColor", color);
        rend.SetPropertyBlock(propBlock);
    }
}
