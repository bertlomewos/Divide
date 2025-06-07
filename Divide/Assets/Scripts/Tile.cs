using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public int x, y;
    [SerializeField] private Color _baseColor, _offsetColor, _wallColor, _PortalColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;

    public bool isWalkable = true;
    public bool isPortal = false;
    private bool isOccupiedByBacteria = false;
    public bool isOffset = false;
    public Nutrient OccupyingNutrient { get; private set; }
    public ExplosionBuff OccupyingExplosion {  get; private set; }

    public void Init(bool isOffset)
    {
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
        if (isWalkable && !isPortal)
        {
            GameManager.instance.OnTileClicked(this);
        }
        if (isWalkable && isPortal)
        {
            GameManager.instance.OnPortalTileClicked(this);
        }
        Debug.Log(x + "," + y);
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
            _renderer.color = this.isOffset ? _offsetColor : _baseColor;
        }
    }
    public void SetExplosion(ExplosionBuff explosion)
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
        _renderer.color = _wallColor;
    }
    public void SetAsPortal()
    {
        isPortal = true;
        _renderer.color = _PortalColor;
    }
}
