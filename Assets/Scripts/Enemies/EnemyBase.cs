using System;
using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected EnemyData data;

    protected Transform playerTransform;

    protected float currentHealth;
    public float PercentHealth => currentHealth / data.maxHealth;

    public event Action<float, float, HitData> OnHit;

    protected SpriteRenderer sr;
    protected Animator animator;
    protected Rigidbody2D rb;

    protected bool isFacingRight = true;

    private Material baseMaterial;
    protected Sprite baseSprite;

    [SerializeField] private Material damageMaterial;
    [SerializeField] private GameObject damageParticles;
    [SerializeField] private GameObject damageSparkParticles;

    [SerializeField] private GameObject deathParticles;

    private Coroutine hitFXRoutine;

    public bool IsAlive { get; set; }
    public bool IsIFrame = false;


    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        sr = animator.GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();
        currentHealth = data.maxHealth;
        IsAlive = true;

        baseSprite = sr.sprite;
        baseMaterial = sr.material;
    }

    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
    protected virtual void Update() { }

    public void RecieveHit(HitData hit)
    {
        if (!IsAlive) return;
        if (IsIFrame) return;
        if (!hit.isPlayerAttack) return;

        ApplyDamage(hit);
        ApplyKnockback(hit.knockbackDirection, hit.knockbackForce);

        OnHit?.Invoke(currentHealth, data.maxHealth, hit);
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

        rb.linearVelocity = (direction * force * data.knockbackMultiplier);
    }

    protected virtual void Die()
    {
        Debug.Log("Enemy Killed");
        IsAlive = false;

        if (deathParticles != null)
            Instantiate(deathParticles, transform.position, Quaternion.identity);

            StopAllCoroutines();
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

        if(damageParticles != null)
            Instantiate(damageParticles, transform.position, Quaternion.identity);

        if (damageSparkParticles != null)
            Instantiate(damageSparkParticles, transform.position, Quaternion.identity);

        animator.Play("Hit");

        sr.material = damageMaterial;

        yield return new WaitForSeconds(duration);

        sr.material = baseMaterial;

        IsIFrame = false;
    }


    protected void FlipFacing()
    {
        isFacingRight = !isFacingRight;

        Vector3 scaler = sr.transform.localScale;
        scaler.x *= -1;
        sr.transform.localScale = scaler;
    }

}

