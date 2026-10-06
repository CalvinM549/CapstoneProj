using System.Collections;
using UnityEngine;

public class LungeAttackExecutor : AttackExecutorBase
{
    [SerializeField] private EnemyMeleeHitbox hitbox;
    [SerializeField] private float hitboxRestAngleOffset = 0f;

    [SerializeField] private float lungeSpeed;
    [SerializeField] private float lungeDuration;

    private Vector2 cachedDirection;
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
        HasExecuted = false;

        Vector2 dirToTarget = ai.context.target != null
            ? ((Vector2)ai.context.target.position - (Vector2)transform.position).normalized
            : (Vector2)transform.right;

        cachedDirection = dirToTarget;
        float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        hitbox.transform.localRotation = Quaternion.Euler(0f, 0f, angle - hitboxRestAngleOffset);

        if (!string.IsNullOrEmpty(attackData.windupTrigger))
            animator.SetTrigger(attackData.windupTrigger);
    }

    public override void Execute(EnemyAIController ai)
    {
        if (HasExecuted) return;

        Executing = true;

        if (!string.IsNullOrEmpty(attackData.activeTrigger))
            animator.SetTrigger(attackData.activeTrigger);

        hitbox.Activate();
        StartCoroutine(LungeRoutine(ai));
    }

    private IEnumerator LungeRoutine(EnemyAIController ai)
    {
        rb.linearVelocity = cachedDirection * lungeSpeed;

        yield return new WaitForSeconds(lungeDuration);

        FinishLunge();

        yield return new WaitForSeconds(attackData.winddownDuration);
        Executing = false;
        HasExecuted = true;
    }

    private void FinishLunge()
    {
        rb.linearVelocity = Vector2.zero;
        hitbox.Deactivate();

        Executing = false;
        HasExecuted = true;
    }

    public override void Interrupt(EnemyAIController ai)
    {
        StopAllCoroutines();
        FinishLunge();
        Executing = false;
        HasExecuted = false;
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
