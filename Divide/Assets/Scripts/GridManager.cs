using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _cam;

    [Header("Game Elements")]
    [SerializeField] private Nutrient _nutrientPrefab;

    [Header("Level Design")]
    [SerializeField] private LevelData currentLevelData;

    public int NutrientCount => currentLevelData.nutrientCoordinates.Count;

    private Dictionary<Vector2, Tile> _tiles;
    public static GridManager instance;

    public int StartX, StartY;
    public int petriDishCap;


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
        _width = currentLevelData.width;
        _height = currentLevelData.height;
        StartX = currentLevelData.SpawnX;
        StartY = currentLevelData.SpawnY;
        petriDishCap = currentLevelData.Capacity;
        GenerateGrid();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartScene(0);
        }
    }

    public void GenerateGrid()
    {
        _tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
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
            _cam.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10f);
        }

        LoadLevel();
    }

    private void LoadLevel()
    {
        if (currentLevelData == null)
        {
            Debug.LogError("No LevelData assigned to GridManager!");
            return;
        }

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

    public void RestartScene(int index)
    {
        SceneManager.LoadScene(index);
    }
}
