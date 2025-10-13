using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractiveObjects : MonoBehaviour
{
    [Header("Configuración Outline")]
    [SerializeField] private Color highlightColor = Color.yellow;

    private Renderer rend;
    private MaterialPropertyBlock propBlock;
    private Color originalColor;
    private bool isPlayerInside = false;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();

        // Obtener color original del outline
        rend.GetPropertyBlock(propBlock);
        originalColor = rend.sharedMaterial.GetColor("_OutlineColor");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            UpdateOutlineColor(highlightColor);
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

    private void UpdateOutlineColor(Color color)
    {
        rend.GetPropertyBlock(propBlock);
        propBlock.SetColor("_OutlineColor", color);
        rend.SetPropertyBlock(propBlock);
    }

    // (Opcional) Visualizador en escena
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isPlayerInside ? Color.yellow : Color.cyan;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);
    }
}

