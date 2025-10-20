using Unity.VisualScripting;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    [Header("Configuración Outline")]
    [SerializeField] private Color highlightColor = Color.yellow;

    [Header("Puzzle o Panel UI")]
    [SerializeField] private GameObject puzzleUI; // ← Asigna aquí el panel que quieres abrir

    private Renderer rend;
    private MaterialPropertyBlock propBlock;
    private Color originalColor;
    private bool isPlayerInside = false;

    void Awake()
    {
        // Buscar Renderer en el padre si no hay en el mismo objeto
        rend = GetComponent<Renderer>();
        if (rend == null)
            rend = GetComponentInParent<Renderer>();

        if (rend == null)
        {
            Debug.LogWarning($"[InteractiveObject] No se encontró Renderer en {name} ni en sus padres.");
            return;
        }

        propBlock = new MaterialPropertyBlock();

        // Obtener color original del outline
        rend.GetPropertyBlock(propBlock);
        if (rend.sharedMaterial.HasProperty("_OutlineColor"))
            originalColor = rend.sharedMaterial.GetColor("_OutlineColor");
        else
            originalColor = Color.white;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            UpdateOutlineColor(highlightColor);
            other.SendMessage("SetInteractable", this, SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            UpdateOutlineColor(originalColor);
        }
    }

    private bool canInteract = true;
    public void Interact()
    {
        if (isPlayerInside)
        {
            if (puzzleUI != null && canInteract)
            {
                canInteract = false;
                GameManager.Instance.SetPlayerCanMove();
                puzzleUI.SetActive(true);
                UpdateOutlineColor(originalColor);
                this.gameObject.SetActive(false); 
            }
            else
            {
                Debug.LogWarning($"[InteractiveObject] No hay puzzle UI asignado en {name}");
            }
        }
    }

    private void UpdateOutlineColor(Color color)
    {
        if (rend == null) return;
        rend.GetPropertyBlock(propBlock);
        propBlock.SetColor("_OutlineColor", color);
        rend.SetPropertyBlock(propBlock);
    }
}
