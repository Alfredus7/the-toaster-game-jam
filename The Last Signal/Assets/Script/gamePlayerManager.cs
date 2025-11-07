using System.Collections;
using TMPro;
using UnityEngine;

public class GamePlayerManager : MonoBehaviour
{
    public static GamePlayerManager Instance { get; private set; }

    [Header("Jugador")]
    public TopDownMovement playerMovement;

    [Header("UI")]
    public TMP_Text progressText;

    [Header("Reparaciones")]
    [SerializeField] private int totalRepairableObjects = 10;
    private int repairedObjectsCount;

    [Header("Animación barra")]
    public float typingSpeed = 0.02f;

    [Header("Nivel que desbloquea al completar este")]
    public int nextLevelToUnlock = -1;

    [Header("cuadro de dialogo")]
    public GameObject DialogStart;
    public GameObject DialogFinish;
    public GameObject DefeatPanel; // ✅ Nuevo panel de derrota

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        UpdateProgressUIImmediate();
        DialogStart.gameObject.SetActive(true);

        // ✅ Asegurar que el panel de derrota esté oculto al inicio
        if (DefeatPanel != null)
            DefeatPanel.SetActive(false);
    }

    public void ObjectRepaired()
    {
        repairedObjectsCount++;
        StartCoroutine(UpdateProgressUIAnimated());

        if (repairedObjectsCount >= totalRepairableObjects)
            OnLevelComplete();
    }

    private void UpdateProgressUIImmediate()
    {
        int totalCubes = 25;
        int filledCubes = Mathf.RoundToInt((repairedObjectsCount / (float)totalRepairableObjects) * totalCubes);
        float percentage = (repairedObjectsCount / (float)totalRepairableObjects) * 100f;

        string bar = "[" + new string('■', filledCubes) + new string('□', totalCubes - filledCubes) + "]";
        progressText.text = $"{bar}\nConexión reparada un: {percentage:F0}%";
    }

    private IEnumerator UpdateProgressUIAnimated()
    {
        int totalCubes = 25;
        float percentage = (repairedObjectsCount / (float)totalRepairableObjects) * 100f;
        int targetFilled = Mathf.RoundToInt((repairedObjectsCount / (float)totalRepairableObjects) * totalCubes);

        char[] bar = new string('□', totalCubes).ToCharArray();

        for (int i = 0; i < targetFilled; i++)
        {
            bar[i] = '■';
            progressText.text = $"[{new string(bar)}]\nConexión reparada un: {percentage:F0}%";
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void SetPlayerCanMove(bool canMove)
    {
        if (playerMovement != null)
            playerMovement.canMove = canMove;
    }

    public void ResetProgress()
    {
        repairedObjectsCount = 0;
        UpdateProgressUIImmediate();
    }

    // ✅ Nuevo método para manejar derrota del jugador
    public void OnPlayerDefeat()
    {
        Debug.Log("💀 Jugador derrotado");

        // Parar el tiempo del juego
        Time.timeScale = 0f;

        // Mostrar panel de derrota
        if (DefeatPanel != null)
            DefeatPanel.SetActive(true);

        // Desactivar movimiento del jugador
        SetPlayerCanMove(false);
    }

    public void OnLevelComplete()
    {
        Debug.Log("🏁 Nivel completado.");
        DialogFinish.gameObject.SetActive(true);
        if (nextLevelToUnlock >= 0 && LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.UnlockLevel(nextLevelToUnlock);
            Debug.Log($"🔓 Desbloqueando nivel con ID {nextLevelToUnlock}");
        }
    }
}