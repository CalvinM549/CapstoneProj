using System;
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected EnemyData data;
    public EnemyData Data => data;

    protected Transform playerTransform;
    public bool hasPlayerLock;

    protected float currentHealth;
    public float PercentHealth => currentHealth / data.baseHealth;

    public event Action<float, float, HitData> OnHit;

    protected SpriteRenderer sr;
    protected Animator animator;
    protected Rigidbody2D rb;

    protected bool isFacingRight = true;

    private Material baseMaterial;
    protected Sprite baseSprite;

    [SerializeField] private Material damageMaterial;

    private Coroutine hitFXRoutine;

    public bool IsAlive { get; set; }
    public bool IsIFrame = false;

    public void OnSpawn()
    {

    }

    public void OnDespawn()
    {

        StopAllCoroutines();
    }

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        sr = animator.GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();
        currentHealth = data.baseHealth;
        IsAlive = true;

        baseSprite = sr.sprite;
        baseMaterial = sr.material;
    }

    protected virtual void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
    protected virtual void Update() { }

    public virtual void Initialize(Transform player) // Occurs when spawned??
    {
        playerTransform = player;
    }

    public virtual void ResetForPool()
    {
        // reset health
    }

    #region Taking Damage

    public void RecieveHit(HitData hit)
    {
        if (!IsAlive) return;
        if (IsIFrame) return;
        if (!hit.isPlayerAttack) return;

        ApplyDamage(hit);
        ApplyKnockback(hit.knockbackDirection, hit.knockbackForce);

        OnHit?.Invoke(currentHealth, data.baseHealth, hit);
        GameEvents.EnemyHit(this, hit);

        if (IsAlive)
        {
            if(hitFXRoutine != null)
                StopCoroutine(hitFXRoutine);
            hitFXRoutine = StartCoroutine(HitFXRoutine(hit.hitstunTime));
        }
        if (currentHealth <= 0)
            Die();
    }

    protected virtual void ApplyDamage(HitData hit)
    {
        currentHealth = Mathf.Max(0, currentHealth - hit.damage);
    }

    protected virtual void ApplyKnockback(Vector2 direction, float force)
    {
        if (direction.magnitude < 0.1f) return;

        rb.linearVelocity = (direction * force);
    }

    protected virtual void Die()
    {
        IsAlive = false;

        VFXManager.Instance.PlayVFX(VFXType.ExplosionComplex, transform.position);

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

}

