using System;
using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public ProjectileData data;
    private Vector2 sourcePosition;

    private Vector2 direction;
    private bool isPlayerProjectile;
    protected Rigidbody2D rb;

    private bool hitTarget;

    private Action<Projectile> returnToPool;
    private Coroutine lifetimeRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    public virtual void Initalize(ProjectileData data, Vector2 direction, Vector2 sourcePos, Action<Projectile> returnToPool, bool playerProjectile)
    {
        this.data = data;
        this.sourcePosition = sourcePos;
        this.direction = direction;
        this.returnToPool = returnToPool;
        isPlayerProjectile = playerProjectile;

        hitTarget = false;

        rb.linearVelocity = direction.normalized * data.speed;

        float angle = Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90);

        if(lifetimeRoutine != null)
            StopCoroutine(lifetimeRoutine);

        lifetimeRoutine = StartCoroutine(LifetimeRoutine());
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(data.lifetime);
        OnExpire();
    }

    protected void ReturnToPool()
    {
        if (lifetimeRoutine != null)
        {
            StopCoroutine(lifetimeRoutine);
            lifetimeRoutine = null;
        }

        rb.linearVelocity = Vector2.zero;

        returnToPool?.Invoke(this);
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (hitTarget) return;

        if (collision.CompareTag("Wall"))
            OnExpire();

        if (isPlayerProjectile && (collision.CompareTag("Player") || collision.CompareTag("PlayerHurtbox")))
            return;

        if (!isPlayerProjectile && (collision.CompareTag("Enemy") || collision.CompareTag("Hurtbox")))
            return;

        IDamageable target = collision.GetComponent<IDamageable>();
        if (target == null || !target.IsAlive) 
            return;

        hitTarget = true;
        HandleHit(target);
    }

    protected virtual void HandleHit(IDamageable target)
    {
        HitData hit = new HitData(data.damage, AttackType.Projectile, isPlayerProjectile)
        {
            sourcePos = sourcePosition,
            knockbackDirection = direction,
            knockbackForce = data.knockback,
            hitstopTime = data.hitstopDuration,
            hitstunTime = data.hitstunTime,

            isParryable = data.isParryable
        };

        // Fire Event
        
        target.RecieveHit(hit);

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


