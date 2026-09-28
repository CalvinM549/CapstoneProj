using System;
using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public ProjectileData data { get; private set; }
    private Vector2 sourcePosition;

    private Vector2 direction;
    private bool playerProjectile;
    protected Rigidbody2D rb;

    private bool hitTarget;

    private HitData hitData;

    private Action<Projectile> returnToPool;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    public virtual void Initialize(
        ProjectileData data, 
        HitData hitData,
        Vector2 direction, 
        Vector2 sourcePos,
        Action<Projectile> returnToPool,
        bool playerProjectile)
    {
        this.data = data;
        this.sourcePosition = sourcePos;
        this.direction = direction;
        this.returnToPool = returnToPool;
        this.playerProjectile = playerProjectile;

        if (hitData == null) print("NO HITDATA");
        this.hitData = hitData;

        hitTarget = false;

        rb.linearVelocity = direction.normalized * data.speed;

        float angle = Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90);

        Invoke(nameof(OnExpire), data.lifetime);
    }

    protected void ReturnToPool()
    {
        CancelInvoke();
        rb.linearVelocity = Vector2.zero;
        hitData = null;

        returnToPool?.Invoke(this);
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (hitTarget) return;

        if (collision.CompareTag("Wall"))
            OnExpire();

        if (playerProjectile && (collision.CompareTag("Player") || collision.CompareTag("PlayerHurtbox")))
            return;

        if (!playerProjectile && (collision.CompareTag("Enemy") || collision.CompareTag("Hurtbox")))
            return;

        IDamageable target = collision.GetComponent<IDamageable>();
        if (target == null || !target.IsAlive) 
            return;

        hitTarget = true;
        HandleHit(target);
    }

    protected virtual void HandleHit(IDamageable target)
    {
        // Fire Event
        
        target.RecieveHit(hitData);

        OnExpire();
    }

    protected virtual void OnExpire()
    {
        rb.linearVelocity = Vector2.zero;

        if (data.impactVFXPrefab != null)
            Instantiate(data.impactVFXPrefab, transform.position, Quaternion.identity);

        ReturnToPool();
    }


}


