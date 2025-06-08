using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game Configuration")]
    [SerializeField] private Bacteria _bacteriaPrefab;

    [Header("Level Progression")]
    [SerializeField] private List<LevelData> levelProgression; 
    [SerializeField] private float delayBeforeNextLevel = 2f;
    private int currentLevelIndex = 0;

    // [Header("Camera Control")]
    // [SerializeField] private Camera _mainCamera;
    // [SerializeField] private float _cameraSmoothSpeed = 0.125f;

    private int _petriDishCapacity;
    private int _totalNutrients;
    private int _currentBacteriaCount = 0;
    [SerializeField]  private List<Bacteria> _bacteriaColony = new List<Bacteria>();
    private int _nutrientsCollected = 0;
    private bool isExplosionBuffActive = false;

    /*UI*/
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

        // if (_mainCamera == null)
        // {
        //     _mainCamera = Camera.main;
        // }
    }

    void Start()
    {
        if (levelProgression.Count > 0)
        {
            StartLevel(currentLevelIndex);
        }
        else
        {
            Debug.LogError("No levels assigned to the Level Progression list in GameManager!");
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            
            LoadScene(0);
        }
    }

    /*
    private void LateUpdate()
    {
        if (_bacteriaColony.Count == 0) return;

        Vector3 centerPoint = GetCenterPoint();
        Vector3 cameraDestination = new Vector3(centerPoint.x, centerPoint.y, _mainCamera.transform.position.z);
        
        _mainCamera.transform.position = Vector3.Lerp(_mainCamera.transform.position, cameraDestination, _cameraSmoothSpeed);
    }
    */

    void StartLevel(int levelIndex)
    {
        GridManager.instance.BuildLevel(levelProgression[levelIndex]);

        _totalNutrients = GridManager.instance.NutrientCount;
        _petriDishCapacity = GridManager.instance.petriDishCap;
        _nutrientsCollected = 0;
        _currentBacteriaCount = 0;

        Vector2 startPos = new Vector2(GridManager.instance.StartX, GridManager.instance.StartY);
        Tile startTile = GridManager.instance.GetTileAtPosition(startPos);

        if (startTile != null && startTile.isWalkable)
        {
            SpawnBacteria(startTile);
        }
        else
        {
            Debug.LogError($"Start tile ({startPos.x},{startPos.y}) for Level {levelIndex + 1} is blocked or does not exist!");
        }
    }

    void LoadNextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex < levelProgression.Count)
        {
            Debug.Log($"LEVEL COMPLETE! Loading Level {currentLevelIndex + 1}...");
            StartCoroutine(LoadLevelRoutine());
        }
        else
        {
            Debug.Log("CONGRATULATIONS! You have completed all levels!");
            _youWin.gameObject.SetActive(true);
        }
    }                

    IEnumerator LoadLevelRoutine()
    {
        yield return new WaitForSeconds(delayBeforeNextLevel);

        foreach (var bacteria in _bacteriaColony)
        {
            if (bacteria != null) Destroy(bacteria.gameObject);
        }
        _bacteriaColony.Clear();

        StartLevel(currentLevelIndex);
    }

    /*
    private Vector3 GetCenterPoint()
    {
        if (_bacteriaColony.Count == 1)
        {
            return _bacteriaColony[0].transform.position;
        }

        var bounds = new Bounds(_bacteriaColony[0].transform.position, Vector3.zero);
        for (int i = 0; i < _bacteriaColony.Count; i++)
        {
            bounds.Encapsulate(_bacteriaColony[i].transform.position);
        }
        return bounds.center;
    }
    */
    private void SpawnBacteria(Tile tile, Bacteria parentBacteria = null)
    {
        if (_currentBacteriaCount >= _petriDishCapacity)
        {
            Debug.Log($"Petri dish is full! Capacity: {_petriDishCapacity}. Game Over!");
            _youLose.gameObject.SetActive(true);
            return;
        }

        if (parentBacteria != null)
        {
            parentBacteria.PerformDivisionShrink();
        }

        Vector3 spawnPosition = (parentBacteria != null) ? parentBacteria.transform.position : tile.transform.position;
        var newBacteria = Instantiate(_bacteriaPrefab, spawnPosition, Quaternion.identity);

        newBacteria.MoveToTile(tile, parentBacteria); 
        tile.SetOccupied(true);
        _bacteriaColony.Add(newBacteria);
        _currentBacteriaCount++;

        CheckForNutrient(tile);
        CheckForExplosion(tile);

        Debug.Log($"Bacteria count: {_currentBacteriaCount}/{_petriDishCapacity}");
    }


    public void OnTileClicked(Tile clickedTile)
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

    public void OnPortalTileClicked(Tile clickedTile)
    {
        foreach (var bacteria in _bacteriaColony.ToList())
        {
            if (IsAdjacent(clickedTile, bacteria.currentTile))
            {
                foreach (var region in GridManager.instance.currentLevelData.portalRegion)
                {
                    Vector2 EnterPos = region.EnterPortal;
                    Vector2 ExitPos = region.ExitPortal;
                    Tile EnterTile = GridManager.instance.GetTileAtPosition(EnterPos);
                    Tile ExitTile = GridManager.instance.GetTileAtPosition(ExitPos);
                    SpawnBacteria(EnterTile);
                    SpawnBacteria(ExitTile);
                }
            }
            else
            {
                Debug.Log($"No adjacent bacteria found for portal tile at {clickedTile.x}, {clickedTile.y}");
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
                LoadNextLevel();
            }
        }
    }

    private void CheckForExplosion(Tile tile)
    {
        if (tile.OccupyingExplosion != null)
        {
            tile.ClearExplosion();
            isExplosionBuffActive = true;
            Debug.Log($"Explosion buff collected!");
        }
    }

    private void Explode(Tile centerTile)
    {
        List<Tile> neighbors = GridManager.instance.GetNeighborTiles(centerTile, true);
        foreach (Tile neighbor in neighbors)
        {
            neighbor.ClearWall();
        }
    }



    private bool IsAdjacent(Tile tile1, Tile tile2)
    {
        return (Mathf.Abs(tile1.x - tile2.x) + Mathf.Abs(tile1.y - tile2.y)) == 1;
    }

    public void LoadScene(int Index)
    {
        SceneManager.LoadScene(Index);
    }
}
