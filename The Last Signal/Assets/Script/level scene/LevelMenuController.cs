using UnityEngine;

[System.Serializable]
public class LevelEntryData
{
    [Tooltip("ID único del nivel (por ejemplo 0, 1, 2...)")]
    public int levelID;

    [Tooltip("Objeto UI que representa este nivel en el menú")]
    public GameObject entryObject;

    [Tooltip("Nombre opcional para depuración o mostrar en UI")]
    public string displayName;
}

public class LevelMenuController : MonoBehaviour
{
    [Header("Entradas de niveles")]
    public LevelEntryData[] levelEntries;

    [Header("UI de diálogo introductorio")]
    public GameObject Dialog;

    private const string IntroDialogKey = "IntroDialogShown";

    private void Start()
    {
        if (LevelProgressManager.Instance == null)
        {
            Debug.LogError("❌ No se encontró LevelProgressManager en la escena.");
            return;
        }

        UpdateLevelEntries();

        if (!IsIntroDialogShown())
        {
            Debug.Log("🗨️ Mostrando diálogo introductorio...");
            Dialog.SetActive(true);
        }
        else
        {
            Debug.Log("ℹ️ El diálogo ya fue mostrado antes.");
        }
    }

    private void UpdateLevelEntries()
    {
        foreach (var entry in levelEntries)
        {
            if (entry.entryObject == null)
            {
                Debug.LogWarning($"⚠️ El nivel con ID {entry.levelID} no tiene GameObject asignado.");
                continue;
            }

            bool unlocked = LevelProgressManager.Instance.IsLevelUnlocked(entry.levelID);
            entry.entryObject.SetActive(unlocked);
        }
    }

    private bool IsIntroDialogShown()
    {
        return PlayerPrefs.GetInt(IntroDialogKey, 0) == 1;
    }
    // Desbloquear Nivel 1 con  dialogo introductorio
    public void EndDialog()
    {
        PlayerPrefs.SetInt(IntroDialogKey, 1);
        PlayerPrefs.Save();

        
        LevelProgressManager.Instance.UnlockLevel(1);
        Debug.Log("📜 Diálogo introductorio mostrado, nivel 1 desbloqueado.");

        UpdateLevelEntries();
    }
}

