using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public int x, y;
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;

    private bool isOccupied = false;
    public Nutrient OccupyingNutrient { get; private set; } // Reference to the nutrient on this tile

    public void Init(bool isOffset)
    {
        _renderer.color = isOffset ? _offsetColor : _baseColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _highlight.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _highlight.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Let the GameManager handle the logic of what happens when a tile is clicked.
        GameManager.instance.OnTileClicked(this);
    }

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
        // Optionally change color if it's occupied by bacteria
        if (occupied)
        {
            _renderer.color = Color.green;
        }
    }

    // Method to place a nutrient on this tile
    public void SetNutrient(Nutrient nutrient)
    {
        if (nutrient != null)
        {
            OccupyingNutrient = nutrient;
            nutrient.transform.position = this.transform.position;
        }
    }

    // Method to remove the nutrient from this tile (when it's "eaten")
    public void ClearNutrient()
    {
        if (OccupyingNutrient != null)
        {
            Destroy(OccupyingNutrient.gameObject);
            OccupyingNutrient = null;
        }
    }
}
