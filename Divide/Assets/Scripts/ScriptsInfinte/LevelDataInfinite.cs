using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WallRegionInfinite
{
    public Vector2Int startCoordinate;
    public Vector2Int endCoordinate;
}
[System.Serializable]
public class PortalRegionInfinite
{
    public Vector2Int EnterPortal;
    public Vector2Int ExitPortal;
}

[CreateAssetMenu(fileName = "New Level Data", menuName = "Levels/Level Data")]
public class LevelDataInfinite : ScriptableObject
{
    public int width, height;
    public int SpawnX, SpawnY;
    public int Capacity;
    [Header("Level Layout")]
    public List<WallRegionInfinite> wallRegions;
    public List<PortalRegionInfinite>portalRegion;
    public List<Vector2Int> nutrientCoordinates;
    public List<Vector2Int> explosionCoordinates;
}
