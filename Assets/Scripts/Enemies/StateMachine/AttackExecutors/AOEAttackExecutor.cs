using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEAttackExecutor : AttackExecutorBase
{
    [SerializeField] private LayerMask targetLayer;

    [SerializeField] private Transform attackOrigin;
    [SerializeField] private GameObject telegraphPrefab;

    private GameObject activeTelegraph;

    public override void BeginTelegraph(EnemyAIController ai)
    {
        if (!string.IsNullOrEmpty(attackData.windupTrigger))
            animator.SetTrigger(attackData.windupTrigger);

        if (attackData.hasHyperarmour)
            controller.SetHyperArmour(true);

        // Do vfx telegraph
        activeTelegraph = Instantiate(telegraphPrefab, attackOrigin);
    }

    public override void Execute(EnemyAIController ai)
    {
        Executing = true;

        if (!string.IsNullOrEmpty(attackData.activeTrigger))
        {
            animator.SetTrigger(attackData.activeTrigger);

        }

        DealAreaDamage();
        StartCoroutine(FinishAfterWinddown());

        Destroy(activeTelegraph);
    }

    private IEnumerator FinishAfterWinddown()
    {
        yield return new WaitForSeconds(attackData.winddownDuration);

        if (attackData.hasHyperarmour)
            controller.SetHyperArmour(false);

        Executing = false;
    }

    public override void Interrupt(EnemyAIController ai)
    {
        StopAllCoroutines();

        if(attackData.hasHyperarmour)
            controller.SetHyperArmour(false);

        Destroy(activeTelegraph);
        Executing = false;
    }

    private void DealAreaDamage()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, attackData.attackRange, targetLayer);
        var alreadyHit = new HashSet<IDamageable>();

        foreach (var col in hits)
        {
            if (!col.TryGetComponent<IDamageable>(out var target)) continue;
            if (!alreadyHit.Add(target)) continue;


            Vector2 dir = ((Vector2)col.transform.position - (Vector2)transform.position).normalized;

            HitData hit = new HitData(attackData.baseDamage, attackData.attackType, isPlayerAttack: false)
            {
                sourcePos = transform.position,
                knockbackDirection = dir,
                knockbackForce = attackData.knockbackForce,
                
                hitstopTime = attackData.hitstopTime,
                hitstunTime = attackData.hitstunTime,

                isBlockable = attackData.isBlockable,
                isParryable = attackData.isParryable
            };

            hit.AddModifier(controller.Stats.Get(StatRef.EnemyOutgoingDamageMult) - 1f, StatModType.PercentAdd, this);

            print($"hit {col.gameObject.name}");
            target.RecieveHit(hit);

        }
    }



#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackData.attackRange);
    }
#endif
}
