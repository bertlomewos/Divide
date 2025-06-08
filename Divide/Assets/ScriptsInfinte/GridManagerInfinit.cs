using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GridManagerInfinite : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private GameObject petriDishObject;
    [SerializeField] private TileInfinite _tilePrefab;
    [SerializeField] private NutrientInfinite _nutrientPrefab;
    [SerializeField] private ExplosionBuffInfinite _explosionPrefab;
    [SerializeField] private Transform _cam;

    [Header("Level Properties")]
    public int NutrientCount { get; private set; }
    public int ExplosionBuffCount { get; private set; }
    public int width { get; private set; }
    public int height { get; private set; }
    public int StartX { get; private set; }
    public int StartY { get; private set; }
    public int petriDishCap { get; private set; }

    private Dictionary<Vector2, TileInfinite> _tiles = new Dictionary<Vector2, TileInfinite>();
    public LevelDataInfinite currentLevelData;
    public static GridManagerInfinite instance;

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

    public void BuildLevel(LevelDataInfinite levelData)
    {
        this.currentLevelData = levelData;
        ClearGrid();

        width = currentLevelData.width;
        height = currentLevelData.height;

        UpdatePetriDishTransform(width, height);

        StartX = currentLevelData.SpawnX;
        StartY = currentLevelData.SpawnY;
        petriDishCap = currentLevelData.Capacity;
        NutrientCount = currentLevelData.nutrientCoordinates.Count;
        ExplosionBuffCount = currentLevelData.explosionCoordinates.Count;

        Debug.Log($"Building level with spawn point at ({StartX}, {StartY}), capacity: {petriDishCap}, nutrients: {NutrientCount}");

        GenerateGrid();

        // Verify spawn tile after generation
        TileInfinite spawnTile = GetTileAtPosition(new Vector2(StartX, StartY));
        if (spawnTile == null)
        {
            Debug.LogError($"Spawn tile at ({StartX}, {StartY}) is null after grid generation!");
        }
        else if (!spawnTile.isWalkable)
        {
            Debug.LogWarning($"Spawn tile at ({StartX}, {StartY}) is not walkable! Forcing walkable state.");
            spawnTile.ClearWall(); // Ensure spawn tile is walkable
        }
        else
        {
            Debug.Log($"Spawn tile at ({StartX}, {StartY}) is walkable and ready.");
        }
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
            _cam.transform.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10f);
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
                    if (x == StartX && y == StartY)
                    {
                        Debug.Log($"Skipping wall placement at spawn point ({x}, {y})");
                        continue;
                    }
                    TileInfinite tile = GetTileAtPosition(new Vector2(x, y));
                    if (tile != null)
                    {
                        tile.SetAsWall();
                    }
                }
            }
        }

        foreach (var nutrientCoord in currentLevelData.nutrientCoordinates)
        {
            TileInfinite tile = GetTileAtPosition(nutrientCoord);
            if (tile != null && tile.isWalkable)
            {
                var nutrient = Instantiate(_nutrientPrefab, tile.transform.position, Quaternion.identity, tile.transform);
                tile.SetNutrient(nutrient);
            }
        }

        foreach (var explosionCoord in currentLevelData.explosionCoordinates)
        {
            TileInfinite tile = GetTileAtPosition(explosionCoord);
            if (tile != null && tile.isWalkable)
            {
                var explosion = Instantiate(_explosionPrefab, tile.transform.position, Quaternion.identity, tile.transform);
                tile.SetExplosion(explosion);
            }
        }

        foreach (var portalRegion in currentLevelData.portalRegion)
        {
            TileInfinite enterTile = GetTileAtPosition(portalRegion.EnterPortal);
            TileInfinite exitTile = GetTileAtPosition(portalRegion.ExitPortal);
            if (enterTile != null && exitTile != null)
            {
                enterTile.SetAsPortal();
                exitTile.SetAsPortal();
            }
        }
    }

    private void UpdatePetriDishTransform(int gridWidth, int gridHeight)
    {
        if (petriDishObject == null)
        {
            Debug.LogWarning("Petri Dish GameObject not assigned in the GridManager.");
            return;
        }

        float dishPosX = (float)gridWidth / 2 - 0.5f;
        float dishPosY = (float)gridHeight / 2 - 0.5f;
        petriDishObject.transform.position = new Vector3(dishPosX, dishPosY, 1);

        SpriteRenderer dishSpriteRenderer = petriDishObject.GetComponent<SpriteRenderer>();
        if (dishSpriteRenderer == null || dishSpriteRenderer.sprite == null)
        {
            Debug.LogError("Petri Dish is missing its SpriteRenderer or Sprite!");
            return;
        }

        Vector2 spriteSize = dishSpriteRenderer.sprite.bounds.size;

        float gridDiagonal = Mathf.Sqrt(gridWidth * gridWidth + gridHeight * gridHeight);
        float padding = 1.0f;
        float desiredDiameter = gridDiagonal + padding;

        float scale = desiredDiameter / spriteSize.x;
        petriDishObject.transform.localScale = new Vector3(scale, scale, 1);
    }

    public TileInfinite GetTileAtPosition(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }
        return null;
    }

    public List<TileInfinite> GetNeighborTiles(TileInfinite currentTile, bool includeDiagonals)
    {
        List<TileInfinite> neighbours = new List<TileInfinite>();

        int[] x_directions = { 0, 0, 1, -1 };
        int[] y_directions = { 1, -1, 0, 0 };

        if (includeDiagonals)
        {
            x_directions = new int[] { -1, 0, 1, -1, 1, -1, 0, 1 };
            y_directions = new int[] { -1, -1, -1, 0, 0, 1, 1, 1 };
        }

        for (int i = 0; i < x_directions.Length; i++)
        {
            Vector2Int neighbourPos = new Vector2Int(currentTile.x + x_directions[i], currentTile.y + y_directions[i]);

            TileInfinite neighbour = GetTileAtPosition(neighbourPos);
            if (neighbour != null)
            {
                neighbours.Add(neighbour);
            }
        }
        return neighbours;
    }

    public TileInfinite FindWalkableTile()
    {
        return _tiles.Values.FirstOrDefault(t => t.isWalkable);
    }
}