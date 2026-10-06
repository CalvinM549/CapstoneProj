using System.Collections;
using UnityEngine;

public class VLSAttackExecutor : AttackExecutorBase, IProjectileEmitter
{
    [SerializeField] private Transform[] firePoints;
    [SerializeField] private GameObject telegraphPrefab;

    [SerializeField] private int missileCount;
    [SerializeField] private float missileScatter;
    [SerializeField] private float missileInterval;

    [SerializeField] private LayerMask groundLayer;

    [SerializeField] protected ProjectileData[] projectiles;
    public ProjectileData[] Projectiles => new[] { attackData.projectile };


    public bool IsPlayerProjectile => false;

    public bool PoolRequested { get; set; }

    private void OnEnable()
    {
        ProjectilePools.RequestPool(this);
    }

    private void OnDisable()
    {
        ProjectilePools.ReleasePool(this);
    }

    public override void BeginTelegraph(EnemyAIController ai)
    {
        animator.Play("FireReady");
        animator.SetBool("IsFiring", true);

        HasExecuted = false;
    }

    public override void Execute(EnemyAIController ai)
    {
        Executing = true;

        if (attackData.hasHyperarmour)
            controller.SetHyperArmour(true);

        StartCoroutine(BarrageRoutine(ai));
    }

    private IEnumerator BarrageRoutine(EnemyAIController ai)
    {
        Vector2[] targetPositions = new Vector2[missileCount];
       
        for (int i = 0; i < missileCount; i++)
        {
            bool targetFound = false;
            while (!targetFound)
            {
                Vector2 targetPos = (Random.insideUnitCircle * missileScatter) + (Vector2)ai.context.target.position;
                targetFound = Physics2D.OverlapPoint(targetPos, groundLayer) != null; 

                if (targetFound)
                {
                    targetPositions[i] = targetPos;
                }
            }
        }

        for (int i = 0; i < missileCount; i++)
        {
            var projectile = projectiles[0];
            var firePos = firePoints[i].position;
            var targetPos = targetPositions[i];

            FireSingle(projectile, firePos, targetPos);

            yield return new WaitForSeconds(missileInterval);
        }

        animator.SetBool("IsFiring", false);


        if (attackData.hasHyperarmour)
            controller.SetHyperArmour(false);

        Executing = false;
        HasExecuted = true;
    }

    protected void FireSingle(ProjectileData projectile, Vector2 firePos, Vector2 targetPos)
    {
        var missile = ProjectileManager.Instance.FireProjectile(projectile, null, firePos, Vector2.up);

        if (missile is MissileProjectile mp)
        {
            mp.InitializeAsMissile(targetPos, telegraphPrefab);
        }
    }

    public override void Interrupt(EnemyAIController ai)
    {
        if (attackData.hasHyperarmour)
            controller.SetHyperArmour(false);

        animator.SetBool("IsFiring", false);
        StopAllCoroutines();
    }
}
