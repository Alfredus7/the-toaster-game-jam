using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Jugador")]
    public TopDownMovement playerMovement;

    [Header("UI")]
    public TMP_Text progressText;

    [SerializeField] private int totalRepairableObjects;
    private int repairedObjectsCount;

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
        UpdateProgressUI();
    }

    public void ObjectRepaired()
    {
        repairedObjectsCount++;
        UpdateProgressUI();
    }

    private void UpdateProgressUI()
    {
        if (progressText != null)
        {
            if (totalRepairableObjects > 0)
            {
                float percentage = ((float)repairedObjectsCount / totalRepairableObjects) * 100f;
                progressText.text = $"Conexion Repara un: {percentage:F0}%";
            }
            else
            {
                progressText.text = "Conexion Repara un: 0%";
            }
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
        totalRepairableObjects = 0;
        UpdateProgressUI();
    }
}