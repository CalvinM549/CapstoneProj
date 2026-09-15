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

    public MapGenerationService(RoomDatabase data, MapGenerationConfig config)
    {
        this.db = data;

        this.bounds = config.zoneSize;
        this.nodeCount = config.roomCount;
        this.walkerCount = Mathf.Max(1, walkerCount);
        this.baseDifficulty = config.baseDifficulty;
        this.difficultyPerDepth = config.difficultyPerDepth;
    }


    public SectorMap GenerateMapWithSeed(int seed) // Add chapter sorting later lol
    {
        SectorMap map = new();

        RNGManager.Instance.InitRNG(seed);

        var grid = RunSimpleRandomWalk();

        map.tiles = GenerateMapNodes(grid);
        map.entryNode = map.tiles[Vector2Int.zero];

        ConnectNodes(map.tiles);
        // Get basic grid map

        // Assign rooms based on weight?

        return map;
    }

    public SectorMap GenerateMapWithSeed(int seed, int chapter = 0) // Add chapter sorting later lol
    {
        SectorMap map = new();

        RNGManager.Instance.InitRNG(seed);

        var grid = RunComplexRandomWalk();
        var analysis = AnalyzeGrid(grid);

        map.tiles = GenerateMapNodes(grid, analysis, chapter);
        map.entryNode = map.tiles[Vector2Int.zero];
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

    private Dictionary<Vector2Int, MapNode> GenerateMapNodes(HashSet<Vector2Int> grid, GridAnalysis analysis, int chapter)
    {
        Dictionary<Vector2Int, MapNode> map = new();

        Vector2Int endPos = analysis.depth.Aggregate((a, b) => b.Value > a.Value ? b : a).Key;

        foreach (var square in grid)
        {
            DirectionMask requiredDoors = analysis.requiredDirections.GetValueOrDefault(square);
            int depth = analysis.depth.GetValueOrDefault(square);

            RoomType category = DetermineCategory(square, endPos);
            RoomData room = SelectRoom(category, requiredDoors, depth, chapter);

            map[square] = new MapNode
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

        // Implement more room type detemining
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

    private void ConnectNodes(Dictionary<Vector2Int, MapNode> map)
    {
        foreach (var kvp in map)
        {
            Vector2Int tile = kvp.Key;
            MapNode node = kvp.Value;

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (node.connections.ContainsKey(direction)) continue;
                if (!map.TryGetValue(tile + direction.ToGridOffset(), out MapNode adjacent)) continue;

                if(!node.room.CanConnectDirection(direction)) continue;
                if(!adjacent.room.CanConnectDirection(direction.Opposite())) continue;

                node.ConnectTo(adjacent, direction);
            }
        }
    }

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

    #region Web generation

    public SectorMap GenerateWithSeed(int seed, int chapter = 0)
    {
        const int maxAttempts = 5;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                return TryGenerate(seed + attempt, chapter);
            }
            catch (InvalidOperationException)
            {
                if (attempt == maxAttempts - 1) throw;
            }
        }

        throw new InvalidOperationException("[MapGenerationService] Failed to generate valid map");
    }

    private SectorMap TryGenerate(int seed, int chapter)
    {
        RNGManager.Instance.InitRNG(seed);

        int[] widthCurve = BuildWidthCurve();
        var rows = BuildRows(widthCurve);

        ConnectRows(rows);
        ValidateConnectivity(rows);

        AssignRoomTypes(rows);
        AssignRooms(rows, chapter);
        // assign rooms

        return BuildSectorMap(rows, seed);
    }

    private int[] BuildWidthCurve()
    {
        int rowCount = Mathf.Max(3, nodeCount);
        int maxWidth = Mathf.Max(1, bounds.y);

        int[] curve = new int[rowCount];
        for (int i = 0; i < rowCount; i++)
        {
            float t = (float)i / (rowCount - 1);
            // peaks a bit before the midpoint, tapers toward both ends
            float widthT = 1f - Mathf.Abs(t - 0.4f) / 0.6f;
            curve[i] = Mathf.RoundToInt(Mathf.Lerp(1, maxWidth, Mathf.Clamp01(widthT)));
        }

        curve[0] = 1;
        curve[^1] = 1;
        return curve;
    }

    private List<List<MapNode>> BuildRows(int[] widthCurve)
    {
        List<List<MapNode>> rows = new();

        for (int depth = 0; depth < widthCurve.Length; depth++)
        {
            var row = new List<MapNode>();

            for (int i = 0; i < widthCurve[depth]; i++)
            {
                MapNode node = new()
                {
                    id = $"{depth}_{i}",
                    depth = depth,
                    rowIndex = i,
                    coordinates = new Vector2Int(depth, i)
                };
                row.Add(node);
            }
            rows.Add(row);
        }

        return rows;
    }

    //Connections
    private void ConnectRows(List<List<MapNode>> rows)
    {
        for (int depth = 0; depth < rows.Count - 1; depth++)
        {
            var current = rows[depth];
            var next = rows[depth + 1];

            foreach (var node in current)
            {
                int connectionCount = RNGManager.Instance.rng.Next(1,3);
                node.connections2 = PickNearbyTargets(node, current.Count, next, connectionCount);
            }

            CheckRowCoverage(current, next);
        }
    }

    private List<MapNode> PickNearbyTargets(MapNode from, int rowSize, List<MapNode> nextRow, int count)
    {
        float t = rowSize <= 1 ? 0f : (float)from.rowIndex / (rowSize - 1);
        int projected = Mathf.RoundToInt(t * (nextRow.Count - 1));

        var candidates = nextRow
            .Where(n => Mathf.Abs(n.rowIndex - projected) <= 1)
            .OrderBy(_ => RNGManager.Instance.rng.Next())
            .Take(count)
            .ToList();

        if(candidates.Count == 0)
            candidates.Add(nextRow[projected]);

        return candidates;
    }

    private void CheckRowCoverage(List<MapNode> current, List<MapNode> next)
    {
        var covered = new HashSet<string>(current.SelectMany(n => n.connections2.Select(c => c.id)));

        foreach (var orphan in next.Where(n => !covered.Contains(n.id)))
        {
            var closest = current.OrderBy(n => Mathf.Abs(n.rowIndex - orphan.rowIndex)).First();
            closest.connections2.Add(orphan);
        }
    }

    private void ValidateConnectivity(List<List<MapNode>> rows)
    {
        var visited = new HashSet<string>();
        var queue = new Queue<MapNode>();
        queue.Enqueue(rows[0][0]);

        while (queue.Count > 0)
        {
            var n = queue.Dequeue();
            if (!visited.Add(n.id)) continue;
            foreach (var connection in n.connections2)
                queue.Enqueue(connection);
        }

        if (!rows[^1].All(n => visited.Contains(n.id)))
            throw new InvalidOperationException("[MapGenerationService] Boss row unreachable, regenerating...");
    }

    // Room assignment

    private void AssignRoomTypes(List<List<MapNode>> rows)
    {
        rows[0].ForEach(n => n.type = RoomType.Start);
        rows[^1].ForEach(n => n.type = RoomType.Boss);

        PlaceGuaranteed(rows, RoomType.Rest, count: 1);
        PlaceGuaranteed(rows, RoomType.Shop, count: 1);

        for (int depth = 0; depth < rows.Count; depth++)
        {
            foreach (var node in rows[depth].Where(n => n.type == default))
            {
                node.type = RollWeightedType(depth, rows.Count);
            }
        }
    }

    private void PlaceGuaranteed(List<List<MapNode>> rows, RoomType type, int count)
    {
        var eligibleRows = Enumerable.Range(1, rows.Count - 2).ToList(); // exclude first / last

        for (int i = 0; i < count && eligibleRows.Count > 0; i++)
        {
            int rowIndex = eligibleRows[RNGManager.Instance.rng.Next(eligibleRows.Count)];
            var unassigned = rows[rowIndex]
                .Where(n => n.type == default)
                .ToList();

            if (unassigned.Count == 0)
            {
                eligibleRows.Remove(rowIndex);
                i--;
                continue;
            }

            unassigned[RNGManager.Instance.rng.Next(unassigned.Count)].type = type;
            eligibleRows.Remove(rowIndex);
        }
    }

    private RoomType RollWeightedType(int depth, int totalRows)
    {
        float progress = (float)depth / totalRows;

        var weights = new (RoomType type, float weight)[]
        {
            (RoomType.Combat, 0.6f),
            (RoomType.Story, 0.15f),
            (RoomType.Vault, 0.1f + progress * 0.15f) // replace all with variables
        };

        float total = weights.Sum(w => w.weight);
        float roll = (float)(RNGManager.Instance.rng.NextDouble() * total);
        float cumulative = 0f;

        foreach (var (type, weight) in weights)
        {
            cumulative += weight;
            if (roll <= cumulative) 
                return type;
        }

        return RoomType.Combat; // fallback
    }

    
    private void AssignRooms(List<List<MapNode>> rows, int chapter)
    {
        foreach (var row in rows)
        {
            foreach (var node in row)
            {
                node.room = SelectRoom(node.type, node.depth, chapter);
            }
        }
    }

    private RoomData SelectRoom(RoomType type, int depth, int chapter)
    {
        var candidates = db.GetCandidates(type, chapter).ToList();

        if(candidates.Count == 0)
        {
            Debug.LogError($"[MapGenerationService] No maps of type: {type}");
            return db.GetByCategory(type).FirstOrDefault();
        }

        float targetDifficulty = DifficultyCurve(depth);
        return WeightedPick(candidates, targetDifficulty);
    }

    // sector map
    private SectorMap BuildSectorMap(List<List<MapNode>> rows, int seed)
    {
        SectorMap map = new()
        {
            seed = seed,
            rowCount = rows.Count
        };

        foreach (var row in rows)
        {
            foreach (var node in row)
            {
                map.nodes.Add(node);
                map.nodesByID[node.id] = node;
            }
        }

        map.entryNode = rows[0][0];
        map.currentNode = map.entryNode;
        map.totalNodes = map.nodes.Count;

        return map;
    }

    #endregion

    // Connect rooms
}
