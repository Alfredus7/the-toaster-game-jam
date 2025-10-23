using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Cargar escena por índice (número en Build Settings)
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // Cargar escena por nombre (debe coincidir exactamente con el nombre en Build Settings)
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Salir del juego (funciona en build, no en editor)
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("El juego se ha cerrado."); // Solo visible en el Editor
    }
}
