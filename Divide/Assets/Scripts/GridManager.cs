using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _cam;

    private Dictionary<Vector2?, Tile> _tiles;
    public GridManager instance;


    private void Awake()
    {
        if (instance == null) { 
        
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }

    private void Start()
    {
        GenerateGrid();
    }
    public void GenerateGrid()
    {
        for(int x = 0; x < _width; x++)
        {
            for(int y = 0; y < _height; y++)
            {
               var SpawnedTile = Instantiate(_tilePrefab, new Vector3(x, y), Quaternion.identity);
                SpawnedTile.name = $"Tile {x} {y}";
                _tiles[new Vector2(x, y)] = SpawnedTile;
                SpawnedTile.x = x;
                SpawnedTile.y = y;

                var isOffset = (x  % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);

               SpawnedTile.SetColor(isOffset);


            }
        }

        _cam.position = new Vector3((float)_width /2 -0.5f, (float)_height / 2 - 0.5f, -10f);
    }

    public  Tile GetTilePosition(Vector2 pos)
    {
        if(_tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }
        else
            return null;
    }
}
