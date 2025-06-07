using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game Configuration")]
    [SerializeField] private Bacteria _bacteriaPrefab;
    [SerializeField] private float _divisionAnimationDuration = 0.3f;
    [SerializeField] private int _petriDishCapacity = 25;

    private int _totalNutrients;
    private int _currentBacteriaCount = 0;
    private List<Bacteria> _bacteriaColony = new List<Bacteria>();
    private int _nutrientsCollected = 0;
    private bool isExplosionBuffActive = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
       
    }


    void Start()
    {
        _totalNutrients = GridManager.instance.NutrientCount;
        _petriDishCapacity = GridManager.instance.petriDishCap;
        Tile startTile = GridManager.instance.GetTileAtPosition(new Vector2(GridManager.instance.StartX, GridManager.instance.StartY));
        if (startTile != null && startTile.isWalkable)
        {
            var newBacteria = Instantiate(_bacteriaPrefab, startTile.transform.position, Quaternion.identity);
            newBacteria.currentTile = startTile;
            startTile.SetOccupied(true);
            _bacteriaColony.Add(newBacteria);
            _currentBacteriaCount++;

            CheckForNutrient(startTile);
            CheckForExplosionBuff(startTile);
        }
        else
        {
            Debug.LogError($"Start tile ({GridManager.instance.StartX},{GridManager.instance.StartY}) is blocked or does not exist! Check your LevelData.");
        }
    }

    private void SpawnBacteria(Tile targetTile, Bacteria parentBacteria)
    {
        if (_currentBacteriaCount >= _petriDishCapacity)
        {
            Debug.Log("Petri dish is full! Game Over!");
            return;
        }

        var newBacteria = Instantiate(_bacteriaPrefab, parentBacteria.transform.position, Quaternion.identity);

        newBacteria.AnimateSpawn(targetTile, parentBacteria.transform.position, _divisionAnimationDuration);
        parentBacteria.AnimateDivisionShrink(_divisionAnimationDuration);

        targetTile.SetOccupied(true);
        _bacteriaColony.Add(newBacteria);
        _currentBacteriaCount++;

        CheckForNutrient(targetTile);
        CheckForExplosionBuff(targetTile);

        Debug.Log($"Bacteria count: {_currentBacteriaCount}/{_petriDishCapacity}");
    }

    public void OnTileClicked(Tile clickedTile)
    {
        Bacteria parentBacteria = _bacteriaColony.FirstOrDefault(b => IsAdjacent(clickedTile, b.currentTile));

        if (parentBacteria == null) return;

        if (isExplosionBuffActive)
        {
            SpawnBacteria(clickedTile, parentBacteria);
            Explode(clickedTile);
            isExplosionBuffActive = false;
        }
        else
        {
            SpawnBacteria(clickedTile, parentBacteria);
        }
    }

    private void CheckForNutrient(Tile tile)
    {
        if (tile.OccupyingNutrient != null)
        {
            tile.ClearNutrient();
            _nutrientsCollected++;
            Debug.Log($"Nutrient collected! Total: {_nutrientsCollected}/{_totalNutrients}");

            if (_nutrientsCollected >= _totalNutrients)
            {
                Debug.Log("You collected all the nutrients! YOU WIN!");
            }
        }
    }


    private void CheckForExplosionBuff(Tile tile)
    {
        if (tile.OccupyingExplosionBuff != null)
        {
            tile.ClearExplosionBuff();
            isExplosionBuffActive = true;
            Debug.Log("Explosion Buff collected! Next adjacent spawn will explode.");
        }
    }

    private void Explode(Tile centerTile)
    {
        Debug.Log($"Exploding around tile ({centerTile.x}, {centerTile.y})!");
        List<Tile> neighbors = GridManager.instance.GetNeighbourTiles(centerTile, true); 
        foreach (Tile neighbor in neighbors)
        {
            neighbor.ClearWall();
        }
    }


    private bool IsAdjacent(Tile tile1, Tile tile2)
    {
        return (Mathf.Abs(tile1.x - tile2.x) + Mathf.Abs(tile1.y - tile2.y)) == 1;
    }
}
