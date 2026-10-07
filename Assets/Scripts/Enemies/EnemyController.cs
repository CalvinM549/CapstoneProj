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

    // References
    protected EnemyAIController ai;
    protected EnemyStats stats;
    protected StatusEffectController statusController;
    protected EnemyUI ui;

    public EnemyStats Stats => stats;

    protected SpriteRenderer sr;
    protected Animator animator;
    protected Rigidbody2D rb;
    
    public bool hasPlayerLock { get; private set;  }
    public event Action<bool> onLockChange;

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

    public event Action<EnemyController> OnDeath;

    protected virtual void Awake()
    {
        ai = GetComponent<EnemyAIController>();
        stats = GetComponent<EnemyStats>();
        statusController = GetComponent<StatusEffectController>();

        animator = GetComponentInChildren<Animator>();
        ui = GetComponentInChildren<EnemyUI>();

        sr = animator.GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();

        baseMaterial = sr.material;
    }

    //private void Start()
    //{
    //    OnSpawn();
    //}

    private void OnEnable()
    {
        GameEvents.OnLockAcquired += HandleLockAcquired;
        GameEvents.OnLockDropped += HandleLockDropped;
    }

    private void OnDisable()
    {
        GameEvents.OnLockAcquired -= HandleLockAcquired;
        GameEvents.OnLockDropped -= HandleLockDropped;
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


    #region Spawning / Despawning

    public virtual void OnSpawn()
    {
        AIManager.Instance.RegisterEnemy(ai);
        stats.Initialize(data, data.aiProfile);

        ui.Initialize();

        currentHealth = stats.Get(StatRef.EnemyMaxHealth);
        IsAlive = true;
        hyperArmourActive = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = 0f;

        animator.Rebind();
        animator.Update(0f);

        sr.material = baseMaterial;

        ai.ResetController(data.aiProfile);
        ai.enabled = true;
    }

    public virtual void OnDespawn()
    {
        AIManager.Instance.UnregisterEnemy(ai);
        ai.enabled = false;
        StopAllCoroutines();
    }

    public virtual void ResetForPool()
    {
        // reset health
    }

    #endregion

    private void HandleLockAcquired(EnemyController enemy)
    {
        if (enemy != this) return;

        hasPlayerLock = true;
        onLockChange?.Invoke(true);
    }

    private void HandleLockDropped()
    {
        if (!hasPlayerLock) return;

        hasPlayerLock = false;
        onLockChange?.Invoke(false);
    }

    public void SetHyperArmour(bool active)
    {
        hyperArmourActive = active;
    }

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

                if (hitFXRoutine != null)
                    StopCoroutine(hitFXRoutine);
                hitFXRoutine = StartCoroutine(HitFXRoutine(hit.hitstunTime));

                ApplyKnockback(hit.knockbackDirection, hit.knockbackForce);
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
        //RunManager.Instance.currencyDropService.DropBurst(transform.position, data.baseCurrencyDrop, 4);

        StopAllCoroutines();

        OnDeath?.Invoke(this);
        GameEvents.EnemyKilled(this);
        //gameObject.SetActive(false);
    }

    private IEnumerator HitFXRoutine(float duration)
    {
        if (rb.linearVelocity.x > 0 && isFacingRight)
        {
            FlipFacing();
        }
        else if (rb.linearVelocity.x < 0 && !isFacingRight)
        {
            FlipFacing();
        }

        VFXManager.Instance.PlayVFX(VFXType.HitSpark, transform.position);

        animator.SetTrigger("RecieveHit");

        sr.material = GameManager.Materials.HitMaterial;

        yield return new WaitForSeconds(duration);

        sr.material = baseMaterial;
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

