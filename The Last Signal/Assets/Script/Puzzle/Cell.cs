using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class Cell : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
{
    public enum CellType { Empty, Start, End, Block }
    public CellType type = CellType.Empty;

    [Header("Solo para Dot")]
    [SerializeField] private Color dotColor; // privado

    private Image image;
    private PuzzleConnectDots puzzle;
    private Color baseColor;

    public void Init(PuzzleConnectDots puzzleRef)
    {
        puzzle = puzzleRef;
        image = GetComponent<Image>();
        baseColor = image.color;

        if (type == CellType.Start || type == CellType.End)
            dotColor = image.color; // Asignar automáticamente desde Image
    }

    public void OnPointerDown(PointerEventData e) => puzzle.StartDrawing(this);
    public void OnPointerEnter(PointerEventData e) => puzzle.ContinueDrawing(this);
    public void OnPointerUp(PointerEventData e) => puzzle.EndDrawing(this);

    public void SetColor(Color c) => image.color = c;
    public void SetImage(Sprite sprite) => image.sprite = sprite;
    public void ResetColor() => image.color = baseColor;

    public Color GetDotColor() => dotColor;
}