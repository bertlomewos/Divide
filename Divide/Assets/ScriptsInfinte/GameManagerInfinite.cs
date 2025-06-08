using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManagerInfinite : MonoBehaviour
{
    public static GameManagerInfinite instance;

    [Header("Game Configuration")]
    [SerializeField] private BacteriaInfinite _bacteriaPrefab;

    [Header("Level 1 Generation Settings")]
    [SerializeField] private int baseWidth = 3;
    [SerializeField] private int baseHeight = 3;
    [SerializeField] private int baseNutrients = 1;
    [SerializeField] private int baseLeniency = 2; // Increased to 2 for playability

    [Header("Timing")]
    [SerializeField] private float delayBeforeNextLevel = 2f;
    private int currentLevel = 1;

    private int _petriDishCapacity;
    private int _totalNutrients;
    private int _currentBacteriaCount = 0;
    [SerializeField] private List<BacteriaInfinite> _bacteriaColony = new List<BacteriaInfinite>();
    private int _nutrientsCollected = 0;
    private bool isExplosionBuffActive = false;

    [Header("UI References")]
    public GameObject _youLose;
    public GameObject _youWin;

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
        Debug.Log("GameManager Start: Initializing level...");
        StartNewLevel();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("Restart key pressed. Reloading scene.");
            LoadScene(0);
        }
    }

    void StartNewLevel()
    {
        if (_youWin != null) _youWin.SetActive(false);
        if (_youLose != null) _youLose.SetActive(false);

        // Adjust minimum grid size based on elements
        int elementCount = baseNutrients + (currentLevel - 1) / 5; // Base nutrients
        elementCount += (currentLevel >= 2 && currentLevel % 2 == 0) ? 1 : 0; // Explosions
        elementCount += (currentLevel >= 4 && currentLevel % 3 == 0) ? 2 : 0; // Portals (2 tiles)
        int minGridSize = elementCount > 1 ? 4 : 3; // Increase to 4 if multiple elements

        // Randomize grid size with level progression
        int minWidth = Mathf.Max(baseWidth + (currentLevel - 1) / 2, minGridSize);
        int minHeight = Mathf.Max(baseHeight + (currentLevel - 1) / 2, minGridSize);
        int maxWidth = minWidth + Random.Range(0, 2);
        int maxHeight = minHeight + Random.Range(0, 2);
        int width = Mathf.Clamp(Random.Range(minWidth, maxWidth + 1), minGridSize, 7);
        int height = Mathf.Clamp(Random.Range(minHeight, maxHeight + 1), minGridSize, 7);

        // Nutrient count capped at 3 to ensure performance in TSP solver. Increase cap only with optimized solver.
        int nutrients = Mathf.Min(baseNutrients + (currentLevel - 1) / 5, 3);
        int leniency = Mathf.Max(1, baseLeniency - (currentLevel - 1) / 3); // Minimum leniency of 1
        int explosions = (currentLevel >= 2 && currentLevel % 2 == 0) ? 1 : 0;
        int portals = (currentLevel >= 4 && currentLevel % 3 == 0) ? 1 : 0;

        Debug.Log($"Starting Level {currentLevel}: Leniency={leniency}, Width={width}, Height={height}, Nutrients={nutrients}, Explosions={explosions}, Portals={portals}");

        LevelDataInfinite newLevelData = null;
        int attempts = 0;
        const int maxAttempts = 5;
        while (newLevelData == null && attempts < maxAttempts)
        {
            newLevelData = LevelGeneratorInfinite.instance.GenerateLevel(width, height, nutrients, explosions, portals, leniency);
            attempts++;
            if (newLevelData == null)
                Debug.LogWarning($"Level generation failed, attempt {attempts}/{maxAttempts}");
        }

        if (newLevelData == null)
        {
            Debug.LogError("FATAL: Could not generate a valid level after max attempts.");
            LoadScene(0);
            return;
        }

        GridManagerInfinite.instance.BuildLevel(newLevelData);

        _totalNutrients = newLevelData.nutrientCoordinates.Count;
        _petriDishCapacity = newLevelData.Capacity;

        Debug.Log($"Spawn point set to: ({newLevelData.SpawnX}, {newLevelData.SpawnY})");
        TileInfinite spawnTile = GridManagerInfinite.instance.GetTileAtPosition(new Vector2Int(newLevelData.SpawnX, newLevelData.SpawnY));
        if (spawnTile == null)
        {
            Debug.LogError($"Spawn tile at ({newLevelData.SpawnX}, {newLevelData.SpawnY}) is null! Aborting spawn.");
            return;
        }
        if (!spawnTile.isWalkable)
        {
            Debug.LogError($"Spawn tile at ({newLevelData.SpawnX}, {newLevelData.SpawnY}) is not walkable! Forcing walkable.");
            spawnTile.ClearWall();
        }
        if (spawnTile.OccupyingNutrient != null || spawnTile.OccupyingExplosion != null)
        {
            Debug.LogWarning($"Spawn tile at ({newLevelData.SpawnX}, {newLevelData.SpawnY}) has nutrient or explosion buff!");
        }

        _currentBacteriaCount = 0;
        try
        {
            SpawnBacteria(spawnTile);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception in SpawnBacteria: {ex.Message}\n{ex.StackTrace}");
        }
    }

    void LoadNextLevel()
    {
        currentLevel++;
        Debug.Log($"LEVEL COMPLETE! Loading Level {currentLevel}...");
        StartCoroutine(LoadLevelRoutine());
    }

    IEnumerator LoadLevelRoutine()
    {
        foreach (var bacteria in _bacteriaColony)
        {
            if (bacteria != null) Destroy(bacteria.gameObject);
        }
        _bacteriaColony.Clear();
        _nutrientsCollected = 0;
        _currentBacteriaCount = 0;

        StartNewLevel();

        yield return null;
    }

    private void CheckForNutrient(TileInfinite tile)
    {
        if (tile.OccupyingNutrient != null)
        {
            tile.ClearNutrient();
            _nutrientsCollected++;
            Debug.Log($"Nutrient collected! Total: {_nutrientsCollected}/{_totalNutrients}");

            if (_nutrientsCollected >= _totalNutrients)
            {
                _youWin.SetActive(true);
                StartCoroutine(WinSequenceRoutine());
            }
        }
    }

    IEnumerator WinSequenceRoutine()
    {
        yield return new WaitForSeconds(delayBeforeNextLevel);
        LoadNextLevel();
    }

    private void SpawnBacteria(TileInfinite tile, BacteriaInfinite parentBacteria = null)
    {
        if (tile == null)
        {
            Debug.LogError("Cannot spawn bacteria: Tile is null!");
            return;
        }
        if (!tile.isWalkable)
        {
            Debug.LogError($"Cannot spawn bacteria: Tile at ({tile.x}, {tile.y}) is not walkable!");
            return;
        }
        if (_currentBacteriaCount >= _petriDishCapacity)
        {
            Debug.Log($"Petri dish is full! Capacity: {_petriDishCapacity}. Game Over!");
            _youLose.SetActive(true);
            return;
        }
        if (_bacteriaPrefab == null)
        {
            Debug.LogError("Bacteria prefab is not assigned in GameManager!");
            return;
        }
        if (parentBacteria != null)
        {
            try
            {
                parentBacteria.PerformDivisionShrink();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Exception in PerformDivisionShrink: {ex.Message}\n{ex.StackTrace}");
                return;
            }
        }
        Vector3 spawnPosition = (parentBacteria != null) ? parentBacteria.transform.position : tile.transform.position;
        var newBacteria = Instantiate(_bacteriaPrefab, spawnPosition, Quaternion.identity);
        if (newBacteria == null)
        {
            Debug.LogError($"Failed to instantiate bacteria at ({tile.x}, {tile.y})!");
            return;
        }
        try
        {
            newBacteria.MoveToTile(tile, parentBacteria);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception in MoveToTile: {ex.Message}\n{ex.StackTrace}");
            Destroy(newBacteria.gameObject);
            return;
        }
        tile.SetOccupied(true);
        _bacteriaColony.Add(newBacteria);
        _currentBacteriaCount++;
        CheckForExplosion(tile);
        CheckForNutrient(tile);
        Debug.Log($"Bacteria spawned at ({tile.x}, {tile.y}). Count: {_currentBacteriaCount}/{_petriDishCapacity}. GameObject: {newBacteria.name}");
    }

    public void OnTileClicked(TileInfinite clickedTile)
    {
        if (isExplosionBuffActive)
        {
            if (_bacteriaColony.Any(bacteria => IsAdjacent(clickedTile, bacteria.currentTile)))
            {
                SpawnBacteria(clickedTile);
                Explode(clickedTile);
                isExplosionBuffActive = false;
                return;
            }
        }
        foreach (var bacteria in _bacteriaColony.ToList())
        {
            if (IsAdjacent(clickedTile, bacteria.currentTile))
            {
                SpawnBacteria(clickedTile, bacteria);
                return;
            }
        }
    }

    public void OnPortalTileClicked(TileInfinite clickedPortalTile)
    {
        BacteriaInfinite adjacentBacteria = _bacteriaColony.FirstOrDefault(b => IsAdjacent(clickedPortalTile, b.currentTile));
        if (adjacentBacteria != null)
        {
            Vector2 enterPos = new Vector2(clickedPortalTile.x, clickedPortalTile.y);
            Vector2 exitPos = Vector2.zero;
            bool foundPortal = false;
            foreach (var region in GridManagerInfinite.instance.currentLevelData.portalRegion)
            {
                if (region.EnterPortal == enterPos)
                {
                    exitPos = region.ExitPortal;
                    foundPortal = true;
                    break;
                }
                if (region.ExitPortal == enterPos)
                {
                    exitPos = region.EnterPortal;
                    foundPortal = true;
                    break;
                }
            }
            if (foundPortal)
            {
                TileInfinite exitTile = GridManagerInfinite.instance.GetTileAtPosition(exitPos);
                if (exitTile != null && exitTile.isWalkable)
                {
                    SpawnBacteria(exitTile, adjacentBacteria);
                }
            }
        }
        else
        {
            Debug.Log($"No adjacent bacteria found for portal tile at ({clickedPortalTile.x}, {clickedPortalTile.y})");
        }
    }

    private void CheckForExplosion(TileInfinite tile)
    {
        if (tile.OccupyingExplosion != null)
        {
            tile.ClearExplosion();
            isExplosionBuffActive = true;
            Debug.Log($"Explosion buff collected!");
        }
    }

    private void Explode(TileInfinite centerTile)
    {
        List<TileInfinite> neighbors = GridManagerInfinite.instance.GetNeighborTiles(centerTile, true);
        foreach (TileInfinite neighbor in neighbors)
        {
            neighbor.ClearWall();
        }
    }

    private bool IsAdjacent(TileInfinite tile1, TileInfinite tile2)
    {
        return (Mathf.Abs(tile1.x - tile2.x) + Mathf.Abs(tile1.y - tile2.y)) == 1;
    }

    public void LoadScene(int Index)
    {
        SceneManager.LoadScene(Index);
    }
}