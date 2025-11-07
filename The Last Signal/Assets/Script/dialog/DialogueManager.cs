using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Events; // ← necesario para UnityEvent

public class DialogueManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image characterImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Configuración")]
    public DialogueSequence currentDialogue;
    public float typingSpeed = 0.03f; // velocidad de escritura (segundos por letra)
    public float autoDelay = 1f; // tiempo de espera antes de pasar automáticamente (modo auto)

    [Header("Eventos")]
    public UnityEvent OnDialogueEnd; // ← se dispara al finalizar el diálogo

    private int currentIndex = 0;
    private bool isActive = false;
    private bool isTyping = false;
    private bool autoMode = false; // ← modo automático
    private string fullText;
    private Coroutine typingCoroutine;
    private Coroutine autoCoroutine;
    private Material defaultMaterial;


    void Start()
    {
        if (characterImage != null)
            defaultMaterial = characterImage.material;

        if (currentDialogue != null)
        {
            StartDialogue(currentDialogue);
        }
        else
        {
            Debug.LogWarning("No hay diálogo asignado al iniciar.");
        }
    }

    public void StartDialogue(DialogueSequence dialogue)
    {
        currentDialogue = dialogue;
        currentIndex = 0;
        isActive = true;
        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        if (currentDialogue == null || currentDialogue.lines.Length == 0)
        {
            Debug.LogWarning("No hay diálogo asignado.");
            return;
        }

        var line = currentDialogue.lines[currentIndex];

        // Actualiza retrato y nombre
        if (line.character != null)
        {
            characterImage.sprite = line.character.portrait;
            nameText.text = line.character.characterName;
            characterImage.material = line.character.Material != null ? line.character.Material : defaultMaterial;
        }
        else
        {
            nameText.text = "";
            characterImage.sprite = null;
            characterImage.material = defaultMaterial;
        }

        // Escribir texto con efecto
        fullText = line.text;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(fullText));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;

            // Ajusta la pausa según el carácter
            float delay = typingSpeed;

            if (c == '…') delay = typingSpeed * 7f;  // pausa larga
            else if (c == '—' || c == '-') delay = typingSpeed * 5f; // pausa media
            else if (c == '.' || c == '!' || c == '?') delay = typingSpeed * 3f; // pausa corta
            else if (c == ',') delay = typingSpeed * 2f; // pausa muy corta

            yield return new WaitForSeconds(delay);
        }

        isTyping = false;

        // Si el modo automático está activo, inicia el temporizador para pasar de línea
        if (autoMode)
        {
            if (autoCoroutine != null) StopCoroutine(autoCoroutine);
            autoCoroutine = StartCoroutine(AutoAdvance());
        }
    }


    IEnumerator AutoAdvance()
    {
        yield return new WaitForSeconds(autoDelay);
        NextLine();
    }

    public void OnJump(InputValue value)
    {
        if (!isActive) return;
        AdvanceLine(); // ← reutiliza el mismo comportamiento
    }

    // === EVENTO 1: AVANZAR ===
    public void AdvanceLine()
    {
        if (!isActive) return;

        // Si todavía está escribiendo, muestra todo el texto instantáneamente
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogueText.text = fullText;
            isTyping = false;
        }
        else
        {
            NextLine();
        }
    }

    // === EVENTO 2: SALTAR TODO EL DIÁLOGO ===
    public void SkipDialogue()
    {
        if (!isActive) return;

        // Cancela corutinas y cierra todo
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (autoCoroutine != null) StopCoroutine(autoCoroutine);

        EndDialogue();
    }

    // === EVENTO 3: AUTO MODE ===
    public void ToggleAuto()
    {
        autoMode = !autoMode;
        Debug.Log("Modo automático: " + (autoMode ? "Activado" : "Desactivado"));

        // Si está escribiendo y activas el auto, no hace nada hasta terminar de escribir
        // Si ya terminó de escribir, comienza a avanzar automáticamente
        if (autoMode && !isTyping)
        {
            if (autoCoroutine != null) StopCoroutine(autoCoroutine);
            autoCoroutine = StartCoroutine(AutoAdvance());
        }
        else if (!autoMode && autoCoroutine != null)
        {
            StopCoroutine(autoCoroutine);
        }
    }

    public void NextLine()
    {
        currentIndex++;

        if (currentIndex < currentDialogue.lines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isActive = false;
        autoMode = false;
        Debug.Log("Fin del diálogo");

        // 🔔 Dispara el evento para avisar a otros scripts o botones
        OnDialogueEnd?.Invoke();

        this.gameObject.SetActive(false);
        // Aquí puedes ocultar el panel o notificar al GamePlayerManager
    }
}
