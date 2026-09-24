using System.Collections;
using UnityEngine;

public class LungeAttackExecutor : AttackExecutorBase
{
    [SerializeField] private EnemyMeleeHitbox hitbox;

    [SerializeField] private float lungeSpeed;
    [SerializeField] private float lungeDuration;

    private Rigidbody2D rb;

    private void Awake()
    {
        controller = GetComponent<EnemyController>();
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

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

        if (!string.IsNullOrEmpty(attackData.activeTrigger))
            animator.SetTrigger(attackData.activeTrigger);

        hitbox.Activate();
        StartCoroutine(LungeRoutine(ai));
    }

    private IEnumerator LungeRoutine(EnemyAIController ai)
    {
        Vector2 dir = ai.context.target != null
            ? ((Vector2)ai.context.target.position - rb.position).normalized
            : (Vector2)transform.right;

        rb.linearVelocity = dir * lungeSpeed;

        yield return new WaitForSeconds(lungeDuration);

        FinishLunge();

        yield return new WaitForSeconds(attackData.winddownDuration);
        Executing = false;
    }

    private void FinishLunge()
    {
        rb.linearVelocity = Vector2.zero;
        hitbox.Deactivate();
    }

    public override void Interrupt(EnemyAIController ai)
    {
        StopAllCoroutines();
        FinishLunge();
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

        StopAllCoroutines();
        FinishLunge();
        Executing = false;
    }
}
