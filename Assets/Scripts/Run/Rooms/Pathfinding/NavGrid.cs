using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// What the AI needs from navigation. Keeps states independent of the algorithm behind it,
/// so the grid/flow-field can be swapped (or supplemented with A*) without touching them.
/// </summary>
public interface INavigation
{
    /// <summary>Advance time-based work (flow field rebuilds). Called once per frame by AIManager.</summary>
    void Tick(float now);

    /// <summary>Direction an agent at 'from' should move to approach 'target', routed around walls. False if no route.</summary>
    bool TryGetMoveDirection(Transform target, Vector2 from, out Vector2 direction);

    /// <summary>True if a circle of the given radius can travel from -> to without touching anything on 'obstacles'.</summary>
    bool HasClearPath(Vector2 from, Vector2 to, float radius, LayerMask obstacles);
}

/// <summary>
/// Walkability grid baked from a room's tilemaps, plus one cached flow field per chase target
/// (the player, or e.g. a decoy). Each field is a single breadth-first search outward from the
/// target, so the cost of a rebuild is independent of how many enemies are following it.
/// </summary>
public class NavGrid : MonoBehaviour, INavigation
{
    [Header("Bake Sources")]
    [Tooltip("A cell must have a tile on one of these to be walkable.")]
    [SerializeField] private Tilemap[] floorTilemaps;
    [Tooltip("Tiles on these block movement (walls, pits). Leave empty if walls are only colliders.")]
    [SerializeField] private Tilemap[] blockingTilemaps;
    [Tooltip("Colliders on these layers also block cells (props, wall colliders). Usually matches AIProfile.obstacleLayer.")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private bool bakeOnStart = true;

    [Header("Agents")]
    [Tooltip("Blocked areas grow by this many cells so paths keep clear of walls. Roughly ceil(largest enemy radius / cell size). " +
             "Corridors narrower than (2 * this + 1) cells become impassable.")]
    [SerializeField, Min(0)] private int inflationCells = 1;

    [Header("Flow Fields")]
    [SerializeField, Min(0.02f)] private float minRebuildInterval = 0.15f;
    [Tooltip("Fields nobody has queried for this long stop rebuilding until they're used again.")]
    [SerializeField, Min(0.1f)] private float idleFieldTimeout = 1f;

    private Grid grid;
    private Vector3Int origin; // cell coordinate of array index (0,0)
    private int width, height;
    private bool[] walkable;   // after inflation
    private bool baked;

    private readonly List<FlowField> fields = new();
    private readonly RaycastHit2D[] castBuffer = new RaycastHit2D[1];

    private void Start()
    {
        if (bakeOnStart)
            Bake();
    }

    private void OnDestroy()
    {
        if (AIManager.Instance != null)
            AIManager.Instance.ClearNav(this);
    }

    #region Baking

    /// <summary>
    /// Builds the walkability grid from the tilemaps and registers this grid with AIManager.
    /// For runtime-generated rooms, disable bakeOnStart and call this once the room's tiles/colliders exist.
    /// </summary>
    [ContextMenu("Bake")]
    public void Bake()
    {
        if (floorTilemaps == null || floorTilemaps.Length == 0 || floorTilemaps[0] == null)
        {
            Debug.LogError($"[NavGrid] No floor tilemaps assigned on {name}.", this);
            return;
        }

        Physics2D.SyncTransforms(); // make sure freshly spawned wall colliders are visible to the overlap checks
        grid = floorTilemaps[0].layoutGrid;

        // Combined floor bounds, padded by 1 so the outer ring is always blocked.
        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        foreach (var tm in floorTilemaps)
        {
            if (tm == null) continue;
            tm.CompressBounds();
            BoundsInt b = tm.cellBounds;
            if (b.size.x == 0 || b.size.y == 0) continue;

            minX = Mathf.Min(minX, b.xMin); minY = Mathf.Min(minY, b.yMin);
            maxX = Mathf.Max(maxX, b.xMax); maxY = Mathf.Max(maxY, b.yMax);
        }

        if (minX > maxX)
        {
            Debug.LogError($"[NavGrid] Floor tilemaps on {name} contain no tiles.", this);
            return;
        }

        origin = new Vector3Int(minX - 1, minY - 1, 0);
        width = (maxX - minX) + 2;
        height = (maxY - minY) + 2;

        var raw = new bool[width * height];
        Vector2 probeSize = (Vector2)grid.cellSize * 0.9f;
        int walkableCount = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var cell = new Vector3Int(origin.x + x, origin.y + y, 0);

                if (!HasTileIn(floorTilemaps, cell)) continue;
                if (HasTileIn(blockingTilemaps, cell)) continue;
                if (Physics2D.OverlapBox(grid.GetCellCenterWorld(cell), probeSize, 0f, obstacleLayer) != null) continue;

                raw[y * width + x] = true;
                walkableCount++;
            }
        }

