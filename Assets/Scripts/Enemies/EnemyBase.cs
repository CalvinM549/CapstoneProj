using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;

    [SerializeField] private float knockbackMultiplier;

    private SpriteRenderer sr;
    [SerializeField] private GameObject particles;

    public bool IsAlive { get; set; }

    private Rigidbody2D rb;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        IsAlive = true;
    }

    public void RecieveHit(HitData hit)
    {
        if (!IsAlive) return;

        ApplyDamage(hit);
        ApplyKnockback(hit.knockbackDirection, hit.knockbackForce);
        
        if (IsAlive) StartCoroutine(HitFXRoutine());
        
    }

    private void ApplyDamage(HitData hit)
    {

        currentHealth = Mathf.Max(0, currentHealth - hit.damage);

        Debug.Log($"[EnemyBase] Hit by {hit.attackType} " +
                  $"— damage: {hit.damage} " +
                  $"— HP: {currentHealth}/{maxHealth}");


        if (currentHealth <= 0)
            Die();
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
        Instantiate(particles, transform.position, Quaternion.identity);
        Color oldcolour = sr.color;
        sr.color = Color.white;

        yield return new WaitForSeconds(0.15f);

        sr.color = oldcolour;
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

