using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _cam;
    [SerializeField] private Nutrient _nutrientPrefab;
    [SerializeField] public int _nutrientCount = 5;

    // Public property to safely expose the private _nutrientCount
    public int NutrientCount => _nutrientCount;

    private Dictionary<Vector2, Tile> _tiles;
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

        GenerateGrid();
        SpawnNutrients();
    }

    public void GenerateGrid()
    {
        _tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, y), Quaternion.identity);
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
    }

    public void SpawnNutrients()
    {
        var emptyTiles = _tiles.Values.OrderBy(t => Random.value).ToList();

        // Ensure we don't try to spawn more nutrients than there are tiles
        int count = Mathf.Min(_nutrientCount, emptyTiles.Count);

        for (int i = 0; i < count; i++)
        {
            var tile = emptyTiles[i];
            var nutrient = Instantiate(_nutrientPrefab);
            tile.SetNutrient(nutrient);
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
