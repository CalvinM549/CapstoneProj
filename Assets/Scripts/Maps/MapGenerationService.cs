using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGenerationService
{
    private RoomDatabase roomDatabase;
    private Vector2Int mapBounds;
    private int mapNodes;

    public MapGenerationService(RoomDatabase data, Vector2Int bounds, int nodes)
    {
        roomDatabase = data;
        mapBounds = bounds;
        mapNodes = nodes;
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

    #region RandomWalk

    public HashSet<Vector2Int> RunSimpleRandomWalk()
    {
        HashSet<Vector2Int> visited = new();

        Vector2Int currentPos = Vector2Int.zero;
        visited.Add(currentPos);

        int currentIteration = 0;
        int currentStep = 0;

        while (currentStep < mapNodes && currentIteration < 100) // Fail state
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

    private bool IsWithinBounds(Vector2Int pos)
    {
        return (pos.x >= -mapBounds.x
             && pos.x <= mapBounds.x
             && pos.y >= -mapBounds.y
             && pos.y <= mapBounds.y);
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

    #region Node Generation

    private Dictionary<Vector2Int, MapNode> GenerateMapNodes(HashSet<Vector2Int> grid)
    {
        // Random generation for nodes atm, no weight

        Dictionary<Vector2Int, MapNode> map = new();
        int nodeIndex = 0;

        foreach (var square in grid)
        {
            //if (square == Vector2Int.zero)
            // Get specific node
            nodeIndex++;
            MapNode node = new()
            {
                room = roomDatabase.GetRandom(), // Replace with weighted function?
                coordinates = square,
                cleared = false
            };

            map[square] = node;
        }

        return map;
    }

    private void ConnectNodes(Dictionary<Vector2Int, MapNode> map)
    {

        foreach (var kvp in map)
        {
            Vector2Int tile = kvp.Key;
            MapNode node = kvp.Value;

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (node.connections.ContainsKey(direction)) continue;

                if (map.TryGetValue(tile + direction.ToGridOffset(), out MapNode adjacent))
                {
                    node.ConnectTo(adjacent, direction);
                }
            }
        }
    }

    #endregion

    // Connect rooms
}
