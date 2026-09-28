using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface INavigation
{
    void Tick(float dt);
    bool TryGetMoveDirection(Transform target, Vector2 from, out Vector2 direction);
    bool HasClearPath(Vector2 fro, Vector2 to, float radius, LayerMask obstacles);
}

public class NavGrid : MonoBehaviour, INavigation
{
    [Header("BakeSources")]

    [SerializeField] private Tilemap[] floorTilemaps;
    [SerializeField] private Tilemap[] obstacleTilemaps;

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

    //private readonly List<FlowField> fields = new();
    //private readonly RaycastHit2D[] castBuffer = new RaycastHit2D[1];

    //private void Start()
    //{
    //    if (bakeOnStart)
    //        Bake();
    //}

    //private void OnDestroy()
    //{
    //    if (AIManager.Instance != null)
    //        AIManager.Instance.ClearNav(this);
    //}

    public bool HasClearPath(Vector2 fro, Vector2 to, float radius, LayerMask obstacles)
    {
        throw new System.NotImplementedException();
    }

    public void Tick(float dt)
    {
        throw new System.NotImplementedException();
    }

    public bool TryGetMoveDirection(Transform target, Vector2 from, out Vector2 direction)
    {
        throw new System.NotImplementedException();
    }
}
