using UnityEngine;
using UnityEngine.EventSystems;

public class TileInfinite : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public int x, y;
    [SerializeField] private Sprite _baseColor, _offsetColor, _wallColor, _PortalColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;

    public bool isWalkable = true;
    public bool isPortal = false;
    private bool isOccupiedByBacteria = false;
    public bool isOffset = false;
    public NutrientInfinite OccupyingNutrient { get; private set; }
    public ExplosionBuffInfinite OccupyingExplosion { get; private set; }

    public void Init(bool isOffset)
    {
        _renderer.sprite = isOffset ? _offsetColor : _baseColor;
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
        if (isWalkable && !isPortal)
        {
            GameManagerInfinite.instance.OnTileClicked(this);
        }
        if (isWalkable && isPortal)
        {
            GameManagerInfinite.instance.OnPortalTileClicked(this);
        }
        Debug.Log(x + "," + y);
    }

    public void SetOccupied(bool occupied)
    {
        isOccupiedByBacteria = occupied;
        if (occupied)
        {
            //_renderer.color = Color.green;
        }
    }

    public void SetNutrient(NutrientInfinite nutrient)
    {
        if (nutrient != null)
        {
            OccupyingNutrient = nutrient;
            nutrient.transform.position = this.transform.position;
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

    public void ClearExplosion()
    {
        if (OccupyingExplosion != null)
        {
            Destroy(OccupyingExplosion.gameObject);
            OccupyingExplosion = null;
        }
    }

    public void ClearWall()
    {
        if (!isWalkable)
        {
            isWalkable = true;
            _renderer.sprite = this.isOffset ? _offsetColor : _baseColor;
        }
    }

    public void SetExplosion(ExplosionBuffInfinite explosion)
    {
        if (explosion != null)
        {
            OccupyingExplosion = explosion;
            explosion.transform.position = this.transform.position;
        }
    }

    public void SetAsWall()
    {
        isWalkable = false;
        _renderer.sprite = _wallColor;
    }

    public void SetAsPortal()
    {
        isPortal = true;
        _renderer.sprite = _PortalColor;
    }
}