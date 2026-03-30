using System.Collections;
using UnityEngine;

public class Sentry : EnemyBase
{
    private Transform playerTransform;

    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstInterval = 0.15f;

    [SerializeField] private float attackCooldown;

    [SerializeField] private float spreadAngle;

    private bool isFiring = false;
    private float attackCooldownTimer;

    private ObjectPool<Projectile> pool;
    [SerializeField] private Transform firePoint;
    [SerializeField] private ProjectileData bulletData;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform bulletContainer;

    [SerializeField] private GameObject bulletFlashEffect;

    private void Awake()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        pool = new ObjectPool<Projectile>(projectilePrefab, 3, bulletContainer);
    }

    protected override void Update()
    {
        HandleCooldown();

        base.Update();

        if (attackCooldownTimer <= 0f && isFiring != true)
            FireBurst();
    }

    private void HandleCooldown()
    {
        if(attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;


    }

    private void FireBurst()
    {
        if (isFiring) return;

        StartCoroutine(FireBurstRoutine());
    }

    private IEnumerator FireBurstRoutine()
    {
        isFiring = true;

        for (int i = 0; i < burstCount; i++)
        {
            var direction = playerTransform.position - transform.position;
            var projectile = pool.Get();
            projectile.transform.position = firePoint.position;
            projectile.Initalize(bulletData, direction, p => pool.ReturnToPool(p), false);

            Instantiate(bulletFlashEffect, firePoint.position, Quaternion.identity);

            if(i < burstCount - 1)
                yield return new WaitForSeconds(burstInterval);
        }

        attackCooldownTimer = attackCooldown;
        isFiring = false;
    }
}
