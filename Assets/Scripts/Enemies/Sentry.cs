using System.Collections;
using UnityEngine;

public class Sentry : EnemyBase
{
    private Transform playerTransform;

    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstInterval = 0.15f;

    [SerializeField] private float attackCooldown;

    [SerializeField] private float spreadAngle = 10f;

    private bool isFiring = false;
    private float attackCooldownTimer;

    private ObjectPool<Projectile> pool;
    [SerializeField] private Transform firePoint;
    [SerializeField] private ProjectileData bulletData;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform bulletContainer;

    [SerializeField] private GameObject bulletFlashEffect;

    protected override void Awake()
    {
        base.Awake();

        playerTransform = GameObject.FindWithTag("Player").transform;
        pool = new ObjectPool<Projectile>(projectilePrefab, 3, bulletContainer);

        attackCooldownTimer = Random.Range(0, attackCooldown);
    }

    protected override void Update()
    {
        HandleCooldown();

        base.Update();

        if (attackCooldownTimer <= 0f && isFiring != true)
            DoBurst();
    }

    private void HandleCooldown()
    {
        if(attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;


    }

    private void DoBurst()
    {
        if (isFiring) return;

        StartCoroutine(FireBurstRoutine());
    }

    private IEnumerator FireBurstRoutine()
    {
        // Do windup

        // wait for it to end then fire

        isFiring = true;

        for (int i = 0; i < burstCount; i++)
        {
            var direction = playerTransform.position - transform.position;

            float spread = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
            Vector2 fireDir = Quaternion.Euler(0f, 0f, spread) * direction;

            if (fireDir.x > 0 && !isFacingRight)
                FlipFacing();
            else if (fireDir.x < 0 && isFacingRight) 
                FlipFacing();

            var projectile = pool.Get();
            projectile.transform.position = firePoint.position;
            projectile.Initalize(bulletData, fireDir, p => pool.ReturnToPool(p), false);

            Instantiate(bulletFlashEffect, firePoint.position, Quaternion.identity);

            if(i < burstCount - 1)
                yield return new WaitForSeconds(burstInterval);
        }

        attackCooldownTimer = attackCooldown;
        isFiring = false;
    }
}
