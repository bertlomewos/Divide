using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game Configuration")]
    [SerializeField] private Bacteria _bacteriaPrefab;
    [SerializeField] private int _petriDishCapacity = 25;

    private int _totalNutrients;
    private int _currentBacteriaCount = 0;
    private List<Bacteria> _bacteriaColony = new List<Bacteria>();
    private int _nutrientsCollected = 0;

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
            SpawnBacteria(startTile);
        }
        else
        {
            Debug.LogError($"Start tile ({GridManager.instance.StartX},{GridManager.instance.StartY}) is blocked or does not exist! Check your LevelData.");
        }
    }

    private void SpawnBacteria(Tile tile)
    {
        if (_currentBacteriaCount >= _petriDishCapacity)
        {
            Debug.Log("Petri dish is full! Game Over!");
            return;
        }

        var newBacteria = Instantiate(_bacteriaPrefab, tile.transform.position, Quaternion.identity);
        newBacteria.MoveToTile(tile);
        tile.SetOccupied(true);
        _bacteriaColony.Add(newBacteria);
        _currentBacteriaCount++;

        CheckForNutrient(tile);

        Debug.Log($"Bacteria count: {_currentBacteriaCount}/{_petriDishCapacity}");
    }

    public void OnTileClicked(Tile clickedTile)
    {
        foreach (var bacteria in _bacteriaColony.ToList())
        {
            if (IsAdjacent(clickedTile, bacteria.currentTile))
            {
                SpawnBacteria(clickedTile);
                return;
            }
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

    private bool IsAdjacent(Tile tile1, Tile tile2)
    {
        return (Mathf.Abs(tile1.x - tile2.x) + Mathf.Abs(tile1.y - tile2.y)) == 1;
    }
}
