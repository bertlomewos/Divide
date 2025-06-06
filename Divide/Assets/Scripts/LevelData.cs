using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WallRegion
{
    public Vector2Int startCoordinate;
    public Vector2Int endCoordinate;
}

[CreateAssetMenu(fileName = "New Level Data", menuName = "Levels/Level Data")]
public class LevelData : ScriptableObject
{
    public int width, height;
    public int SpawnX, SpawnY;
    public int Capacity;
    [Header("Level Layout")]
    public List<WallRegion> wallRegions;
    public List<Vector2Int> nutrientCoordinates;
}
