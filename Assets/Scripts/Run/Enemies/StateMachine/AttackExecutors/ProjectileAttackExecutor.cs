using UnityEngine;

public class ProjectileAttackExecutor : AttackExecutorBase, IProjectileEmitter
{
    // Enemy weapon data object?

    [SerializeField] private ProjectileData[] projectiles;
    public ProjectileData[] Projectiles => projectiles;

    public bool IsPlayerProjectile => false;

    public bool PoolRequested {  get; set; }

    public override void BeginTelegraph(EnemyAIController ai)
    {
        ai.GetComponent<Animator>().SetTrigger("RangedWindup");
        // any other things
    }


    public override void Execute(EnemyAIController ai)
    {
        throw new System.NotImplementedException();
    }

    public override void CancelTelegraph(EnemyAIController ai)
    {
        throw new System.NotImplementedException();
    }
}
