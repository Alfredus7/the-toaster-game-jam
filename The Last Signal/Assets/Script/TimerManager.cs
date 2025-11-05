using UnityEngine;
using TMPro;

public class TimerManager: MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public GameObject deathPanel;

    [Header("Time Settings")]
    public float timeLimit = 180f; // 3 minutos por defecto

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
            ActivateDeathPanel();
        }
    }

    void UpdateUI()
    {
        // Format time as 00:00
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Método para iniciar el temporizador
    public void StartTimer()
    {
        timeRemaining = timeLimit;
        isRunning = true;
        deathPanel.SetActive(false);
        Time.timeScale = 1f;

        // Activar movimiento del jugador
        if (GameManager.Instance != null)
            GameManager.Instance.SetPlayerCanMove(true);

        UpdateUI();
    }

    // Método para pausar el temporizador (usar en puzzles)
    public void StopTimer()
    {
        isRunning = false;

        // Desactivar movimiento del jugador
        if (GameManager.Instance != null)
            GameManager.Instance.SetPlayerCanMove(false);
    }

    // Método para reanudar el temporizador (después de puzzles)
    public void PlayTimer()
    {
        isRunning = true;

        // Activar movimiento del jugador
        if (GameManager.Instance != null)
            GameManager.Instance.SetPlayerCanMove(true);
    }

    void ActivateDeathPanel()
    {
        isRunning = false;
        deathPanel.SetActive(true);
        Time.timeScale = 0f;

        // Desactivar movimiento del jugador al morir
        if (GameManager.Instance != null)
            GameManager.Instance.SetPlayerCanMove(false);
    }
}