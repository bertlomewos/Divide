using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator instance;

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

    public LevelData GenerateLevel(int width, int height, int nutrientCount, int explosionBuffCount, int portalPairCount, int leniency)
    {
        LevelData levelData = CreatePuzzleLevel(width, height, nutrientCount, explosionBuffCount, portalPairCount, leniency);

        if (levelData == null)
        {
            Debug.LogError("Failed to generate a solvable puzzle level.");
            return null;
        }

        Debug.Log($"Generated puzzle level. Optimal moves: {levelData.Capacity - leniency}, Player capacity: {levelData.Capacity}");
        return levelData;
    }

    private LevelData CreatePuzzleLevel(int width, int height, int nutrientCount, int explosionBuffCount, int portalPairCount, int leniency)
    {
        var levelData = ScriptableObject.CreateInstance<LevelData>();
        levelData.width = width;
        levelData.height = height;
        levelData.nutrientCoordinates = new List<Vector2Int>();
        levelData.explosionCoordinates = new List<Vector2Int>();
        levelData.portalRegion = new List<PortalRegion>();
        levelData.wallRegions = new List<WallRegion>();

        bool[,] walls = new bool[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                walls[x, y] = true;

        List<Vector2Int> availablePositions = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                availablePositions.Add(new Vector2Int(x, y));

        // Place spawn point
        Vector2Int spawnPos = PopRandomPosition(availablePositions);
        if (spawnPos.x == -1) return null;
        levelData.SpawnX = spawnPos.x;
        levelData.SpawnY = spawnPos.y;
        walls[spawnPos.x, spawnPos.y] = false;

        // Generate maze
        GenerateMaze(walls, width, height, spawnPos);

        // Place nutrients in hard-to-reach areas
        nutrientCount = Mathf.Min(nutrientCount, availablePositions.Count);
        for (int i = 0; i < nutrientCount; i++)
        {
            Vector2Int pos = FindHardToReachPosition(availablePositions, walls, width, height, spawnPos, levelData.nutrientCoordinates, explosionBuffCount > 0);
            if (pos.x == -1) break;
            levelData.nutrientCoordinates.Add(pos);
            walls[pos.x, pos.y] = false;
            availablePositions.Remove(pos);
        }

        // Place explosion buffs in strategic locations
        explosionBuffCount = Mathf.Min(explosionBuffCount, availablePositions.Count);
        for (int i = 0; i < explosionBuffCount; i++)
        {
            Vector2Int pos = FindStrategicPosition(availablePositions, walls, width, height, levelData.nutrientCoordinates);
            if (pos.x == -1) break;
            levelData.explosionCoordinates.Add(pos);
            walls[pos.x, pos.y] = false;
            availablePositions.Remove(pos);
        }

        // Place portals in distant regions
        portalPairCount = Mathf.Min(portalPairCount, availablePositions.Count / 2);
        for (int i = 0; i < portalPairCount; i++)
        {
            if (availablePositions.Count < 2) break;
            Vector2Int enter = FindDistantPosition(availablePositions, walls, width, height, spawnPos);
            if (enter.x == -1) break;
            availablePositions.Remove(enter);
            Vector2Int exit = FindDistantPosition(availablePositions, walls, width, height, enter);
            if (exit.x == -1) break;
            levelData.portalRegion.Add(new PortalRegion { EnterPortal = enter, ExitPortal = exit });
            walls[enter.x, enter.y] = false;
            walls[exit.x, exit.y] = false;
            availablePositions.Remove(exit);
        }

        // Add walls to levelData
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (walls[x, y])
                {
                    levelData.wallRegions.Add(new WallRegion { startCoordinate = new Vector2Int(x, y), endCoordinate = new Vector2Int(x, y) });
                }
            }
        }

        // Verify solvability
        int optimalMoves = CalculateOptimalPathCost(levelData);
        if (optimalMoves == -1)
        {
            Debug.LogError("Generated an unsolvable level.");
            return null;
        }

        levelData.Capacity = optimalMoves + leniency;
        return levelData;
    }

    private void GenerateMaze(bool[,] walls, int width, int height, Vector2Int startPos)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        walls[startPos.x, startPos.y] = false;
        stack.Push(startPos);

        int[] dx = { 0, 0, 2, -2 };
        int[] dy = { 2, -2, 0, 0 };

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            var neighbors = new List<int> { 0, 1, 2, 3 }.OrderBy(_ => Random.value).ToList();
            bool found = false;

            foreach (int i in neighbors)
            {
                Vector2Int next = new Vector2Int(current.x + dx[i], current.y + dy[i]);
                if (next.x >= 0 && next.x < width && next.y >= 0 && next.y < height && walls[next.x, next.y])
                {
                    walls[next.x, next.y] = false;
                    walls[current.x + dx[i] / 2, current.y + dy[i] / 2] = false;
                    stack.Push(next);
                    found = true;
                    break;
                }
            }

            if (!found) stack.Pop();
        }

        // Add extra walls for complexity
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!walls[x, y] && Random.value < 0.1f && new Vector2Int(x, y) != startPos)
                    walls[x, y] = true;
            }
        }

        // Ensure at least one open tile next to spawn
        int[] dxAdj = { 1, -1, 0, 0 };
        int[] dyAdj = { 0, 0, 1, -1 };
        bool hasOpenNeighbor = false;
        for (int i = 0; i < 4; i++)
        {
            int nx = startPos.x + dxAdj[i], ny = startPos.y + dyAdj[i];
            if (nx >= 0 && nx < width && ny >= 0 && ny < height && !walls[nx, ny])
            {
                hasOpenNeighbor = true;
                break;
            }
        }
        if (!hasOpenNeighbor)
        {
            int i = Random.Range(0, 4);
            int nx = startPos.x + dxAdj[i], ny = startPos.y + dyAdj[i];
            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                walls[nx, ny] = false;
        }
    }

    private Vector2Int PopRandomPosition(List<Vector2Int> positions)
    {
        if (positions.Count == 0) return new Vector2Int(-1, -1);
        int index = Random.Range(0, positions.Count);
        Vector2Int pos = positions[index];
        positions.RemoveAt(index);
        return pos;
    }

    private Vector2Int FindHardToReachPosition(List<Vector2Int> positions, bool[,] walls, int width, int height, Vector2Int spawnPos, List<Vector2Int> nutrientCoordinates, bool hasExplosions)
    {
        // Relaxed constraints for solvability
        int minSpawnDistance = (width >= 5 || height >= 5) ? 3 : 2;
        var candidates = positions
            .Where(p => p != spawnPos && ManhattanDistance(p, spawnPos) >= minSpawnDistance)
            .Where(p => nutrientCoordinates.Count == 0 || nutrientCoordinates.All(n => ManhattanDistance(p, n) >= 2))
            .Select(p => new { Pos = p, WallCount = CountAdjacentWalls(p, walls, width, height) })
            .Where(p => !hasExplosions || p.WallCount >= 1)
            .OrderByDescending(p => hasExplosions ? p.WallCount : ManhattanDistance(p.Pos, spawnPos))
            .Take(10)
            .ToList();

        if (candidates.Count == 0)
            return PopRandomPosition(positions);
        return candidates[Random.Range(0, candidates.Count)].Pos;
    }

    private Vector2Int FindStrategicPosition(List<Vector2Int> positions, bool[,] walls, int width, int height, List<Vector2Int> nutrients)
    {
        // Avoid placing explosions too close to nutrients
        var candidates = positions
            .Where(p => nutrients.All(n => ManhattanDistance(p, n) > 1))
            .Select(p => new { Pos = p, WallCount = CountAdjacentWalls(p, walls, width, height) })
            .Where(p => p.WallCount > 0)
            .OrderByDescending(p => p.WallCount)
            .Take(5)
            .ToList();

        if (candidates.Count == 0)
            return PopRandomPosition(positions);
        return candidates[Random.Range(0, candidates.Count)].Pos;
    }

    private Vector2Int FindDistantPosition(List<Vector2Int> positions, bool[,] walls, int width, int height, Vector2Int referencePos)
    {
        var candidates = positions
            .OrderByDescending(p => ManhattanDistance(p, referencePos))
            .Take(5)
            .ToList();

        if (candidates.Count == 0)
            return PopRandomPosition(positions);
        return candidates[Random.Range(0, candidates.Count)];
    }

    private int CountAdjacentWalls(Vector2Int pos, bool[,] walls, int width, int height)
    {
        int count = 0;
        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };
        for (int i = 0; i < 4; i++)
        {
            int nx = pos.x + dx[i], ny = pos.y + dy[i];
            if (nx >= 0 && nx < width && ny >= 0 && ny < height && walls[nx, ny])
                count++;
        }
        return count;
    }

    private int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private int CalculateOptimalPathCost(LevelData levelData)
    {
        if (levelData.nutrientCoordinates.Count == 0) return 0;

        var wallSet = new HashSet<Vector2Int>(levelData.wallRegions.Select(w => w.startCoordinate));
        var portalMap = new Dictionary<Vector2Int, Vector2Int>();
        foreach (var p in levelData.portalRegion)
        {
            portalMap[p.EnterPortal] = p.ExitPortal;
            portalMap[p.ExitPortal] = p.EnterPortal;
        }

        var explosionPositions = new HashSet<Vector2Int>(levelData.explosionCoordinates);
        var pointsOfInterest = new List<Vector2Int> { new Vector2Int(levelData.SpawnX, levelData.SpawnY) };
        pointsOfInterest.AddRange(levelData.nutrientCoordinates);
        pointsOfInterest = pointsOfInterest.Distinct().ToList();

        int n = pointsOfInterest.Count;
        if (n <= 1) return 0;

        var costMatrix = new int[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = i; j < n; j++)
            {
                int cost = CalculateShortestPath(pointsOfInterest[i], pointsOfInterest[j], levelData, wallSet, portalMap, explosionPositions);
                if (cost == -1) return -1;
                costMatrix[i, j] = costMatrix[j, i] = cost;
            }
        }

        var memo = new Dictionary<(int, int), int>();
        return FindMinMoves(0, 1 << 0, pointsOfInterest, costMatrix, memo);
    }

    private int CalculateShortestPath(Vector2Int start, Vector2Int end, LevelData data, HashSet<Vector2Int> walls, Dictionary<Vector2Int, Vector2Int> portals, HashSet<Vector2Int> explosions)
    {
        if (start == end) return 0;

        var queue = new Queue<(Vector2Int pos, int dist, HashSet<Vector2Int> usedExplosions)>();
        var visited = new HashSet<(Vector2Int, string)>();

        queue.Enqueue((start, 0, new HashSet<Vector2Int>()));
        visited.Add((start, ""));

        while (queue.Count > 0)
        {
            var (currentPos, currentDist, usedExplosions) = queue.Dequeue();
            if (currentPos == end) return currentDist;

            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                Vector2Int neighbor = new Vector2Int(currentPos.x + dx[i], currentPos.y + dy[i]);
                if (neighbor.x >= 0 && neighbor.x < data.width && neighbor.y >= 0 && neighbor.y < data.height)
                {
                    bool canMove = !walls.Contains(neighbor) || (explosions.Contains(currentPos) && !usedExplosions.Contains(currentPos));
                    if (canMove && !visited.Contains((neighbor, GetExplosionKey(usedExplosions))))
                    {
                        var newUsedExplosions = new HashSet<Vector2Int>(usedExplosions);
                        if (explosions.Contains(currentPos) && walls.Contains(neighbor))
                            newUsedExplosions.Add(currentPos);
                        visited.Add((neighbor, GetExplosionKey(newUsedExplosions)));
                        queue.Enqueue((neighbor, currentDist + 1, newUsedExplosions));
                    }

                    if (portals.ContainsKey(neighbor) && !visited.Contains((portals[neighbor], GetExplosionKey(usedExplosions))))
                    {
                        visited.Add((portals[neighbor], GetExplosionKey(usedExplosions)));
                        queue.Enqueue((portals[neighbor], currentDist + 1, new HashSet<Vector2Int>(usedExplosions)));
                    }
                }
            }
        }
        return -1;
    }

    private string GetExplosionKey(HashSet<Vector2Int> usedExplosions)
    {
        return string.Join(",", usedExplosions.OrderBy(p => p.x).ThenBy(p => p.y).Select(p => $"{p.x},{p.y}"));
    }

    private int FindMinMoves(int currentNodeIndex, int visitedMask, List<Vector2Int> points, int[,] costMatrix, Dictionary<(int, int), int> memo)
    {
        if (visitedMask == (1 << points.Count) - 1)
            return 0;

        if (memo.ContainsKey((currentNodeIndex, visitedMask)))
            return memo[(currentNodeIndex, visitedMask)];

        int minCost = int.MaxValue;
        for (int nextNodeIndex = 0; nextNodeIndex < points.Count; nextNodeIndex++)
        {
            if ((visitedMask & (1 << nextNodeIndex)) == 0)
            {
                int newCost = costMatrix[currentNodeIndex, nextNodeIndex] +
                              FindMinMoves(nextNodeIndex, visitedMask | (1 << nextNodeIndex), points, costMatrix, memo);
                if (newCost < minCost)
                    minCost = newCost;
            }
        }

        memo[(currentNodeIndex, visitedMask)] = minCost;
        return minCost;
    }
}