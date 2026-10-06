using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerationService
{
    private const int SaltLayout = 0x1A70;
    private const int SaltTypes = 0x7E57;
    private const int SaltRooms = 0x2007;
    private const int SaltRewards = 0x5EED;
    private const int SaltCode = 0xC0DE;

    private const string CodeChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";


    private readonly RoomDatabase db;
    private readonly RoomTypeDatabase typeDb;
    private readonly MapGenerationConfig config;

    private int MaxExits => Mathf.Max(1, config.maxExitsPerNode);

    public MapGenerationService(RoomDatabase roomData, RoomTypeDatabase typeData, MapGenerationConfig config)
    {
        this.db = roomData;
        this.typeDb = typeData;

        this.config = config;
    }

    private float DifficultyCurve(int depth) => config.baseDifficulty + depth * config.difficultyPerDepth;

    public static System.Random RNGStream(int seed, int salt)
    {
        unchecked { return new System.Random(seed * 397 ^ salt); }
    }

    #region Generation

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

        var layoutRng = RNGStream(seed, SaltLayout);

        int[] widthCurve = BuildWidthCurve(layoutRng);
        var rows = BuildRows(widthCurve);

        ConnectRows(rows, layoutRng);

        AssignTypes(rows, RNGStream(seed, SaltTypes));
        AssignRooms(rows, chapter, RNGStream(seed, SaltRooms));
        //AssignRewards(rows, seed);
        AssignOffers(rows, seed);

        var map = BuildSectorMap(rows, seed, chapter);
        Validate(map);
        return map;
    }

    private int[] BuildWidthCurve(System.Random rng)
    {
        int rowCount = Mathf.Max(3, config.roomCount);
        int maxWidth = Mathf.Max(1, config.maxWidth);


        int[] curve = new int[rowCount];
        for (int i = 0; i < rowCount; i++)
        {
            float t = (float)i / (rowCount - 1);
            float peak = Mathf.Lerp(0.2f, 0.6f, (float)rng.NextDouble());
            float spread = Mathf.Lerp(0.4f, 0.8f, (float)rng.NextDouble());

            float widthT = 1f - Mathf.Abs(t - peak) / spread;
            curve[i] = Mathf.RoundToInt(Mathf.Lerp(1, maxWidth, Mathf.Clamp01(widthT)));
        }

        curve[0] = 1;
        curve[^1] = 1;

        for (int i = 1; i < rowCount; i++)
            curve[i] = Mathf.Min(curve[i], curve[i - 1] * MaxExits);

        return curve;
    }

    private List<List<MapNode>> BuildRows(int[] widthCurve)
    {
        var rows = new List<List<MapNode>>(widthCurve.Length);

        for (int depth = 0; depth < widthCurve.Length; depth++)
        {
            var row = new List<MapNode>(widthCurve[depth]);

            for (int i = 0; i < widthCurve[depth]; i++)
            {
                row.Add( new MapNode
                {
                    id = $"{depth}_{i}",
                    depth = depth,
                    rowIndex = i,
                    coordinates = new Vector2Int(depth, i)
                });
            }
            rows.Add(row);
        }

        return rows;
    }

    #endregion

    #region Connection

    //Connections
    private void ConnectRows(List<List<MapNode>> rows, System.Random rng)
    {
        for (int depth = 0; depth < rows.Count - 1; depth++)
        {
            var current = rows[depth];
            var next = rows[depth + 1];

            foreach (var node in current)
            {
                int connectionCount = RNGManager.Instance.rng.Next(1,3);
                node.exits = PickNearbyTargets(node, current.Count, next, connectionCount);
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
        var covered = new HashSet<string>(current.SelectMany(n => n.exits.Select(c => c.id)));

        foreach (var orphan in next.Where(n => !covered.Contains(n.id)))
        {
            var closest = current.OrderBy(n => Mathf.Abs(n.rowIndex - orphan.rowIndex)).First();
            closest.exits.Add(orphan);
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
            foreach (var connection in current.exits)
                queue.Enqueue(connection);
        }

        if (!rows[^1].All(n => visited.Contains(n.id)))
            throw new InvalidOperationException("[MapGenerationService] Boss row unreachable, regenerating...");
    }

    #endregion

    #region Room Types

    private void AssignTypes(List<List<MapNode>> rows, System.Random rng)
    {
        int last = rows.Count - 1;
        var counts = new Dictionary<RoomTypeData, int>();

        foreach (var n in rows[0]) Assign(n, typeDb.GetStart());
        foreach (var n in rows[last]) Assign(n, typeDb.GetEnd());

        foreach (var t in typeDb.All.Where(t => t.guaranteedCount > 0))
        {
            for (int i = 0; i < t.guaranteedCount; i++)
            {
                var spot = PickGuaranteedSpot(t);
                if (spot == null)
                {
                    Debug.LogWarning($"[MapGenerationService] Could not place guaranteed '{t.displayName}' ({i + 1}/{t.guaranteedCount})");
                    break;
                }
                Assign(spot, t);
            }
        }

        for (int depth = 1; depth < last; depth++)
        {
            float progress = (float)depth / last;
            foreach (var node in rows[depth])
            {
                if (node.type != null) continue;

                var pool = typeDb.All
                    .Where(t => t.canRandomSpawn && Allowed(t, depth))
                    .Select(t => (type: t, weight: t.GetWeight(progress)))
                    .Where(x => x.weight > 0f)
                    .ToList();

                Assign(node, pool.Count > 0 ? WeightedPick(pool, rng) : typeDb.FallbackType);
            }
        }

        void Assign(MapNode node, RoomTypeData type)
        {
            node.type = type;
            counts[type] = counts.GetValueOrDefault(type) + 1;
        }

        bool Allowed(RoomTypeData type, int depth)
        {
            if (type.maxPerMap >= 0 && counts.GetValueOrDefault(type) >= type.maxPerMap) return false;
            int from = Mathf.Max(0, depth - type.minRowGap + 1);
            int to = Mathf.Min(last, depth + type.minRowGap - 1);

            for (int i = from; i <= to; i++)
                if (rows[i].Any(n => n.type == type)) return false;
            return true;
        }

        MapNode PickGuaranteedSpot(RoomTypeData type)
        {
            var eligible = new List<int>();
            for (int depth = 1; depth < last; depth++)
            {
                if ((float)depth / last < type.guarenteedMinProgress) continue;
                if (!Allowed(type, depth)) continue;
                if (rows[depth].Any(n => n.type == null)) eligible.Add(depth);
            }

            if(eligible.Count == 0) return null;

            var chokePoints = eligible.Where(d => rows[d].Count == 1).ToList();
            var pool = chokePoints.Count > 0 ? chokePoints : eligible;

            int row = pool[rng.Next(pool.Count)];
            var free = rows[row].Where(n => n.type == null).ToList();
            return free[rng.Next(free.Count)];
        }
    }

    #endregion

    #region Room Data
    private void AssignRooms(List<List<MapNode>> rows, int chapter, System.Random rng)
    {
        foreach (var row in rows)
        {
            foreach (var node in row)
            {
                node.room = SelectRoom(node, chapter, rng);
            }
        }
    }

    private RoomData SelectRoom(MapNode node, int chapter, System.Random rng)
    {
        int requiredExits = node.exits.Count;
        float target = DifficultyCurve(node.depth);

        var candidates = new List<(RoomData room, float weight)>();
        foreach (var r in db.GetCandidates(node.type, chapter))
        {
            if(r.roomPrefab == null) continue;
            if (r.roomPrefab.ExitCount < requiredExits) continue;

            candidates.Add((r, 1f / (1f + Mathf.Abs(r.difficultyCost - target))));
        }

        if(candidates.Count == 0)
        {
            Debug.LogError($"[MapGenerationService] No rooms of type: {node.type.displayName}");
        }

        return WeightedPick(candidates, rng);
    }

    #endregion

    #region Assign Rewards

    private void AssignRewards(List<List<MapNode>> rows, int seed)
    {

        foreach (var row in rows)
            foreach (var node in row)
            {
                var rng = new System.Random(seed ^ 0x5EED);
                node.offers = node.type.rewards.Resolve(node.depth, rng);
            }
    }

    private void AssignOffers(List<List<MapNode>> rows, int seed)
    {
        foreach(var row in rows)
            foreach (var node in row)
            {
                var rewardRng = RNGManager.NodeRng(seed, node.coordinates, SaltRewards);
                node.offers = node.type.rewards != null
                    ? node.type.rewards.Resolve(node.depth, rewardRng)
                    : Array.Empty<ResolvedOffer>();

                var codeRng = RNGManager.NodeRng(seed, node.coordinates, SaltCode);
                node.sectorCode = MakeCode(codeRng, codeRng.Next(3, 7));
            }
    }

    private static string MakeCode(System.Random rng, int length)
    {
        var chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = CodeChars[rng.Next(CodeChars.Length)];
        }

        return new string(chars);
    }

    #endregion

    #region Build Map

    // sector map
    private static SectorMap BuildSectorMap(List<List<MapNode>> rows, int seed, int chapter)
    {
        SectorMap map = new()
        {
            seed = seed,
            chapter = chapter,
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
        map.totalNodes = map.nodes.Count;

        return map;
    }

    private static void Validate(SectorMap map)
    {
        int lastDepth = map.rowCount - 1;

        foreach (var node in map.nodes)
        {
            if (node.depth < lastDepth && node.exits.Count == 0)
                throw new InvalidOperationException($"[MapGen] Node {node.id} is a dead end (seed {map.seed})");

            if (node.type == null || node.room == null)
                throw new InvalidOperationException($"[MapGen] Node {node.id} missing type or room (seed {map.seed})");
        }

        var visited = new HashSet<string>();
        var stack = new Stack<MapNode>();
        stack.Push(map.entryNode);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (!visited.Add(node.id)) continue;
            foreach(var e in  node.exits) stack.Push(e);
        }

        if(visited.Count != map.nodes.Count)
            throw new InvalidOperationException($"[MapGen] Unreachable nodes (seed {map.seed})");
    }

    private static T WeightedPick<T>(IReadOnlyList<(T item, float weight)> entries, System.Random rng)
    {
        if (entries.Count == 1) return entries[0].item;

        float total = 0f;
        for (int i = 0; i < entries.Count; i++)
            total += entries[i].weight;

        float roll = (float)(rng.NextDouble() * total);
        for (int i = 0; i < entries.Count; i++)
        {
            roll -= entries[i].weight;
            if(roll <= 0f) 
                return entries[i].item;
        }

        return entries[^1].item;
    }

    #endregion
}
