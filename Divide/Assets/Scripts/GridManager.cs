// ---- GridManager.cs ----
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Nutrient _nutrientPrefab;
    [SerializeField] private Transform _cam;
    public int NutrientCount { get; private set; }
    public int width { get; private set; }
    public int height { get; private set; }
    public int StartX { get; private set; }
    public int StartY { get; private set; }
    public int petriDishCap { get; private set; }

    private Dictionary<Vector2, Tile> _tiles = new Dictionary<Vector2, Tile>();
    private LevelData currentLevelData;
    public static GridManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

   
    public void BuildLevel(LevelData levelData)
    {
        this.currentLevelData = levelData;
        ClearGrid(); 

      
        width = currentLevelData.width;
        height = currentLevelData.height;
        StartX = currentLevelData.SpawnX;
        StartY = currentLevelData.SpawnY;
        petriDishCap = currentLevelData.Capacity;
        NutrientCount = currentLevelData.nutrientCoordinates.Count;

        GenerateGrid();
    }

    private void ClearGrid()
    {
        foreach (var tile in _tiles.Values)
        {
            if (tile != null) Destroy(tile.gameObject);
        }
        _tiles.Clear();
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, y), Quaternion.identity, transform);
                spawnedTile.name = $"Tile {x} {y}";
                spawnedTile.x = x;
                spawnedTile.y = y;

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset);

                _tiles[new Vector2(x, y)] = spawnedTile;
            }
        }
        if (_cam != null)
        {
            _cam.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10f);
        }

        LoadLevelLayout();
    }

    private void LoadLevelLayout()
    {
        
        foreach (var region in currentLevelData.wallRegions)
        {
            for (int x = region.startCoordinate.x; x <= region.endCoordinate.x; x++)
            {
                for (int y = region.startCoordinate.y; y <= region.endCoordinate.y; y++)
                {
                    Tile tile = GetTileAtPosition(new Vector2(x, y));
                    if (tile != null)
                    {
                        tile.SetAsWall();
                    }
                }
            }
        }

        foreach (var nutrientCoord in currentLevelData.nutrientCoordinates)
        {
            Tile tile = GetTileAtPosition(nutrientCoord);
            if (tile != null && tile.isWalkable)
            {
                var nutrient = Instantiate(_nutrientPrefab);
                tile.SetNutrient(nutrient);
            }
        }
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }
        return null;
    }
}
