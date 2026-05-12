using System.Collections;
using UnityEngine;

public class Sentry : EnemyBase, IProjectileEmitter
{

    [SerializeField] private Transform[] patrolPoints;
    private int currentPoint = 0;

    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstInterval = 0.15f;

    [SerializeField] private float attackCooldown;

    [SerializeField] private float spreadAngle = 10f;

    [SerializeField] private Sprite fireSprite;

    private bool isFiring = false;
    private float attackCooldownTimer;

    [SerializeField] private Transform firePoint;
    [SerializeField] private ProjectileData bulletData;

    [SerializeField] private GameObject bulletFlashEffect;
    [SerializeField] private GameObject windupEffect;

    [SerializeField] private ProjectileData[] projectiles;
    public ProjectileData[] Projectiles => projectiles;

    public bool IsPlayerProjectile => false;

    public bool PoolRequested { get; set; }

    protected override void Awake()
    {
        base.Awake();

        playerTransform = GameObject.FindWithTag("Player").transform;

        attackCooldownTimer = Random.Range(0, attackCooldown);
    }

    protected override void OnEnable()
    {
        ProjectilePools.RequestPool(this);
    }

    protected override void OnDisable()
    {
        ProjectilePools.ReleasePool(this);
    }

    protected override void Update()
    {
        HandleCooldown();
        UpdateMovement();

        base.Update();

        if (attackCooldownTimer <= 0f && isFiring != true)
            DoBurst();

        if (isFiring)
        {
            Vector2 direction = playerTransform.position - transform.position;

            if (direction.x > 0 && !isFacingRight)
                FlipFacing();
            else if (direction.x < 0 && isFacingRight)
                FlipFacing();
        }
    }

    private void HandleCooldown()
    {
        if(attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;
    }

    private void UpdateMovement()
    {
        if (isFiring)
        {
            animator.SetBool("IsWalking", false);
            return;
        }

        animator.SetBool("IsWalking", true);

        Vector3 targetPoint = patrolPoints[currentPoint].transform.position;

        Vector2 direction = transform.position - targetPoint;
        if (direction.x > 0 && isFacingRight)
            FlipFacing();
        else if(direction.x < 0 && !isFacingRight)
            FlipFacing();

            transform.position = Vector2.MoveTowards(transform.position, targetPoint, (data.moveSpeed * Time.deltaTime));
        if (transform.position == targetPoint)
        {
            if (currentPoint >= patrolPoints.Length - 1)
                currentPoint = 0;
            else
                currentPoint++;
        }
    }

    private void DoBurst()
    {
        if (isFiring) return;

        StartCoroutine(FireBurstRoutine());
    }

    private IEnumerator FireBurstRoutine()
    {
        isFiring = true;

        animator.Play("FireReady");
        // Windup

        sr.sprite = fireSprite;

        Instantiate(windupEffect, firePoint);

        yield return new WaitForSeconds(1.5f);

        // Firing

        for (int i = 0; i < burstCount; i++)
        {
            var direction = playerTransform.position - transform.position;

            float spread = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
            Vector2 fireDir = Quaternion.Euler(0f, 0f, spread) * direction;

            if (fireDir.x > 0 && !isFacingRight)
                FlipFacing();
            else if (fireDir.x < 0 && isFacingRight) 
                FlipFacing();

            FireProjectile(Projectiles[0], firePoint.position, direction, false);

            if (i < burstCount - 1)
                yield return new WaitForSeconds(burstInterval);
        }

        attackCooldownTimer = attackCooldown;

        yield return new WaitForSeconds(0.2f);

        animator.Play("ReturnFromFire");

        yield return new WaitForSeconds(1.5f);


        isFiring = false;

        sr.sprite = baseSprite;
    }

    public void FireProjectile(ProjectileData projectile, Vector2 firePos, Vector2 direction, bool isPlayer)
    {
        ProjectileManager.Instance.FireProjectile(projectile, firePos, direction, isPlayer);
        Instantiate(bulletFlashEffect, firePoint);
    }
}
