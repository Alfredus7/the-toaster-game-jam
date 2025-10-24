using UnityEngine;
using System.Collections.Generic;

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager Instance { get; private set; }

    private const string ProgressKey = "UnlockedLevels";
    private HashSet<int> unlockedLevels = new HashSet<int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadProgress();
    }

    private void LoadProgress()
    {
        string saved = PlayerPrefs.GetString(ProgressKey, "0"); // Nivel 0 desbloqueado por defecto
        unlockedLevels = new HashSet<int>();

        foreach (string s in saved.Split(','))
        {
            if (int.TryParse(s, out int lvl))
                unlockedLevels.Add(lvl);
        }
    }

    private void SaveProgress()
    {
        string data = string.Join(",", unlockedLevels);
        PlayerPrefs.SetString(ProgressKey, data);
        PlayerPrefs.Save();
    }

    public bool IsLevelUnlocked(int levelID)
    {
        return unlockedLevels.Contains(levelID);
    }

    public void UnlockLevel(int levelID)
    {
        if (!unlockedLevels.Contains(levelID))
        {
            unlockedLevels.Add(levelID);
            SaveProgress();
            Debug.Log($"✅ Nivel {levelID} desbloqueado.");
        }
    }
}
