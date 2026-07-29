using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MapGenerationService
{
    private RoomDatabase roomDatabase;
    private int rngSeed;
    private Vector2Int mapBounds;

    private int iterations;

    public MapGenerationService(RoomDatabase data, int seed, Vector2Int bounds)
    {
        roomDatabase = data;
        rngSeed = seed;
        mapBounds = bounds;
    }

    //public RunMap GenerateTestingMap(RoomData[] rooms)
    //{
    //    List<List<MapNode>> temp = new List<List<MapNode>>();
    //    int currentNodeIndex = 0;

    //    if (rooms.Length <= 0) Debug.Log("[MapGenerationService] No Rooms given");

    //    for (int layerIndex = 0; layerIndex < rooms.Length; layerIndex++)
    //    {
    //        List<MapNode> layer = new();

    //        // Loop through nodes in layer usually
    //        MapNode newNode = new MapNode()
    //        {
    //            room = rooms[layerIndex],
    //            nodeIndex = currentNodeIndex,
    //            // Setup Connections - refer to Aesthosis??
    //            cleared = false
    //        };
    //        layer.Add(newNode);
    //        currentNodeIndex++;

    //        temp.Add(layer);

    //    }

    //    RunMap map = new RunMap()
    //    {
    //        //rows = temp,
    //        startNode = temp[0][0]
    //    };

    //    return map;
    //}

    public RunMap GenerateMapWithSeed(int seed) // Add chapter sorting later lol
    {
        RunMap map = new();

        var grid = RunSimpleRandomWalk(5);

        map.tiles = GenerateMapNodes(grid);
        map.startNode = map.tiles[Vector2Int.zero];

        ConnectNodes(map.tiles);
        // Get basic grid map

        // Assign rooms based on weight?

        return map;
    }

    #region RandomWalk

    public HashSet<Vector2Int> RunSimpleRandomWalk(int nodeCount)
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

        //if (!currentMap.Contains(pos + Direction.North.ToGridOffset()))
        //    return false;
        //if (!currentMap.Contains(pos + Direction.South.ToGridOffset()))
        //    return false;
        //if (!currentMap.Contains(pos + Direction.East.ToGridOffset()))
        //    return false;
        //if (!currentMap.Contains(pos + Direction.West.ToGridOffset()))
        //    return false;

        //return true;
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
                room = roomDatabase.GetRandom(rngSeed + nodeIndex), // Replace with weighted function?
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

                if (map.TryGetValue(kvp.Key + direction.ToGridOffset(), out MapNode adjacent))
                {
                    node.ConnectTo(adjacent, direction);
                }
            }
        }
    }

    #endregion

    // Connect rooms
}
