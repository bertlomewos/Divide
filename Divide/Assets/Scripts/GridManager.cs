using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Nutrient _nutrientPrefab;
    [SerializeField] private ExplosionBuff _explosionPrefab;
    
    [SerializeField] private Transform _cam;
    [SerializeField] private Transform _Background;
    public int NutrientCount { get; private set; }
    public int ExplosionBuffCount { get; private set; }
    public int width { get; private set; }
    public int height { get; private set; }
    public int StartX { get; private set; }
    public int StartY { get; private set; }
    public int petriDishCap { get; private set; }

    private Dictionary<Vector2, Tile> _tiles = new Dictionary<Vector2, Tile>();
    public LevelData currentLevelData;
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
        ExplosionBuffCount = currentLevelData.explosionCoordinates.Count;

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
        if (_Background != null)
        {
            _Background.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10f);
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

        foreach (var explosionCoord in currentLevelData.explosionCoordinates)
        {
            Tile tile = GetTileAtPosition(explosionCoord);
            if (tile != null && tile.isWalkable)
            {
                var explosion = Instantiate(_explosionPrefab);
                tile.SetExplosion(explosion);
            }
        }
        foreach(var Place in currentLevelData.portalRegion)
        {
            Tile enterTile = GetTileAtPosition(Place.EnterPortal);
            Tile exitTile = GetTileAtPosition(Place.ExitPortal);
            if (enterTile != null && exitTile != null)
            {
                enterTile.SetAsPortal();
                exitTile.SetAsPortal();
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

    public List<Tile> GetNeighborTiles(Tile currentTile, bool includeDiagonals)
    {
        List<Tile> neighbours = new List<Tile>();

        int[] x_directions = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] y_directions = { -1, -1, -1, 0, 0, 1, 1, 1 };

        if (!includeDiagonals)
        {
            x_directions = new int[] { 0, 0, 1, -1 };
            y_directions = new int[] { 1, -1, 0, 0 };
        }

        for (int i = 0; i < x_directions.Length; i++)
        {
            Vector2Int neighbourPos = new Vector2Int(currentTile.x + x_directions[i], currentTile.y + y_directions[i]);

            Tile neighbour = GetTileAtPosition(neighbourPos);
            if (neighbour != null)
            {
                neighbours.Add(neighbour);
            }
        }
        return neighbours;
    }
}
