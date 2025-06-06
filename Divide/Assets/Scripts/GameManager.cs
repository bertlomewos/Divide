using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private Bacteria _bacteriaPrefab;
    [SerializeField] private int _petriDishCapacity = 20;
    [SerializeField] private int _totalNutrients; // Will be set by GridManager

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
        // Get total nutrients from GridManager after it has spawned them
        _totalNutrients = FindObjectOfType<GridManager>()._nutrientCount;

        // Start the game by spawning the first bacteria
        Tile startTile = GridManager.instance.GetTileAtPosition(new Vector2(5, 5)); // Example start
        if (startTile != null)
        {
            SpawnBacteria(startTile);
        }
    }

    private void SpawnBacteria(Tile tile)
    {
        if (_currentBacteriaCount >= _petriDishCapacity)
        {
            Debug.Log("Petri dish is full! Game Over!");
            // Add proper game over UI/logic here
            return;
        }

        var newBacteria = Instantiate(_bacteriaPrefab, tile.transform.position, Quaternion.identity);
        newBacteria.MoveToTile(tile);
        tile.SetOccupied(true);
        _bacteriaColony.Add(newBacteria);
        _currentBacteriaCount++;

        // Check if the tile has a nutrient to "eat"
        CheckForNutrient(tile);

        Debug.Log($"Bacteria count: {_currentBacteriaCount}/{_petriDishCapacity}");
    }

    // Renamed from OnTileClicked for clarity
    public void OnTileClicked(Tile clickedTile)
    {
        // Check if any existing bacteria is adjacent to the clicked tile
        foreach (var bacteria in _bacteriaColony.ToList()) // Use ToList() to avoid collection modification issues
        {
            if (IsAdjacent(clickedTile, bacteria.currentTile))
            {
                SpawnBacteria(clickedTile);
                return; // Exit after spawning one
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
                // Add proper win screen/logic here
            }
        }
    }

    private bool IsAdjacent(Tile tile1, Tile tile2)
    {
        // Manhattan distance of 1 means they are adjacent (not diagonally)
        return (Mathf.Abs(tile1.x - tile2.x) + Mathf.Abs(tile1.y - tile2.y)) == 1;
    }
}
