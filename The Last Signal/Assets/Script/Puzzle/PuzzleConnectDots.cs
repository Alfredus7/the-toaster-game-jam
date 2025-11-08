using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PuzzleConnectDots : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Sprite start, empty, Fails, end, dot, path;
    [SerializeField] private Color InBlankColor = new Color(0f, 1f, 0.53f);

    [Header("Eventos")]
    public UnityEvent OnPuzzleStart, OnPuzzleStartDraw, OnPuzzleFail, OnPuzzleConectNode, OnPuzzleEnd;

    private List<Cell> cells = new List<Cell>();
    private bool isDrawing, isLocked;
    private Cell startCell;
    private Color currentColor;
    private Dictionary<Color, List<Cell>> paths = new Dictionary<Color, List<Cell>>();
    private Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
    private float neighborThreshold = 1.1f;

    // Para el typewriter
    private bool isTypingMessage = false;
    private Coroutine typingCoroutine;
    private float messageTypingSpeed = 0.03f;

    private void OnEnable() => InitializePuzzle();

    private void InitializePuzzle()
    {
        cells = gridParent?.Cast<Transform>()
                       .Select(t => t.GetComponent<Cell>())
                       .Where(c => c != null).ToList() ?? cells;
        ClearMessage();
        ApplySpritesToCells();
        cells.ForEach(cell => cell.Init(this, InBlankColor));
        OnPuzzleStart?.Invoke();
    }

    private void ApplySpritesToCells()
    {
        foreach (Cell cell in cells)
        {
            Image img = cell.GetComponent<Image>();
            if (img == null) continue;

            img.sprite = cell.type switch
            {
                Cell.CellType.Start => start,
                Cell.CellType.Fails => Fails,
                Cell.CellType.End => end,
                Cell.CellType.Dot => dot,
                _ => empty
            };
        }
    }

    public void StartDrawing(Cell cell)
    {
        if (cell.type != Cell.CellType.Start) return;

        startCell = cell;
        currentColor = cell.GetDotColor();
        isDrawing = true;
        paths[currentColor] = new List<Cell> { cell };
        OnPuzzleStartDraw?.Invoke();
    }

    public void ContinueDrawing(Cell cell)
    {
        if (!isDrawing || paths[currentColor].Contains(cell)) return;

        // Lógica para Fails (X)
        if (cell.type == Cell.CellType.Fails)
        {
            if (!cell.HasColor() || cell.GetDotColor() == currentColor)
            {
                FailPath("¡Cortocircuito! Ruta bloqueada");
                return;
            }
        }

        // Lógica para Ends (cuadrados vacíos)
        if (cell.type == Cell.CellType.End)
        {
            if (cell.HasColor() && cell.GetDotColor() != currentColor)
            {
                FailPath("Polaridad incorrecta en nodo final");
                return;
            }
        }

        // Lógica para Dots (puntos)
        if (cell.type == Cell.CellType.Dot)
        {
            if (cell.HasColor() && cell.GetDotColor() != currentColor)
            {
                FailPath("Polaridad incorrecta en punto de paso");
                return;
            }
        }

        Cell lastCell = paths[currentColor].Last();
        if (!IsValidNeighbor(lastCell, cell)) return;

        // Si es End, completar la ruta
        if (cell.type == Cell.CellType.End)
        {
            paths[currentColor].Add(cell);
            cell.SetImage(start);
            EndDrawing(cell);
            return;
        }

        // Para Dots y Empty, continuar dibujando
        if (cell.type == Cell.CellType.Dot || cell.type == Cell.CellType.Empty)
        {
            cell.SetColor(currentColor);
            cell.SetImage(path);
        }

        paths[currentColor].Add(cell);
    }

    public void EndDrawing(Cell cell)
    {
        if (!isDrawing) return;

        if (paths[currentColor].Last().type == Cell.CellType.End)
        {
            CheckPuzzleProgress();
            ShowMessage("¡Conexión establecida!");
            OnPuzzleConectNode?.Invoke();
            isDrawing = false;
        }
        else FailPath("Ruta inválida. Reiniciando...");
    }

    private void FailPath(string msg)
    {
        if (isLocked) return;
        isLocked = true;
        isDrawing = false;
        ShowMessage(msg);
        OnPuzzleFail?.Invoke();
        Invoke(nameof(DelayedResetPuzzle), 0.5f);
    }

    private void DelayedResetPuzzle()
    {
        ResetPuzzle();
        isLocked = false;
    }

    private void ResetPuzzle()
    {
        cells.ForEach(c => c.ResetColor());
        ApplySpritesToCells();
        paths.Clear();
        isDrawing = false;
    }

    private bool IsValidNeighbor(Cell a, Cell b)
    {
        RectTransform ra = a.GetComponent<RectTransform>();
        RectTransform rb = b.GetComponent<RectTransform>();
        float cellSize = Mathf.Max(ra.sizeDelta.x, ra.sizeDelta.y) * neighborThreshold;

        return directions.Any(dir =>
            Vector2.Distance(rb.anchoredPosition, ra.anchoredPosition + dir * cellSize) < cellSize * 0.7f);
    }

    private void CheckPuzzleProgress()
    {
        bool allSourcesConnected = cells
            .Where(c => c.type == Cell.CellType.Start)
            .All(start =>
            {
                Color startColor = start.GetDotColor();
                return paths.ContainsKey(startColor) &&
                       paths[startColor].Any(c => c.type == Cell.CellType.End &&
                       (!c.HasColor() || c.GetDotColor() == startColor));
            });

        if (!allSourcesConnected)
        {
            ShowMessage("Conexión parcial establecida...");
            return;
        }

        bool allDotsConnected = cells
            .Where(c => c.type == Cell.CellType.Dot)
            .All(dot =>
            {
                if (!dot.HasColor())
                {
                    return paths.Values.Any(p => p.Contains(dot));
                }
                else
                {
                    return paths.ContainsKey(dot.GetDotColor()) &&
                           paths[dot.GetDotColor()].Contains(dot);
                }
            });

        if (!allDotsConnected)
        {
            FailPath("Quedaron puntos sin energía");
            return;
        }

        ShowPuzzleCompleted();
    }

    private void ShowPuzzleCompleted()
    {
        ShowMessage("¡Conexión reparada!");
        Invoke(nameof(OnPuzzleCompleted), 1f);
    }

    private void OnPuzzleCompleted()
    {
        OnPuzzleEnd?.Invoke();
        GamePlayerManager.Instance.ObjectRepaired();
        gameObject.SetActive(false);
    }

    private void ShowMessage(string text)
    {
        if (messageText == null || !gameObject.activeInHierarchy) return;
        typeo(text);
        CancelInvoke(nameof(ClearMessage));
        Invoke(nameof(ClearMessage), 1.5f + (text.Length * messageTypingSpeed));
    }

    private void ClearMessage()
    {
        if (messageText == null || !gameObject.activeInHierarchy) return;
        typeo("Conecta los nodos para reparar el circuito");
    }

    public void typeo(string text)
    {
        // Check if game object is active before starting coroutine
        if (!gameObject.activeInHierarchy) return;

        // Cancel typing anterior si existe
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            isTypingMessage = false;
        }

        // Usar la utilidad de typewriter
        typingCoroutine = StartCoroutine(TypewriterUtility.TypeText(
            text,
            messageText,
            messageTypingSpeed,
            (typing) => isTypingMessage = typing
        ));
    }

    public void RefreshCells() => InitializePuzzle();
    public Color GetInBlankColor() => InBlankColor;
}