using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private EnemyData data;

    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;

    [SerializeField] private float knockbackMultiplier;

    private SpriteRenderer sr;
    private Material originalMaterial;

    [SerializeField] private Material damageMaterial;
    [SerializeField] private Sprite damageSprite;
    [SerializeField] private GameObject particles;

    private Coroutine hitFXRoutine;

    public bool IsAlive { get; set; }

    private Rigidbody2D rb;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
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
            hitFXRoutine = StartCoroutine(HitFXRoutine());
        }
        if (currentHealth <= 0)
            Die();
    }

    protected void ApplyDamage(HitData hit)
    {

        currentHealth = Mathf.Max(0, currentHealth - hit.damage);

        Debug.Log($"[EnemyBase] Hit by {hit.attackType} " +
                  $"— damage: {hit.damage} " +
                  $"— HP: {currentHealth}/{maxHealth}");

    }

    private void ApplyKnockback(Vector2 direction, float force)
    {
        if (direction.magnitude < 0.1f) return;

        rb.linearVelocity = (direction * force * knockbackMultiplier);
    }

    private void Die()
    {
        Debug.Log("Enemy Killed");
        gameObject.SetActive(false);
        currentHealth = maxHealth;
    }

    private IEnumerator HitFXRoutine()
    {
        if (rb.linearVelocity.x > 0)
        {
            sr.flipX = false;
        }
        else if (rb.linearVelocity.x < 0)
        {
            sr.flipX = true;
        }

        Instantiate(particles, transform.position, Quaternion.identity);
        originalMaterial = sr.material;
        Sprite baseSprite = sr.sprite;

        sr.sprite = damageSprite;
        sr.material = damageMaterial;

        yield return new WaitForSeconds(0.15f);

        sr.sprite = baseSprite;
        sr.material = originalMaterial;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1.2f,
            $"HP {currentHealth}/{maxHealth}"
        );
    }
#endif

}

