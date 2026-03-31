using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private EnemyData data;

    [SerializeField] private float currentHealth;

    protected SpriteRenderer sr;
    protected bool isFacingRight = true;

    private Material originalMaterial;

    [SerializeField] private Material damageMaterial;
    [SerializeField] private Sprite damageSprite;
    [SerializeField] private GameObject particles;

    private Coroutine hitFXRoutine;

    public bool IsAlive { get; set; }

    private Rigidbody2D rb;

    protected virtual void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = data.maxHealth;
        IsAlive = true;
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

        Instantiate(particles, transform.position, Quaternion.identity);
        originalMaterial = sr.material;
        Sprite baseSprite = sr.sprite;

        sr.sprite = damageSprite;
        sr.material = damageMaterial;

        yield return new WaitForSeconds(duration);

        sr.sprite = baseSprite;
        sr.material = originalMaterial;
    }


    protected void FlipFacing()
    {
        isFacingRight = !isFacingRight;

        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

}

