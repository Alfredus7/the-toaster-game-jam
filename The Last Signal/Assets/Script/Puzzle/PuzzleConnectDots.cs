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
    private Dictionary<Color, bool> completedPaths = new Dictionary<Color, bool>();
    private Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
    private float neighborThreshold = 1.2f;

    // Para el typewriter
    private bool isTypingMessage = false;
    private Coroutine typingCoroutine;
    private float messageTypingSpeed = 0.03f;

    // Nuevo: Control de mensajes
    private bool showingTemporaryMessage = false;
    private string defaultMessage = "Conecta los nodos para reparar el circuito";

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
        // VERIFICACIÓN CORREGIDA: Si el puzzle está bloqueado o ya hay un path completado para este color
        if (isLocked || !isDrawing && completedPaths.ContainsKey(cell.GetDotColor()) && completedPaths[cell.GetDotColor()])
        {
            ShowTemporaryMessage("¡Este nodo ya está conectado!", 1.5f);
            return;
        }

        if (cell.type != Cell.CellType.Start) return;

        startCell = cell;
        currentColor = cell.GetDotColor();
        isDrawing = true;
        paths[currentColor] = new List<Cell> { cell };
        OnPuzzleStartDraw?.Invoke();
    }

    public void ContinueDrawing(Cell cell)
    {
        // VERIFICACIÓN MEJORADA: No permitir dibujar si el path actual ya está completado
        if (!isDrawing || !paths.ContainsKey(currentColor) || paths[currentColor].Count == 0)
            return;

        // Verificar si este path actual YA ESTÁ COMPLETADO
        if (completedPaths.ContainsKey(currentColor) && completedPaths[currentColor])
        {
            FailPath("¡Este camino ya está completado!");
            return;
        }

        // No permitir dibujar sobre otros Start (excepto el propio)
        if (cell.type == Cell.CellType.Start && cell != startCell)
        {
            FailPath("No puedes pasar por otro nodo de inicio");
            return;
        }

        // NO PERMITIR SOBREESCRIBIR CELDAS QUE YA TIENEN COLOR DE OTRO PATH COMPLETADO
        if (IsCellPartOfCompletedPath(cell))
        {
            FailPath("No puedes pasar por rutas ya completadas");
            return;
        }

        if (paths[currentColor].Contains(cell)) return;

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

        // Para Dots y Empty, continuar dibujando SOLO si no tienen color o tienen el mismo color
        if (cell.type == Cell.CellType.Dot || cell.type == Cell.CellType.Empty)
        {
            // Verificar nuevamente que no sea parte de un path completado
            if (!IsCellPartOfCompletedPath(cell) && (!cell.HasColor() || cell.GetDotColor() == currentColor))
            {
                cell.SetColor(currentColor);
                cell.SetImage(path);
            }
            else
            {
                FailPath("No puedes cruzar rutas ya completadas");
                return;
            }
        }

        paths[currentColor].Add(cell);
    }

    // NUEVO MÉTODO: Verificar si la celda es parte de un path COMPLETADO
    private bool IsCellPartOfCompletedPath(Cell cell)
    {
        foreach (var completedPath in completedPaths)
        {
            if (completedPath.Value && // El path está completado
                paths.ContainsKey(completedPath.Key) &&
                paths[completedPath.Key].Contains(cell))
            {
                return true;
            }
        }
        return false;
    }

    // MÉTODO ACTUALIZADO: Verificar si la celda ya está coloreada por otro path
    private bool IsCellAlreadyColoredByOtherPath(Cell cell)
    {
        // Si la celda no tiene color, no está ocupada
        if (!cell.HasColor()) return false;

        // Si la celda tiene el mismo color que el path actual, no está ocupada por otro
        if (cell.GetDotColor() == currentColor) return false;

        // Si la celda está en el path actual, no está ocupada por otro
        if (paths.ContainsKey(currentColor) && paths[currentColor].Contains(cell)) return false;

        // Si la celda es parte de un path COMPLETADO, está ocupada
        if (IsCellPartOfCompletedPath(cell)) return true;

        // Para celdas Empty y Dot: si tienen color diferente al actual, están ocupadas
        if ((cell.type == Cell.CellType.Empty || cell.type == Cell.CellType.Dot) &&
            cell.HasColor() && cell.GetDotColor() != currentColor)
        {
            return true;
        }

        return false;
    }

    public void EndDrawing(Cell cell)
    {
        // Verificar que hay al menos una celda en el path antes de terminar
        if (!isDrawing || !paths.ContainsKey(currentColor) || paths[currentColor].Count <= 1)
        {
            if (isDrawing)
            {
                FailPath("Ruta demasiado corta");
            }
            return;
        }

        if (paths[currentColor].Last().type == Cell.CellType.End)
        {
            // Marcar este path como completado
            completedPaths[currentColor] = true;
            isDrawing = false;

            // PRIMERO verificar el progreso general antes de mostrar cualquier mensaje
            CheckPuzzleProgress();
        }
        else
        {
            FailPath("Ruta inválida. Reiniciando...");
        }
    }

    private void FailPath(string msg)
    {
        if (isLocked) return;
        isLocked = true;
        isDrawing = false;
        ShowTemporaryMessage(msg, 1.5f);
        OnPuzzleFail?.Invoke();

        // Resetear todo el puzzle después de un delay
        Invoke(nameof(ResetPuzzle), 0.5f);
    }

    private void ResetPuzzle()
    {
        cells.ForEach(c => c.ResetColor());
        ApplySpritesToCells();
        paths.Clear();
        completedPaths.Clear();
        isDrawing = false;
        isLocked = false;
    }

    //// NUEVO MÉTODO: Resetear solo el path actual en lugar de todo el puzzle
    //private void ResetCurrentPath()
    //{
    //    if (paths.ContainsKey(currentColor))
    //    {
    //        // Remover el color solo de las celdas que pertenecen a este path y no están en paths completados
    //        foreach (var cell in paths[currentColor])
    //        {
    //            if (!IsCellPartOfCompletedPath(cell) && cell != startCell)
    //            {
    //                cell.ResetColor();
    //                // Restaurar sprite original basado en el tipo de celda
    //                Image img = cell.GetComponent<Image>();
    //                if (img != null)
    //                {
    //                    img.sprite = cell.type switch
    //                    {
    //                        Cell.CellType.Dot => dot,
    //                        Cell.CellType.Empty => empty,
    //                        _ => img.sprite
    //                    };
    //                }
    //            }
    //        }
    //        paths.Remove(currentColor);
    //    }
    //}

    //private void UnlockPuzzle()
    //{
    //    isLocked = false;
    //}

    //private void ResetPuzzle()
    //{
    //    cells.ForEach(c => c.ResetColor());
    //    ApplySpritesToCells();
    //    paths.Clear();
    //    completedPaths.Clear();
    //    isDrawing = false;
    //}

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
                return completedPaths.ContainsKey(startColor) && completedPaths[startColor];
            });

        if (!allSourcesConnected)
        {
            ShowTemporaryMessage("¡Conexión parcial establecida!", 1.5f);
            OnPuzzleConectNode?.Invoke();
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
        ShowTemporaryMessage("¡Conexión reparada!", 2f);
        OnPuzzleConectNode?.Invoke();
        Invoke(nameof(OnPuzzleCompleted), 1f);
    }

    private void OnPuzzleCompleted()
    {
        OnPuzzleEnd?.Invoke();
        GamePlayerManager.Instance.ObjectRepaired();
        gameObject.SetActive(false);
    }

    // Nuevo método para mensajes temporales
    private void ShowTemporaryMessage(string text, float duration)
    {
        if (messageText == null || !gameObject.activeInHierarchy) return;

        // Cancelar cualquier mensaje temporal anterior
        CancelInvoke(nameof(ClearTemporaryMessage));

        showingTemporaryMessage = true;
        typeo(text);

        // Programar volver al mensaje por defecto
        Invoke(nameof(ClearTemporaryMessage), duration);
    }

    private void ClearTemporaryMessage()
    {
        showingTemporaryMessage = false;
        ShowDefaultMessage();
    }

    private void ShowDefaultMessage()
    {
        if (messageText == null || !gameObject.activeInHierarchy || showingTemporaryMessage) return;
        typeo(defaultMessage);
    }

    private void ClearMessage()
    {
        if (messageText == null || !gameObject.activeInHierarchy) return;
        ShowDefaultMessage();
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