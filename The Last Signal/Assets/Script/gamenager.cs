using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Jugador")]
    public TopDownMovement playerMovement;

    [Header("UI")]
    public TMP_Text progressText;

    [SerializeField] private int totalRepairableObjects = 10;
    private int repairedObjectsCount;

    [Header("Barra animación")]
    public float typingSpeed = 0.02f; // segundos por cubo

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UpdateProgressUIImmediate();
    }

    public void ObjectRepaired()
    {
        repairedObjectsCount++;
        StartCoroutine(UpdateProgressUIAnimated());
    }

    private void UpdateProgressUIImmediate()
    {
        // Mostrar la barra y porcentaje sin animación
        int totalCubes = 25;
        int filledCubes = Mathf.RoundToInt((repairedObjectsCount / (float)totalRepairableObjects) * totalCubes);
        float percentage = (totalRepairableObjects > 0) ? ((float)repairedObjectsCount / totalRepairableObjects) * 100f : 0f;

        string bar = "[";
        for (int i = 0; i < totalCubes; i++)
        {
            bar += i < filledCubes ? "■" : "□";
        }
        bar += "]";
        progressText.text = $"{bar}\nConexión reparada un: {percentage:F0}%";
    }

    private IEnumerator UpdateProgressUIAnimated()
    {
        int totalCubes = 25;
        float percentage = (totalRepairableObjects > 0) ? ((float)repairedObjectsCount / totalRepairableObjects) * 100f : 0f;

        // Barra inicial con cubos vacíos
        char[] barArray = new char[totalCubes];
        for (int i = 0; i < totalCubes; i++)
            barArray[i] = '□';

        progressText.text = $"[{new string(barArray)}]\nConexión reparada un: {percentage:F0}%";

        // Número de cubos que deben llenarse según progreso
        int targetFilledCubes = Mathf.RoundToInt((repairedObjectsCount / (float)totalRepairableObjects) * totalCubes);

        for (int i = 0; i < targetFilledCubes; i++)
        {
            barArray[i] = '■';
            progressText.text = $"[{new string(barArray)}]\nConexión reparada un: {percentage:F0}%";
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
}
