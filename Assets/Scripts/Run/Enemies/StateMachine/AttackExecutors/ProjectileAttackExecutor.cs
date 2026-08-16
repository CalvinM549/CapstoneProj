using System.Collections;
using UnityEngine;

public class ProjectileAttackExecutor : AttackExecutorBase, IProjectileEmitter
{
    // Enemy weapon data object?
    [SerializeField] protected Transform firePos;
    [SerializeField] protected float spreadAngle;

    [SerializeField] protected VFXType shootFlashFX;
    [SerializeField] protected LineRenderer laserSight;
    private Transform targetPos;

    [SerializeField] protected ProjectileData[] projectiles;
    public ProjectileData[] Projectiles => projectiles;

    public bool IsPlayerProjectile => false;
    public bool PoolRequested {  get; set; }

    protected bool hasFired = false;
    protected Vector2 laserAdjustment;

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
            laserSight.SetPosition(0, firePos.position);
        }
    }

    public override void BeginTelegraph(EnemyAIController ai)
    {
        targetPos = ai.context.target;
        if (laserSight != null)
        {
            laserAdjustment = Random.insideUnitCircle * 0.5f; 

            laserSight.enabled = true;
            laserSight.SetPosition(1, (Vector2)targetPos.position + laserAdjustment);
            laserSight.SetPosition(0, firePos.localPosition);
        }

        ai.GetComponentInChildren<Animator>().Play("FireReady");
        hasFired = false;
        // any other things
    }


    public override void Execute(EnemyAIController ai)
    {
        if (hasFired) return;

        if (laserSight != null && laserSight.enabled)
            laserSight.enabled = false;
            
        Executing = true;
        Vector2 dir = (Vector2)ai.context.target.position - ai.Body.position;
        FireSingle(projectiles[0], dir);
        Executing = false;
        hasFired = true;


        ai.GetComponentInChildren<Animator>().Play("ReturnFromFire");
    }

    protected void FireSingle(ProjectileData projectile, Vector2 direction)
    {
        ProjectilePools.FireProjectile(this, projectile, firePos.transform.position, direction);
        VFXManager.Instance.PlayVFX(shootFlashFX, transform.position, direction);
    }

    public override void Interrupt(EnemyAIController ai)
    {
        if(laserSight != null  && laserSight.enabled)
            laserSight.enabled = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, attackData.attackRange);
    }
#endif
}
