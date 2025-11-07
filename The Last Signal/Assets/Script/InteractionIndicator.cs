using UnityEngine;
using TMPro;

public class InteractionIndicator : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TextMeshPro indicatorText;

    [Header("Configuraci�n")]
    [SerializeField] private string keyToPress = "E";

    void Start()
    {
        // Ocultar texto al inicio
        if (indicatorText != null)
            indicatorText.gameObject.SetActive(false);
    }

    public void HideIndicator()
    {
        if (indicatorText != null)
            indicatorText.gameObject.SetActive(false);
    }

    public void ShowIndicator()
    {
        if (indicatorText != null)
        {
            indicatorText.gameObject.SetActive(true);
            indicatorText.text = $"[{keyToPress}] Interactuar";
        }
    }

    public void ShowCooldown()
    {
        if (indicatorText != null)
        {
            indicatorText.gameObject.SetActive(true);
        }
    }

    public void CooldownDisplay(float remainingTime)
    {
        if (indicatorText != null)
        {
            indicatorText.text = $"Reactivando: {remainingTime:F1}s";
        }
    }
}