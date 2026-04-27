using System;
using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected EnemyData data;

    protected float currentHealth;
    public float PercentHealth => currentHealth / data.maxHealth;

    public event Action<float, float, HitData> OnHit;

    [SerializeField] protected SpriteRenderer sr;
    [SerializeField] protected Animator animator;
    protected bool isFacingRight = true;

    private Material baseMaterial;
    protected Sprite baseSprite;

    [SerializeField] private Material damageMaterial;
    [SerializeField] private Sprite damageSprite;
    [SerializeField] private GameObject damageParticles;

    private Coroutine hitFXRoutine;

    public bool IsAlive { get; set; }

    private Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = data.maxHealth;
        IsAlive = true;

        baseSprite = sr.sprite;
        baseMaterial = sr.material;
    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {

    }

    protected virtual void Update()
    {
        
    }

    public void RecieveHit(HitData hit)
    {
        if (!IsAlive) return;
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

    protected void ApplyDamage(HitData hit)
    {
        currentHealth = Mathf.Max(0, currentHealth - hit.damage);
    }

    private void ApplyKnockback(Vector2 direction, float force)
    {
        if (direction.magnitude < 0.1f) return;

        rb.linearVelocity = (direction * force * data.knockbackMultiplier);
    }

    private void Die()
    {
        Debug.Log("Enemy Killed");
        gameObject.SetActive(false);
        currentHealth = data.maxHealth;
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

        if(damageParticles != null)
            Instantiate(damageParticles, transform.position, Quaternion.identity);

        animator.Play("Hit");

        sr.material = damageMaterial;

        yield return new WaitForSeconds(duration);

        sr.material = baseMaterial;
    }


    protected void FlipFacing()
    {
        isFacingRight = !isFacingRight;

        Vector3 scaler = sr.transform.localScale;
        scaler.x *= -1;
        sr.transform.localScale = scaler;
    }

}

