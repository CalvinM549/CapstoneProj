using System;
using System.Collections;
using UnityEngine;

public enum EnemyState
{
    Spawning,
    Active,
    Staggered,
    Dead
}

public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public EnemyData data;

    [SerializeField] private Material damageMaterial;

    // References
    protected EnemyAIController ai;
    protected EnemyStats stats;
    protected StatusEffectController statusController;

    public EnemyStats Stats => stats;

    protected SpriteRenderer sr;
    protected Animator animator;
    protected Rigidbody2D rb;
    
    public bool hasPlayerLock;

    protected float currentHealth;
    public float PercentHealth => currentHealth / data.baseHealth;

    public event Action<float, float, HitData> OnHit;

    protected bool isFacingRight = true;

    private Material baseMaterial;


    private Coroutine hitFXRoutine;

    public bool IsAlive { get; set; }
    public bool IsIFrame = false;

    private bool hyperArmourActive;
    protected virtual bool CanBeStaggered => !hyperArmourActive;

    protected virtual void Awake()
    {
        ai = GetComponent<EnemyAIController>();
        stats = GetComponent<EnemyStats>();
        statusController = GetComponent<StatusEffectController>();

        animator = GetComponentInChildren<Animator>();
        sr = animator.GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();

        baseMaterial = sr.material;
    }

    private void Start()
    {
        AIManager.Instance.RegisterEnemy(ai);
        OnSpawn();
    }

    private void Update()
    {
        if (!IsIFrame)
        {
            animator.SetBool("IsWalking", rb.linearVelocity.magnitude > 0.1);

            if (rb.linearVelocityX > 0 && !isFacingRight)
                FlipFacing();
            else if (rb.linearVelocityX < 0 && isFacingRight)
                FlipFacing();
        }
    }

    private void OnDisable()
    {
        AIManager.Instance.UnregisterEnemy(ai);
    }

    #region Spawning / Despawning

    public virtual void OnSpawn()
    {
        stats.Initialize(data, data.aiProfile);

        currentHealth = stats.Get(StatRef.EnemyMaxHealth);
        IsAlive = true;
        hyperArmourActive = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = 0f;

        animator.Rebind();
        animator.Update(0f);

        ai.ResetController(data.aiProfile);
        ai.enabled = true;
    }

    public virtual void OnDespawn()
    {
        ai.enabled = false;
        StopAllCoroutines();
    }

    private void HandleDeath()
    {

    }

    public virtual void ResetForPool()
    {
        // reset health
    }

    #endregion

    public void SetHyperArmour(bool active) => hyperArmourActive = active;

    #region Taking Damage


    public void RecieveHit(HitData hit)
    {
        if (!IsAlive) return;
        //if (IsIFrame) return;
        if (!hit.isPlayerAttack) return;

        ApplyDamage(hit);
        ApplyStatuses(hit);

        if (hit.attackType != AttackType.DamageOverTime)
        {
            if (CanBeStaggered)
            {
                ai.ChangeState(EnemyStates.Staggered);
                ai.context.staggerTimer = hit.hitstunTime;
            }

            ApplyKnockback(hit.knockbackDirection, hit.knockbackForce);

            if (IsAlive)
            {
                if (hitFXRoutine != null)
                    StopCoroutine(hitFXRoutine);
                hitFXRoutine = StartCoroutine(HitFXRoutine(hit.hitstunTime));
            }
        }

        OnHit?.Invoke(currentHealth, data.baseHealth, hit);
        GameEvents.EnemyHit(this, hit);

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void ApplyDamage(HitData hit)
    {
        float defenceMult = stats.Get(StatRef.EnemyIncomingDamageMult);
        hit.AddModifier(defenceMult - 1, StatModType.PercentAdd, this);

        currentHealth = Mathf.Max(0, currentHealth - hit.FinalDamage);
    }

    protected virtual void ApplyStatuses(HitData hit)
    {
        foreach (var pending in hit.PendingStatusEffects)
        {
            statusController?.ApplyEffects(pending.data, pending.source, pending.appliedByPlayer, pending.stacks);
        }
    }

    protected virtual void ApplyKnockback(Vector2 direction, float force)
    {
        if (direction.magnitude < 0.1f) return;

        rb.linearVelocity = (direction * force);
    }

    protected virtual void Die()
    {
        OnDespawn();
        IsAlive = false;

        VFXManager.Instance.PlayVFX(VFXType.ExplosionComplex, transform.position);
        RunManager.Instance.currencyDropService.DropBurst(transform.position, data.baseCurrencyDrop, 4);

        StopAllCoroutines();

        GameEvents.EnemyKilled(this);
        gameObject.SetActive(false);
    }

    private IEnumerator HitFXRoutine(float duration)
    {
        IsIFrame = true;

        if (rb.linearVelocity.x > 0 && isFacingRight)
        {
            FlipFacing();
        }
        else if (rb.linearVelocity.x < 0 && !isFacingRight)
        {
            FlipFacing();
        }

        VFXManager.Instance.PlayVFX(VFXType.HitSpark, transform.position);

        animator.Play("Hit");

        sr.material = damageMaterial;

        yield return new WaitForSeconds(duration);

        sr.material = baseMaterial;

        IsIFrame = false;
    }

    #endregion

    protected void FlipFacing()
    {
        isFacingRight = !isFacingRight;

        Vector3 scaler = sr.transform.localScale;
        scaler.x *= -1;
        sr.transform.localScale = scaler;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, data.aiProfile.aggroRange);
    }
#endif
}

