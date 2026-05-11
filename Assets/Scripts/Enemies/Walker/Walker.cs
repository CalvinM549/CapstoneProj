using System.Collections;
using UnityEngine;

public class Walker : EnemyBase, IProjectileEmitter
{
    private Transform playerPosition;

    [SerializeField] private float attackCooldown;

    [Header("Missiles")]
    [SerializeField] private int missileCount;
    [SerializeField] private float missileInterval;
    [SerializeField] private float explosionRadius;

    [SerializeField] private Transform[] firePoints;
    [SerializeField] private GameObject telegraphPrefab;

    private Vector2[] targetPositions;
    private Vector2 currentPosition;

    [Header("Slam")]



    private bool isAttacking;
    private float attackCooldownTimer;

    private float slamCooldownTimer;

    [SerializeField] private ProjectileData[] projectiles;
    public ProjectileData[] Projectiles => projectiles;

    public bool IsPlayerProjectile => false;
    public bool PoolRequested { get; set; }

    private void Awake()
    {
        base.Awake();
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
        UpdateCooldown();
        UpdateMovement();
        
        base.Update();

        if (attackCooldownTimer <= 0f && isAttacking != true)
            MissileBarrage();
    }

    private void UpdateCooldown()
    {
        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;
    }

    private void UpdateMovement()
    {
        //?? idk how it should move
    }

    private void MissileBarrage()
    {
        if (isAttacking) return;

        // Fire 4 missiles into the air
        // Mark regions on the map near the player equal to number of missiles

        // After delay, each section explodes (and visual missile falling from sky)

        StartCoroutine(MissileBarrageRoutine());
    }

    private IEnumerator MissileBarrageRoutine()
    {
        isAttacking = true;

        animator.Play("MissileFire");

        yield return new WaitForSeconds(0.5f); // wait for animation to finish


        Vector2[] targetPositions = new Vector2[missileCount];

        for (int i = 0; i < missileCount; i++)
        {
            Vector2 scatter = Random.insideUnitCircle * 2f; // 2f is the scatter distance
            targetPositions[i] = (Vector2)playerPosition.position + scatter;
        }


        for (int i = 0; i < missileCount; i++)
        {
            FireMissile(i);
            
            if(i < missileCount - 1)
                yield return new WaitForSeconds(missileInterval);
        }

        isAttacking = false;

        yield return new WaitForSeconds(0.4f);

        attackCooldownTimer = attackCooldown;
    }

    private void DoSlam()
    {
        // Raise self up, wait until slam attacking
        // Do damage and stun all around when hitting ground
    }

    private void FireMissile(int count)
    {
        if (count > missileCount) return;

        currentPosition = targetPositions[count];

        FireProjectile(projectiles[0], firePoints[count].position, Vector2.up, false);
    }

    public void FireProjectile(ProjectileData projectile, Vector2 firePos, Vector2 direction, bool isPlayer)
    {
        var missile = ProjectileManager.Instance.FireProjectile(projectile, firePos, direction, isPlayer) as MissileProjectile;
        
        if (missile is MissileProjectile mp)
        {
            mp.InitializeAsMissile(currentPosition, telegraphPrefab);
        }
        
        // Missile smoke puff?
    }
}
