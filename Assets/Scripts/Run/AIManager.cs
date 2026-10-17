using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    private List<EnemyAIController> activeEnemies = new(16);

    private Player playerRef;
    private bool initialized = false;

    public INavigation Nav { get; private set; }
    public void SetNav (INavigation nav) => Nav = nav;
    public void ClearNav(INavigation nav)
    {
        if (Nav == nav) Nav = null;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Initialize(Player playerRef)
    {
        if (initialized) return;

        this.playerRef = playerRef;

        initialized = true;
    }

    public void RegisterEnemy(EnemyAIController ai) => activeEnemies.Add(ai);
    public void UnregisterEnemy(EnemyAIController ai) => activeEnemies.Remove(ai);

    private void Update()
    {
        float now = Time.time;
        Nav?.Tick(now);

        for (int i = 0; i < activeEnemies.Count; i++)
        {
            var ai = activeEnemies[i];
            if (now >= ai.nextTickTime)
            {
                ai.Tick(ai.profile.tickInterval);
                ai.nextTickTime = now + ai.profile.tickInterval;
            }
        }
    }

    // Closest registered (living) enemy within range of 'from', optionally ignoring one
    public bool TryGetNearestEnemy(Vector2 from, float maxRange, out Transform nearest, Transform exclude = null)
    {
        nearest = null;
        float best = maxRange * maxRange;

        for (int i = 0; i < activeEnemies.Count; i++)
        {
            var ai = activeEnemies[i];
            if (ai == null || !ai.isActiveAndEnabled) continue;
            if (exclude != null && ai.Self == exclude) continue;

            float d = ((Vector2)ai.Self.position - from).sqrMagnitude;
            if (d > best) continue;

            best = d;
            nearest = ai.Self;
        }

        return nearest != null;
    }

    // All living enemies within radius of 'center' (results cleared first)
    public void GetEnemiesInRadius(Vector2 center, float radius, List<EnemyController> results)
    {
        results.Clear();
        float r2 = radius * radius;

        for (int i = 0; i < activeEnemies.Count; i++)
        {
            var ai = activeEnemies[i];
            if (ai == null || !ai.isActiveAndEnabled) continue;
            if (((Vector2)ai.Self.position - center).sqrMagnitude > r2) continue;

            var enemy = ai.GetComponent<EnemyController>();
            if (enemy != null && enemy.IsAlive) results.Add(enemy);
        }
    }

    private readonly RaycastHit2D[] losHitBuffer = new RaycastHit2D[1];

    public bool TryFindTarget(EnemyAIController ai, out Transform target)
    {
        target = playerRef.transform;
        float dist = Vector2.Distance(ai.Self.position, target.position);

        if (dist > ai.profile.aggroRange)
        {
            target = null;
            return false;
        }

        if (!ai.profile.requireLineOfSight)
            return true;

        Vector2 dir = (target.position - ai.Self.position).normalized;
        int hits = Physics2D.RaycastNonAlloc(ai.Self.position, dir, losHitBuffer, dist, ai.profile.obstacleLayer);
        return hits == 0;
    }


}
