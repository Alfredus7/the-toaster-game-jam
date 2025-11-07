using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public GameObject deathPanel;

    [Header("Time Settings")]
    public float timeLimit = 180f; // 3 minutos por defecto

    [Header("Color Settings")]
    public Color runningColor = Color.white;
    public Color pausedColor = Color.yellow;

    private float timeRemaining;
    private bool isRunning = false;

    void Update()
    {
        if (!isRunning) return;

        // Reduce time
        timeRemaining -= Time.deltaTime;
        UpdateUI();

        // Check if time expired
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            ActivateDefeatPanel();
        }
    }

    void UpdateUI()
    {
        if (!isRunning)
        {
            timerText.color = pausedColor;
        }
        else
        {
            // Tiempo corriendo en color blanco
            timerText.color = runningColor;
        }
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // M�todo para iniciar el temporizador
    public void StartTimer()
    {
        timeRemaining = timeLimit;
        isRunning = true;
        deathPanel.SetActive(false);
        Time.timeScale = 1f;

        // Activar movimiento del jugador
        if (GamePlayerManager.Instance != null)
            GamePlayerManager.Instance.SetPlayerCanMove(true);

        UpdateUI();
    }

    // M�todo para pausar el temporizador (usar en puzzles)
    public void StopTimer()
    {
        isRunning = false;

        // Desactivar movimiento del jugador
        if (GamePlayerManager.Instance != null)
            GamePlayerManager.Instance.SetPlayerCanMove(false);

        UpdateUI(); // Actualizar UI para cambiar color
    }

    // M�todo para reanudar el temporizador (despu�s de puzzles)
    public void PlayTimer()
    {
        isRunning = true;

        // Activar movimiento del jugador
        if (GamePlayerManager.Instance != null)
            GamePlayerManager.Instance.SetPlayerCanMove(true);

        UpdateUI(); // Actualizar UI para cambiar color
    }

    void ActivateDefeatPanel()
    {
        isRunning = false;
        deathPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}