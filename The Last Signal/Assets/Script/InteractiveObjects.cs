using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class InteractiveObject : MonoBehaviour
{
    [Header("Configuración Outline")]
    [SerializeField] private Color highlightColor = Color.yellow;

    [Header("Evento al Interactuar")]
    public UnityEvent OnInteract;

    [Header("Configuración Reactivación")]
   private float reactivationTime = 10f;

    private Renderer rend;
    private MaterialPropertyBlock propBlock;
    private Color originalColor;
    private bool isPlayerInside = false;
    private InteractionIndicator indicator;
    public bool canInteract = true;

    void Awake()
    {
        // Buscar Renderer
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

        // Buscar el indicador
        indicator = GetComponentInChildren<InteractionIndicator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            if (canInteract)
            {
                SetOutlineColor(highlightColor);
                indicator.ShowIndicator();
            }
            else 
            {
                if (isRestart)
                {
                    indicator.ShowCooldown();
                }
            }
            other.SendMessage("SetInteractable", this, SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            SetOutlineColor(originalColor);
            if (indicator != null)
                indicator.HideIndicator();
        }
    }

    public void Interact()
    {
        if (!isPlayerInside || !canInteract) return;

        // Ejecutar el evento
        canInteract = false;
        SetOutlineColor(originalColor);
        indicator.HideIndicator();
        OnInteract?.Invoke();
    }
    bool isRestart;
    public void ReactivateObject()
    {
        isRestart = true;
        StartCoroutine(ReactivarObjeto());
    }

    private IEnumerator ReactivarObjeto()
    {
        float timer = reactivationTime;

        // Mostrar cooldown inicial
        if (indicator != null && isPlayerInside)
            indicator.ShowCooldown();

        // Actualizar cooldown en tiempo real
        while (timer > 0)
        {
            if (indicator != null && isPlayerInside)
                indicator.CooldownDisplay(timer);

            timer -= Time.deltaTime;
            yield return null;
        }
        isRestart = false;
        // Reactivar objeto
        canInteract = true;
        Debug.Log($"🔄 {gameObject.name} reactivado");

        // Actualizar indicador si el jugador sigue dentro
        if (isPlayerInside)
        {
            SetOutlineColor(highlightColor);
            if (indicator != null)
                indicator.ShowIndicator();
        }
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