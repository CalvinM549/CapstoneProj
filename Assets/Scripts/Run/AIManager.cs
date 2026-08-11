using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    private List<EnemyAIController> activeEnemies = new(16);

    private Player playerRef;
    private bool initialized = false;

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

        Vector2 dir = (target.position - ai.Self.position).normalized;
        int hits = Physics2D.RaycastNonAlloc(ai.Self.position, dir, losHitBuffer, dist, ai.profile.obstacleLayer);
        return hits == 0;
    }


}
