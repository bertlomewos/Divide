using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public int x, y;
    [SerializeField] private Color _baseColor, _offsetColor, _wallColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;

    public bool isWalkable = true;
    private bool isOccupiedByBacteria = false;
    public bool isOffset = false;
    public Nutrient OccupyingNutrient { get; private set; }
    public ExplosionBuff OccupyingExplosionBuff { get; private set; }

    public void Init(bool isOffset)
    {
        this.isOffset = isOffset;
        _renderer.color = isOffset ? _offsetColor : _baseColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isWalkable) _highlight.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _highlight.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isWalkable)
        {
            GameManager.instance.OnTileClicked(this);
        }
    }

    public void SetOccupied(bool occupied)
    {
        isOccupiedByBacteria = occupied;
        if (occupied)
        {
            _renderer.color = Color.green;
        }
    }

    public void SetNutrient(Nutrient nutrient)
    {
        if (nutrient != null)       
        {
            OccupyingNutrient = nutrient;
            nutrient.transform.position = this.transform.position;
        }
    }

    public void SetExplosionBuff(ExplosionBuff explosionBuff)
    {
        if (explosionBuff != null)
        {
            OccupyingExplosionBuff = explosionBuff;
            explosionBuff.transform.position = this.transform.position;
        }
    }

    public void ClearNutrient()
    {
        if (OccupyingNutrient != null)
        {
            Destroy(OccupyingNutrient.gameObject);
            OccupyingNutrient = null;
        }
    }

    public void ClearExplosionBuff()
    {
        if (OccupyingExplosionBuff != null)
        {
            Destroy(OccupyingExplosionBuff.gameObject);
            OccupyingExplosionBuff = null;
        }
    }

    public void SetAsWall()
    {
        isWalkable = false;
        _renderer.color = _wallColor;
    }

    public void ClearWall()
    {
        if (!isWalkable)
        {
            isWalkable = true;
            _renderer.color = this.isOffset ? _offsetColor : _baseColor;
        }
    }
}
