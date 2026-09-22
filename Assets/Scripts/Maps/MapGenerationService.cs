using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MapGenerationService
{   
    private RoomDatabase db;
    
    private MapGenerationConfig config;
    
    private Vector2Int bounds;
    private int nodeCount;

    private float baseDifficulty;
    private float difficultyPerDepth;

    public MapGenerationService(RoomDatabase data, MapGenerationConfig config)
    {
        this.db = data;

        this.config = config;

        this.bounds = config.zoneSize;
        this.nodeCount = config.roomCount;
        this.baseDifficulty = config.baseDifficulty;
        this.difficultyPerDepth = config.difficultyPerDepth;
    }


    private float DifficultyCurve(int depth) => baseDifficulty + depth * difficultyPerDepth;


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
        AssignRewards(rows, seed);


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

            float val1 = Mathf.Lerp(0.2f, 0.6f, (float)RNGManager.Instance.rng.NextDouble());
            float val2 = Mathf.Lerp(0.4f, 0.8f, (float)RNGManager.Instance.rng.NextDouble());

            float widthT = 1f - Mathf.Abs(t - val1) / val2;
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
            var current = queue.Dequeue();
            if (!visited.Add(current.id)) continue;
            foreach (var connection in current.connections2)
                queue.Enqueue(connection);
        }

        if (!rows[^1].All(n => visited.Contains(n.id)))
            throw new InvalidOperationException("[MapGenerationService] Boss row unreachable, regenerating...");
    }

    // Room assignment

    private void AssignRoomTypes(List<List<MapNode>> rows)
    {
        rows[0].ForEach(n => n.type = RoomType.Start);
        rows[^1].ForEach(n => n.type = RoomType.End);

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
        var eligibleRows = Enumerable.Range(2, rows.Count - 2).ToList(); // exclude first / last

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

    private RoomData WeightedPick(List<RoomData> candidates, float targetDifficulty)
    {
        if (candidates.Count == 1) return candidates[0];

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
            if (roll <= cumulative) return candidates[i];
        }

        return candidates[^1];
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

    private void AssignRewards(List<List<MapNode>> rows, int seed)
    {
        var rng = new System.Random(seed ^ 0x5EED);

        foreach (var row in rows)
            foreach (var node in row)
                node.rewards = AssignRewardType(node.type, node.depth);
    }



    private NodeRewards AssignRewardType(RoomType room, int depth)
    {
        var rewards = new NodeRewards();
        rewards.isShop = false;

        switch (room)
        {
            case RoomType.Start:
                rewards.category = RewardCategory.Weapon;
                rewards.baseOfferCount = 3;
                break;

            case RoomType.Elite:
                rewards.category = RewardCategory.MajorUpgrade;
                rewards.baseOfferCount = 3;
                break;

            case RoomType.Story:
            case RoomType.Combat:
                rewards.category = RollWeightedReward(room, depth);
                rewards.baseOfferCount = 3;
                break;

            case RoomType.Shop:
                rewards.category = RollWeightedReward(room, depth);
                rewards.baseOfferCount = 6;
                rewards.isShop = true;
                break;

            case RoomType.Rest:
            case RoomType.End:
            default:
                break;
        }

        return rewards;
    }

    private RewardCategory RollWeightedReward(RoomType type, int depth)
    {
        var weights = new (RewardCategory type, float weight)[]
        {
            (RewardCategory.AuxUpgrade, type == RoomType.Shop ? 0.5f : 0.9f),
            (RewardCategory.MajorUpgrade, 0.15f + (0.01f * depth)),
            (RewardCategory.Weapon, 0.1f),
            (RewardCategory.Tool, 0.1f)
        };

        float total = weights.Sum(w => w.weight);
        float roll = (float)(RNGManager.Instance.rng.NextDouble() * total);
        float cumulative = 0f;

        foreach (var (category, weight) in weights)
        {
            cumulative += weight;
            if (roll <= cumulative)
                return category;
        }

        return RewardCategory.AuxUpgrade; // fallback
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

    private void PrintDebugMap(List<List<MapNode>> rows)
    {
        Debug.Log(rows);
    }

    #endregion

    // Connect rooms
}
