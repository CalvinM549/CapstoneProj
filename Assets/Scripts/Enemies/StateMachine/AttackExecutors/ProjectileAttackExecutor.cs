using System.Collections;
using UnityEngine;

public class ProjectileAttackExecutor : AttackExecutorBase, IProjectileEmitter
{
    [SerializeField] protected Transform firePoint;

    [SerializeField] protected VFXType shootFlashFX;
    [SerializeField] protected LineRenderer laserSight;

    private Transform targetPos;
    private EnemyAIController aiController;
    protected Vector2 laserAdjustment;

    private readonly RaycastHit2D[] laserHitBuffer = new RaycastHit2D[1];

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
        if (laserSight != null && laserSight.enabled)
            UpdateLaserSight();
    }

    private void UpdateLaserSight()
    {
        if(targetPos == null || aiController == null) return;

        Vector2 origin = firePoint.position;
        Vector2 targetPoint = (Vector2)targetPos.position + laserAdjustment;
        Vector2 toTarget = targetPoint - origin;
        float distance = toTarget.magnitude;

        Vector2 endPoint = targetPoint;

        if (distance > 0.0001f)
        {
            Vector2 direction = toTarget / distance;
            int hits = Physics2D.RaycastNonAlloc(origin, direction, laserHitBuffer, distance, aiController.profile.obstacleLayer);

            if (hits > 0)
                endPoint = laserHitBuffer[0].point;
        }

        laserSight.SetPosition(0, origin);
        laserSight.SetPosition(1, endPoint);
    }

    public override void BeginTelegraph(EnemyAIController ai)
    {
        targetPos = ai.context.target;
        aiController = ai;
        HasExecuted = false;

        if (laserSight != null && targetPos != null)
        {
            laserAdjustment = Random.insideUnitCircle * 0.5f;

            laserSight.enabled = true;
            UpdateLaserSight();
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
