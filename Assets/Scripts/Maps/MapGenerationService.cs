using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Linq;

public class MapGenerationService
{
    private RoomDatabase db;
    private Vector2Int bounds;
    private int nodeCount;
    private int walkerCount;

    private float baseDifficulty;
    private float difficultyPerDepth;

    public MapGenerationService(
        RoomDatabase data,
        Vector2Int bounds,
        int nodeCount, 
        int walkerCount,
        float baseDifficulty,
        float difficultyPerDepth
        )
    {
        this.db = data;
        this.bounds = bounds;
        this.nodeCount = nodeCount;
        this.walkerCount = Mathf.Max(1, walkerCount);
        this.baseDifficulty = baseDifficulty;
        this.difficultyPerDepth = difficultyPerDepth;
    }



    public RunMap GenerateMapWithSeed(int seed) // Add chapter sorting later lol
    {
        RunMap map = new();

        RNGManager.Instance.InitRNG(seed);

        var grid = RunSimpleRandomWalk();

        map.tiles = GenerateMapNodes(grid);
        map.startNode = map.tiles[Vector2Int.zero];

        ConnectNodes(map.tiles);
        // Get basic grid map

        // Assign rooms based on weight?

        return map;
    }

    public RunMap GenerateMapWithSeed(int seed, int chapter = 0) // Add chapter sorting later lol
    {
        RunMap map = new();

        RNGManager.Instance.InitRNG(seed);

        var grid = RunComplexRandomWalk();
        var analysis = AnalyzeGrid(grid);

        map.tiles = GenerateMapNodes(grid, analysis, chapter);
        map.startNode = map.tiles[Vector2Int.zero];
        map.totalNodes = map.tiles.Count;

        ConnectNodes(map.tiles);

        return map;
    }

    #region RandomWalk

    public HashSet<Vector2Int> RunSimpleRandomWalk()
    {
        HashSet<Vector2Int> visited = new();

        Vector2Int currentPos = Vector2Int.zero;
        visited.Add(currentPos);

        int currentIteration = 0;
        int currentStep = 0;

        while (currentStep < nodeCount && currentIteration < 100) // Fail state
        {
            currentIteration++;
            Vector2Int randomDirection = DirectionExtensions.Random().ToGridOffset();
            Vector2Int nextPos = currentPos + randomDirection;

            if (!visited.Contains(nextPos) && IsWithinBounds(nextPos))
            {
                currentPos = nextPos;
                visited.Add(nextPos);
                currentStep++;
            }

            else if (IsTrapped(currentPos, visited))
            {
                currentPos = Vector2Int.zero;
            }
        }

        return visited;
    }

    public HashSet<Vector2Int> RunComplexRandomWalk()
    {
        HashSet<Vector2Int> visited = new() { Vector2Int.zero };
        List<Vector2Int> visitedList = new() { Vector2Int.zero };

        int nodesPerWalker = Mathf.Max(1, nodeCount / walkerCount);

        for (int walker = 0; walker < walkerCount; walker++)
        {
            Vector2Int currentPos = walker == 0 
                ? Vector2Int.zero 
                : visitedList[RNGManager.Instance.rng.Next(visitedList.Count)];

            int currentIteration = 0;
            int currentStep = 0;

            while (currentStep < nodesPerWalker && currentIteration < 100)
            {
                currentIteration++;

                Vector2Int randomDirection = DirectionExtensions.Random().ToGridOffset();
                Vector2Int nextPos = currentPos + randomDirection;

                if (!visited.Contains(nextPos) && IsWithinBounds(nextPos))
                {
                    currentPos = nextPos;
                    visited.Add(nextPos);
                    visitedList.Add(nextPos);
                    currentStep++;
                }

                else if (IsTrapped(currentPos, visited))
                {
                    currentPos = visitedList[RNGManager.Instance.rng.Next(visitedList.Count)];
                }
            }
        }

        return visited;
    }

    private bool IsWithinBounds(Vector2Int pos)
    {
        return (pos.x >= -bounds.x
             && pos.x <= bounds.x
             && pos.y >= -bounds.y
             && pos.y <= bounds.y);
    }

    private bool IsTrapped(Vector2Int pos, HashSet<Vector2Int> currentMap)
    {
        foreach (Direction direction in Enum.GetValues(typeof(Direction)))
        {
            Vector2Int adjacent = pos + direction.ToGridOffset();

            if (!currentMap.Contains(adjacent) && IsWithinBounds(adjacent))
                return false;
        }

        return true;
    }

    #endregion

    #region Area Generation

    #endregion

    #region Map Generation

    #endregion

    private class GridAnalysis
    {
        public Dictionary<Vector2Int, DirectionMask> requiredDirections = new();
        public Dictionary<Vector2Int, int> depth = new();
    }

