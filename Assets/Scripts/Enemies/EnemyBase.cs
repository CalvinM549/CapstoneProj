using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;

    [SerializeField] private float knockbackMultiplier;

    public bool IsAlive { get; set; }

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        IsAlive = true;
    }

    public void RecieveHit(HitData hit)
    {
        if (!IsAlive) return;

        ApplyDamage(hit);
        ApplyKnockback(hit.knockbackDirection, hit.knockbackForce);
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
        if (direction.sqrMagnitude < 0.0001f) return;

        rb.linearVelocity = (direction * force * knockbackMultiplier);
    }

    private void Die()
    {
        Debug.Log("Enemy Killed");
        gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Keep the inspector health value in range when editing
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a health fraction label above the object in scene view
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1.2f,
            $"HP {currentHealth}/{maxHealth}"
        );
    }
#endif

}

