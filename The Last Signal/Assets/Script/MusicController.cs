using UnityEngine;
using System.Collections;

public class MusicController : MonoBehaviour
{
    [Header("Configuraci�n de M�sica")]
    public AudioSource musicSource;
    public float fadeDuration = 2.0f;

    private float originalVolume;
    private Coroutine currentFadeCoroutine;

    void Start()
    {
        if (musicSource != null)
        {
            originalVolume = musicSource.volume;
            musicSource.volume = 0f; // Empezar con volumen en 0
        }
    }

    // M�todo para iniciar m�sica con fade in suave
    public void PlayMusic()
    {
        if (musicSource == null) return;

        // Detener fade actual si existe
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        currentFadeCoroutine = StartCoroutine(FadeInCoroutine());
    }

    // M�todo para detener m�sica con fade out suave
    public void StopMusic()
    {
        if (musicSource == null || !musicSource.isPlaying) return;

        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        currentFadeCoroutine = StartCoroutine(FadeOutCoroutine());
    }

    // Corrutina para Fade In suave
    private IEnumerator FadeInCoroutine()
    {
        if (!musicSource.isPlaying)
        {
            musicSource.volume = 0f;
            musicSource.Play();
        }

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, originalVolume, elapsedTime / fadeDuration);
            yield return null;
        }

        musicSource.volume = originalVolume;
        currentFadeCoroutine = null;
    }

    // Corrutina para Fade Out suave
    private IEnumerator FadeOutCoroutine()
    {
        float elapsedTime = 0f;
        float startVolume = musicSource.volume;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();
        currentFadeCoroutine = null;
    }
}