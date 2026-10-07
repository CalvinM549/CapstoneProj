using System.Collections;
using UnityEngine;

public class ProjectileAttackExecutor : AttackExecutorBase, IProjectileEmitter
{
    [SerializeField] protected Transform firePoint;

    [SerializeField] protected VFXType shootFlashFX;
    [SerializeField] private float sightStartOffset;

    [SerializeField] private LaserTelegraph laser;
    [SerializeField] private StatusEffectData applyStatusOnHit;

    private Transform targetPos;
    private EnemyAIController aiController;

    // Interface
    public ProjectileData[] Projectiles => new[] { attackData.projectile };

    public bool IsPlayerProjectile => false;
    public bool PoolRequested {  get; set; }

    private void OnEnable() => ProjectilePools.RequestPool(this);

    private void OnDisable() => ProjectilePools.ReleasePool(this);

    public override void BeginTelegraph(EnemyAIController ai)
    {
        targetPos = ai.context.target;
        aiController = ai;
        HasExecuted = false;

        if (laser != null)
            laser.DoTarget(targetPos, 3f, attackData.telegraphDuration);

        if(!string.IsNullOrEmpty(attackData.windupTrigger))
            animator.SetTrigger(attackData.windupTrigger);
    }


    public override void Execute(EnemyAIController ai)
    {
        Executing = true;
        
        if(attackData.hasHyperarmour)
            controller.SetHyperArmour(true);

        StartCoroutine(FireRoutine(ai));
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
        hitData.AddStatus(applyStatusOnHit, controller, false);

        ProjectilePools.FireProjectile(this, projectile, hitData, firePoint.position, direction);
    }

    public override void Interrupt(EnemyAIController ai)
    {
        StopAllCoroutines();

        if (attackData.hasHyperarmour)
            controller.SetHyperArmour(false);

        if (laser != null)
            laser.CancelTarget();

        Executing = false;
        HasExecuted = false;
    }
}