    private GridAnalysis AnalyzeGrid(HashSet<Vector2Int> grid)
    {
        GridAnalysis analysis = new();
        HashSet<Vector2Int> visited = new() { Vector2Int.zero };
        Queue<Vector2Int> queue = new();

        queue.Enqueue(Vector2Int.zero);
        analysis.depth[Vector2Int.zero] = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                Vector2Int neighborPos = current + direction.ToGridOffset();
                if (!grid.Contains(neighborPos)) continue;
                if(visited.Contains(neighborPos)) continue;

                visited.Add(neighborPos);
                analysis.depth[neighborPos] = analysis.depth[current] + 1;

                analysis.requiredDirections[current] = analysis.requiredDirections.GetValueOrDefault(current) | direction.ToMask();
                analysis.requiredDirections[neighborPos] = analysis.requiredDirections.GetValueOrDefault(neighborPos) | direction.Opposite().ToMask();

                queue.Enqueue(neighborPos);
            }
        }
    
        return analysis;
    }

    private Dictionary<Vector2Int, RoomNode> GenerateMapNodes(HashSet<Vector2Int> grid, GridAnalysis analysis, int chapter)
    {
        Dictionary<Vector2Int, RoomNode> map = new();

        Vector2Int endPos = analysis.depth.Aggregate((a, b) => b.Value > a.Value ? b : a).Key;

        foreach (var square in grid)
        {
            DirectionMask requiredDoors = analysis.requiredDirections.GetValueOrDefault(square);
            int depth = analysis.depth.GetValueOrDefault(square);

            RoomType category = DetermineCategory(square, endPos);
            RoomData room = SelectRoom(category, requiredDoors, depth, chapter);

            map[square] = new RoomNode
            {
                room = room,
                coordinates = square,
                cleared = false
            };
        }

        return map;
    }

    private RoomType DetermineCategory(Vector2Int pos, Vector2Int endPos)
    {
        if (pos == Vector2Int.zero) return RoomType.Start;


        return RoomType.Combat;
    }

    private RoomData SelectRoom(RoomType category, DirectionMask requiredDoors, int depth, int chapter)
    {
        var candidates = db.GetCandidates(category, requiredDoors, chapter).ToList();

        if (candidates.Count == 0)
        {
            Debug.LogError($"[MapGenerationService] No {category} room supports required connections {requiredDoors} at all - check your RoomDatabase entries");
            return db.GetByCategory(category).FirstOrDefault();
        }

        float targetDifficulty = DifficultyCurve(depth);
        return WeightedPick(candidates, targetDifficulty);

    }
    
    private float DifficultyCurve(int depth) => baseDifficulty + depth * difficultyPerDepth;

    private RoomData WeightedPick(List<RoomData> candidates, float targetDifficulty)
    {
        if(candidates.Count == 1) return candidates[0];

        var weights = new float[candidates.Count];
        float totalWeight = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            float distance = Mathf.Abs(candidates[i].difficultyCost - targetDifficulty);
            weights[i] = 1f / (1f + distance);
            totalWeight += weights[i];
        }

        float roll = (float)(RNGManager.Instance.rng.NextDouble() * totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            cumulative += weights[i];
            if(roll <= cumulative) return candidates[i];
        }

        return candidates[^1];
    }

    private void ConnectNodes(Dictionary<Vector2Int, RoomNode> map)
    {
        foreach (var kvp in map)
        {
            Vector2Int tile = kvp.Key;
            RoomNode node = kvp.Value;

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (node.connections.ContainsKey(direction)) continue;
                if (!map.TryGetValue(tile + direction.ToGridOffset(), out RoomNode adjacent)) continue;

                if(!node.room.CanConnectDirection(direction)) continue;
                if(!adjacent.room.CanConnectDirection(direction.Opposite())) continue;

                node.ConnectTo(adjacent, direction);
            }
        }
    }

    #region Node Generation

    private Dictionary<Vector2Int, RoomNode> GenerateMapNodes(HashSet<Vector2Int> grid)
    {
        // Random generation for nodes atm, no weight

        Dictionary<Vector2Int, RoomNode> map = new();
        int nodeIndex = 0;

        foreach (var square in grid)
        {
            //if (square == Vector2Int.zero)
            // Get specific node
            nodeIndex++;
            RoomNode node = new()
            {
                room = db.GetRandom(), // Replace with weighted function?
                coordinates = square,
                cleared = false
            };

            map[square] = node;
        }

        return map;
    }

    //private void ConnectNodes(Dictionary<Vector2Int, MapNode> map)
    //{

    //    foreach (var kvp in map)
    //    {
    //        Vector2Int tile = kvp.Key;
    //        MapNode node = kvp.Value;

    //        foreach (Direction direction in Enum.GetValues(typeof(Direction)))
    //        {
    //            if (node.connections.ContainsKey(direction)) continue;

    //            if (map.TryGetValue(tile + direction.ToGridOffset(), out MapNode adjacent))
    //            {
    //                node.ConnectTo(adjacent, direction);
    //            }
    //        }
    //    }
    //}

    #endregion

    // Connect rooms
}
