using System.Collections;
using UnityEngine;

public class VLSAttackExecutor : AttackExecutorBase, IProjectileEmitter
{
    private const int maxTargetAttempts = 30;

    private readonly int FireReadyHash = Animator.StringToHash("FireReady");
    private readonly int IsFiringHash = Animator.StringToHash("IsFiring");

    [SerializeField] private Transform[] firePoints;
    [SerializeField] private MissileStrike strikePrefab;

    [SerializeField] private int missileCount;
    [SerializeField] private float missileScatter;
    [SerializeField] private float missileInterval;

    [SerializeField] private LayerMask groundLayer;

    public ProjectileData[] Projectiles => new[] { attackData.projectile };
    public bool IsPlayerProjectile => false;
    public bool PoolRequested { get; set; }

    private void OnEnable() => ProjectilePools.RequestPool(this);

    private void OnDisable() => ProjectilePools.ReleasePool(this);

    public override void BeginTelegraph(EnemyAIController ai)
    {
        animator.Play(FireReadyHash);
        animator.SetBool(IsFiringHash, true);

        HasExecuted = false;
    }

    public override void Execute(EnemyAIController ai)
    {
        Executing = true;

        if (attackData.hasHyperarmour)
            controller.SetHyperArmour(true);

        StartCoroutine(BarrageRoutine(ai));
    }

    public override void Interrupt(EnemyAIController ai)
    {
        StopAllCoroutines();
        StopFiring();
    }

    private IEnumerator BarrageRoutine(EnemyAIController ai)
    {
        Vector2[] targetPositions = GetTargets(ai.context.target.position);
        var wait = new WaitForSeconds(missileInterval);
       
        for (int i = 0; i < missileCount; i++)
        {
            Vector2 firePos = firePoints[i % firePoints.Length].position;
            FireSingle(firePos, targetPositions[i]);

            yield return wait;
        }

        StopFiring();

        Executing = false;
        HasExecuted = true;
    }

    protected void FireSingle(Vector2 firePos, Vector2 targetPos)
    {
        var projectile = attackData.projectile;

        var hitData = new HitData(projectile.damage, AttackType.Projectile, isPlayerAttack: true)
        {
            sourcePos = firePos,
            knockbackForce = projectile.knockback,

            hitstopTime = projectile.hitstopDuration,
            hitstunTime = projectile.hitstunTime,

            isParryable = projectile.isParryable,
            isBlockable = projectile.isBlockable
        };

        hitData.AddModifier(controller.Stats.Get(StatRef.EnemyOutgoingDamageMult) - 1f, StatModType.PercentAdd, controller);

        var missile = ProjectileManager.Instance.FireProjectile(attackData.projectile, hitData, firePos, Vector2.up);

        if (missile is VLSProjectile mp)
            mp.InitializeAsMissile(targetPos, strikePrefab);
    }

    private Vector2[] GetTargets(Vector2 center)
    {
        var targets = new Vector2[missileCount];

        for (int i = 0; i < missileCount; i++)
            targets[i] = FindGroundPoint(center);

        return targets;
    }

    private Vector2 FindGroundPoint(Vector2 center)
    {
        int currentAttempt = 0;
        while (currentAttempt < maxTargetAttempts)
        {
            currentAttempt++;

            Vector2 candidate = center + Random.insideUnitCircle * missileScatter;

            if (Physics2D.OverlapPoint(candidate, groundLayer) != null)
                return candidate;
        }

        return center;
    }

    private void StopFiring()
    {
        if (attackData.hasHyperarmour)
            controller.SetHyperArmour(false);

        animator.SetBool(IsFiringHash, false);
    }

}