        if (walkableCount == 0)
            Debug.LogWarning($"[NavGrid] Bake on {name} found no walkable cells - check tilemap/layer assignments.", this);

        walkable = inflationCells > 0 ? Inflate(raw, inflationCells) : raw;
        fields.Clear();
        baked = true;

        if (AIManager.Instance != null)
            AIManager.Instance.SetNav(this);
    }

    private static bool HasTileIn(Tilemap[] maps, Vector3Int cell)
    {
        if (maps == null) return false;
        for (int i = 0; i < maps.Length; i++)
        {
            if (maps[i] != null && maps[i].HasTile(cell))
                return true;
        }
        return false;
    }

    private bool[] Inflate(bool[] source, int radius)
    {
        var result = (bool[])source.Clone();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (source[y * width + x]) continue;

                for (int dy = -radius; dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if ((uint)nx < (uint)width && (uint)ny < (uint)height)
                            result[ny * width + nx] = false;
                    }
                }
            }
        }

        return result;
    }

    #endregion

    #region INavigation

    public void Tick(float now)
    {
        if (!baked) return;

        for (int i = fields.Count - 1; i >= 0; i--)
        {
            FlowField field = fields[i];

            if (field.Target == null) // e.g. a destroyed decoy
            {
                fields.RemoveAt(i);
                continue;
            }

            if (now - field.LastQueryTime > idleFieldTimeout) continue; // nobody is following this target right now
            if (now - field.LastBuildTime < minRebuildInterval) continue;
            if (!TryGetCell(field.Target.position, out int x, out int y)) continue;
            if (field.IsBuiltFor(x, y)) continue; // target hasn't changed cells

            field.Build(x, y, now);
        }
    }

    public bool TryGetMoveDirection(Transform target, Vector2 from, out Vector2 direction)
    {
        direction = Vector2.zero;
        if (!baked || target == null) return false;
        if (!TryGetCell(from, out int x, out int y)) return false;

        FlowField field = GetOrCreateField(target);
        field.LastQueryTime = Time.time;

        if (!field.TryGetNextCell(x, y, out int nextX, out int nextY)) return false;

        Vector2 delta = (Vector2)grid.GetCellCenterWorld(new Vector3Int(origin.x + nextX, origin.y + nextY, 0)) - from;
        if (delta.sqrMagnitude < 0.0001f) return false;

        direction = delta.normalized;
        return true;
    }

    public bool HasClearPath(Vector2 from, Vector2 to, float radius, LayerMask obstacles)
    {
        Vector2 delta = to - from;
        float distance = delta.magnitude;
        if (distance < 0.001f) return true;

        var filter = new ContactFilter2D(); // triggers ignored by default
        filter.SetLayerMask(obstacles);

        return Physics2D.CircleCast(from, radius, delta / distance, filter, castBuffer, distance) == 0;
    }

    #endregion

    private FlowField GetOrCreateField(Transform target)
    {
        for (int i = 0; i < fields.Count; i++)
        {
            if (fields[i].Target == target)
                return fields[i];
        }

        var field = new FlowField(target, walkable, width, height, inflationCells + 2);
        fields.Add(field);

        if (TryGetCell(target.position, out int x, out int y))
            field.Build(x, y, Time.time);

        return field;
    }

    private bool TryGetCell(Vector2 world, out int x, out int y)
    {
        Vector3Int cell = grid.WorldToCell(world);
        x = cell.x - origin.x;
        y = cell.y - origin.y;
        return (uint)x < (uint)width && (uint)y < (uint)height;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!baked || grid == null) return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
        Vector3 size = grid.cellSize * 0.95f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!walkable[y * width + x])
                    Gizmos.DrawCube(grid.GetCellCenterWorld(new Vector3Int(origin.x + x, origin.y + y, 0)), size);
            }
        }
    }
