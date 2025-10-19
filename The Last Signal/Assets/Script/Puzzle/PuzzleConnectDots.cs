using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class PuzzleConnectDots : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private float neighborThreshold = 1.1f;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private List<GameObject> objectsToActivate = new List<GameObject>();

    [Header("Sprites")]
    [SerializeField] private Sprite On, empty, block, Off;

    private List<Cell> cells = new List<Cell>();
    private bool isDrawing = false;
    private Cell startCell;
    private Color currentColor;
    private Dictionary<Color, List<Cell>> paths = new Dictionary<Color, List<Cell>>();

    private Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    private void Start() => InitializePuzzle();

    private void InitializePuzzle()
    {
        if (cells.Count == 0 && gridParent != null)
            cells = gridParent.Cast<Transform>().Select(t => t.GetComponent<Cell>()).Where(c => c != null).ToList();

        ApplySpritesToCells();
        cells.ForEach(cell => cell.Init(this));
    }

    private void ApplySpritesToCells()
    {
        foreach (Cell cell in cells)
        {
            Image cellImage = cell.GetComponent<Image>();
            if (cellImage == null) continue;

            cellImage.sprite = cell.type switch
            {
                Cell.CellType.DotStart => On,
                Cell.CellType.Block => block,
                Cell.CellType.DotEnd => Off,
                _ => empty
            };
        }
    }

    public void RefreshCells()
    {
        if (gridParent != null)
        {
            InitializePuzzle();
        }
    }

    // ========== LÓGICA DE DIBUJADO ========== //

    public void StartDrawing(Cell cell)
    {
        if (cell.type != Cell.CellType.DotStart) return;

        startCell = cell;
        currentColor = cell.GetDotColor();
        isDrawing = true;

        paths[currentColor] = new List<Cell> { cell };
    }

    public void ContinueDrawing(Cell cell)
    {
        if (!isDrawing || cell.type == Cell.CellType.Block || paths[currentColor].Contains(cell)) return;

        Cell lastCell = paths[currentColor].Last();
        if (!IsValidNeighbor(lastCell, cell)) return;

        if (cell.type == Cell.CellType.Empty)
        {
            cell.SetColor(currentColor);
            cell.SetImage(On);
        }

        paths[currentColor].Add(cell);
    }

    public void EndDrawing(Cell cell)
    {
        if (!isDrawing) return;

        var path = paths[currentColor];
        Cell lastCell = path.Last();

        Cell targetEnd = FindValidEndCell(lastCell);
        if (targetEnd != null)
        {
            targetEnd.SetImage(On);
            path.Add(targetEnd);
            ShowMessage("Se reparó un nodo");
            CheckPuzzleCompletion();
        }
        else
        {
            ResetPuzzle();
            ShowMessage("Falla en la reparación, reintentando...");
        }

        isDrawing = false;
    }

    private Cell FindValidEndCell(Cell lastCell)
    {
        return cells.FirstOrDefault(c =>
            c.type == Cell.CellType.DotEnd &&
            c.GetDotColor() == currentColor &&
            IsValidNeighbor(lastCell, c));
    }

    // ========== LÓGICA DEL PUZZLE ========== //

    private bool IsValidNeighbor(Cell a, Cell b)
    {
        if (paths.ContainsKey(currentColor) && paths[currentColor].Contains(b))
            return true;

        RectTransform ra = a.GetComponent<RectTransform>();
        RectTransform rb = b.GetComponent<RectTransform>();
        Vector2 diff = rb.anchoredPosition - ra.anchoredPosition;
        float cellSize = Mathf.Max(ra.sizeDelta.x, ra.sizeDelta.y) * neighborThreshold;

        return directions.Any(dir =>
            Vector2.Distance(rb.anchoredPosition, ra.anchoredPosition + dir * cellSize) < cellSize * 0.7f);
    }

    private void ResetPuzzle()
    {
        cells.ForEach(c => c.ResetColor());
        ApplySpritesToCells();
        paths.Values.ToList().ForEach(path => path.Clear());
    }

    private void CheckPuzzleCompletion()
    {
        bool allConnected = cells.Where(c => c.type == Cell.CellType.DotStart)
            .All(startCell =>
            {
                Color color = startCell.GetDotColor();
                return paths.ContainsKey(color) &&
                       paths[color].Count >= 2 &&
                       paths[color].Last().type == Cell.CellType.DotEnd &&
                       paths[color].Last().GetDotColor() == color;
            });

        if (allConnected) ShowPuzzleCompleted();
    }

    private void ShowPuzzleCompleted()
    {
        ShowMessage("Reparación completada");
        Invoke(nameof(OnPuzzleCompleted), 1f);
    }

    private void OnPuzzleCompleted()
    {
        objectsToActivate.ForEach(obj => obj.SetActive(true));
        GameManager.Instance?.SetPlayerCanMove(true);
        GameManager.Instance?.ObjectRepaired();
        gameObject.SetActive(false);
    }

    private void ShowMessage(string text)
    {
        if (messageText == null) return;
        messageText.text = text;
        CancelInvoke(nameof(ClearMessage));
        Invoke(nameof(ClearMessage), 1f);
    }

    private void ClearMessage() => messageText.text = "conecta nodos con sus pares";
}