using System.Collections;
using UnityEngine;

public class ProjectileAttackExecutor : AttackExecutorBase, IProjectileEmitter
{
    [SerializeField] protected Transform firePoint;

    [SerializeField] protected VFXType shootFlashFX;
    [SerializeField] protected LineRenderer laserSight;

    private Transform targetPos;
    protected Vector2 laserAdjustment;

    // Interface
    public ProjectileData[] Projectiles => new[] { attackData.projectile };

    public bool IsPlayerProjectile => false;
    public bool PoolRequested {  get; set; }

    private void OnEnable()
    {
        if(laserSight != null)
            laserSight.enabled = false;

        ProjectilePools.RequestPool(this);
    }

    private void OnDisable()
    {
        ProjectilePools.ReleasePool(this);
    }

    private void Update()
    {
        if (laserSight != null && laserSight.enabled && targetPos != null)
        {
            laserSight.SetPosition(1, (Vector2)targetPos.position + laserAdjustment);
            laserSight.SetPosition(0, firePoint.position);
        }
    }

    public override void BeginTelegraph(EnemyAIController ai)
    {
        targetPos = ai.context.target;
        HasExecuted = false;

        if (laserSight != null)
        {
            laserAdjustment = Random.insideUnitCircle * 0.5f;

            laserSight.enabled = true;
            laserSight.SetPosition(1, (Vector2)targetPos.position + laserAdjustment);
            laserSight.SetPosition(0, firePoint.localPosition);
        }

        if(!string.IsNullOrEmpty(attackData.windupTrigger))
            animator.SetTrigger(attackData.windupTrigger);

        //animator.Play("FireReady");
        // any other things
    }


    public override void Execute(EnemyAIController ai)
    {
        if (laserSight != null && laserSight.enabled)
            laserSight.enabled = false;

        Executing = true;
        
        if(attackData.hasHyperarmour)
            controller.SetHyperArmour(true);

        StartCoroutine(FireRoutine(ai));

        //animator.Play("ReturnFromFire");
    }

    protected IEnumerator FireRoutine(EnemyAIController ai)
    {
        for (int i = 0; i < attackData.projectileCount; i++)
        {
            Vector2 direction = ai.context.target != null
                ? (ai.context.target.position - firePoint.position).normalized
                : transform.right;

            VFXManager.Instance.PlayVFX(shootFlashFX, firePoint.position, direction);

            FireSingle(attackData.projectile, direction);

            yield return new WaitForSeconds(attackData.shotInterval);
        }

        if (attackData.hasHyperarmour)
            controller.SetHyperArmour(false);

        if (!string.IsNullOrEmpty(attackData.winddownTrigger))
            animator.SetTrigger(attackData.winddownTrigger);

        yield return new WaitForSeconds(attackData.winddownDuration);
        Executing = false;
        HasExecuted = true;
    }

    protected void FireSingle(ProjectileData projectile, Vector2 direction)
    {
        var hitData = new HitData(projectile.damage, AttackType.Projectile, isPlayerAttack: true)
        {
            sourcePos = firePoint.position,
            knockbackDirection = direction,
            knockbackForce = projectile.knockback,

            hitstopTime = projectile.hitstopDuration,
            hitstunTime = projectile.hitstunTime,

            isParryable = projectile.isParryable,
            isBlockable = projectile.isBlockable
        };

        hitData.AddModifier(controller.Stats.Get(StatRef.EnemyOutgoingDamageMult) - 1f, StatModType.PercentAdd, controller);

        ProjectilePools.FireProjectile(this, projectile, hitData, firePoint.position, direction);
    }

    public override void Interrupt(EnemyAIController ai)
    {
        StopAllCoroutines();

        if (attackData.hasHyperarmour)
            controller.SetHyperArmour(false);

        if(laserSight != null  && laserSight.enabled)
            laserSight.enabled = false;

        Executing = false;
        HasExecuted = false;
    }
}
