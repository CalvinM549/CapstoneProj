using System.Collections;
using UnityEngine;
using static UnityEngine.Analytics.IAnalytic;

public class MeleeAttackExecutor : AttackExecutorBase
{
    [SerializeField] private EnemyMeleeHitbox hitbox;

    private void OnEnable()
    {
        hitbox.OnHitDetected += OnHitboxHit;
    }

    private void OnDisable()
    {
        hitbox.OnHitDetected -= OnHitboxHit;
    }

    public override void BeginTelegraph(EnemyAIController ai)
    {
        if (!string.IsNullOrEmpty(attackData.windupTrigger))
            animator.SetTrigger(attackData.windupTrigger);
    }

    public override void Execute(EnemyAIController ai)
    {
        Executing = true;

        if (attackData.hasHyperarmour)
            controller.SetHyperArmour(true);

        if (!string.IsNullOrEmpty(attackData.activeTrigger))
            animator.SetTrigger(attackData.activeTrigger);

        hitbox.Activate();
        StartCoroutine(FinishAfterWinddown());

        Executing = false;
    }

    private IEnumerator FinishAfterWinddown()
    {
        yield return new WaitForSeconds(attackData.winddownDuration);
        hitbox.Deactivate();

        if (attackData.hasHyperarmour)
            controller.SetHyperArmour(false);

        Executing = false;
    }

    public override void Interrupt(EnemyAIController ai)
    {
        StopAllCoroutines();
        hitbox.Deactivate();

        if(attackData.hasHyperarmour)
            controller.SetHyperArmour(false);

        Executing = false;
    }

    private void OnHitboxHit(Collider2D collision)
    {
        if (!collision.TryGetComponent<IDamageable>(out var target)) return;

        HitData hit = new HitData(attackData.baseDamage, attackData.attackType, isPlayerAttack: false)
        {
            sourcePos = transform.position,
            knockbackDirection = ((Vector2)collision.transform.position - (Vector2)transform.position).normalized,
            knockbackForce = attackData.knockbackForce,

            hitstunTime = attackData.hitstunTime,
            hitstopTime = attackData.hitstopTime,

            isBlockable = attackData.isBlockable,
            isParryable = attackData.isParryable
        };

        hit.AddModifier(controller.Stats.Get(StatRef.EnemyOutgoingDamageMult) - 1f, StatModType.PercentAdd, this);

        target.RecieveHit(hit);
    }
}
