using UnityEngine;

public class MeleeAttackExecutor : AttackExecutorBase
{
    [SerializeField] private Transform hitOrigin;
    [SerializeField] private float hitRadius;
    [SerializeField] private int damage;
    [SerializeField] private float knockback;

    [SerializeField] private float hitStunTime;

    public override void BeginTelegraph(EnemyAIController ai)
    {
        // Do telegraph anim
    }

    public override void Execute(EnemyAIController ai)
    {
        Executing = true;

        var hits = Physics2D.OverlapCircleAll(hitOrigin.position, hitRadius, ai.profile.targetLayer);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            if (hit.TryGetComponent<IDamageable>(out var target))
            {
                Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;

                HandleHit(target, knockbackDir);
            }
        }

        Executing = false;
    }

    private void HandleHit(IDamageable target, Vector2 dir)
    {
        HitData hit = new HitData()
        {
            damage = this.damage,
            attackType = AttackType.Heavy,
            sourcePos = transform.position,
            knockbackDirection = dir,
            knockbackForce = this.knockback,
            hitstunTime = this.hitStunTime,

            isPlayerAttack = false,
            isParryable = true
        };

        target.RecieveHit(hit);
    }

    public override void Interrupt(EnemyAIController ai)
    {
        throw new System.NotImplementedException();
    }
}