#endif
}

/// <summary>
/// Distance-to-target for every walkable cell, from one breadth-first search. Agents query it by
/// stepping to whichever neighbouring cell has the lowest distance. Arrays are allocated once.
/// </summary>
internal sealed class FlowField
{
    public readonly Transform Target;
    public float LastQueryTime;
    public float LastBuildTime = float.NegativeInfinity;
    public bool Built { get; private set; }

    private readonly bool[] walkable;
    private readonly int width, height, seedSearchRadius;
    private readonly int[] dist;  // -1 = unreachable
    private readonly int[] queue;

    private int builtForX = -1, builtForY = -1;

    public FlowField(Transform target, bool[] walkable, int width, int height, int seedSearchRadius)
    {
        Target = target;
        this.walkable = walkable;
        this.width = width;
        this.height = height;
        this.seedSearchRadius = seedSearchRadius;

        dist = new int[walkable.Length];
        queue = new int[walkable.Length];
    }

    public bool IsBuiltFor(int x, int y) => Built && x == builtForX && y == builtForY;

    public void Build(int targetX, int targetY, float now)
    {
        LastBuildTime = now;
        builtForX = targetX;
        builtForY = targetY;
        Built = false;

        System.Array.Fill(dist, -1);

        if (!TryResolveSeed(targetX, targetY, out int seed)) return;

        int head = 0, tail = 0;
        dist[seed] = 0;
        queue[tail++] = seed;

        while (head < tail)
        {
            int current = queue[head++];
            int cx = current % width, cy = current / width;
            int next = dist[current] + 1;

            Visit(cx + 1, cy, next, ref tail);
            Visit(cx - 1, cy, next, ref tail);
            Visit(cx, cy + 1, next, ref tail);
            Visit(cx, cy - 1, next, ref tail);
        }

        Built = true;
    }

    /// <summary>
    /// Best neighbouring cell (8-way, no corner cutting) that gets closer to the target.
    /// Works from unreachable cells too (e.g. an enemy knocked into an inflated wall margin),
    /// stepping to any reachable neighbour. False if already at the target or no route exists.
    /// </summary>
    public bool TryGetNextCell(int x, int y, out int nextX, out int nextY)
    {
        nextX = nextY = 0;
        if (!Built) return false;

        int own = dist[y * width + x];
        if (own == 0) return false;

        int best = own > 0 ? own : int.MaxValue;
        bool found = false;

        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = x + dx, ny = y + dy;
                if ((uint)nx >= (uint)width || (uint)ny >= (uint)height) continue;

                int d = dist[ny * width + nx];
                if (d < 0 || d >= best) continue;

                if (dx != 0 && dy != 0 && (!IsWalkable(x + dx, y) || !IsWalkable(x, y + dy))) continue;

                best = d;
                nextX = nx;
                nextY = ny;
                found = true;
            }
        }

        return found;
    }

    private void Visit(int x, int y, int d, ref int tail)
    {
        if ((uint)x >= (uint)width || (uint)y >= (uint)height) return;

        int i = y * width + x;
        if (!walkable[i] || dist[i] >= 0) return;

        dist[i] = d;
        queue[tail++] = i;
    }

    private bool IsWalkable(int x, int y)
    {
        return (uint)x < (uint)width && (uint)y < (uint)height && walkable[y * width + x];
    }

    // If the target is standing in a blocked/inflated cell (hugging a wall), seed from the nearest walkable one.
    private bool TryResolveSeed(int x, int y, out int seed)
    {
        seed = -1;

        for (int r = 0; r <= seedSearchRadius; r++)
        {
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != r) continue;

                    int nx = x + dx, ny = y + dy;
                    if ((uint)nx >= (uint)width || (uint)ny >= (uint)height) continue;

                    int i = ny * width + nx;
                    if (!walkable[i]) continue;

                    seed = i;
                    return true;
                }
            }
        }

        return false;
    }
}