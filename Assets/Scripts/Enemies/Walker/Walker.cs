using System.Collections;
using UnityEngine;

public class Walker : EnemyBase, IProjectileEmitter
{

    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float attackCooldown;

    [Header("Missiles")]
    [SerializeField] private int missileCount;
    [SerializeField] private float missileInterval;
    [SerializeField] private float missileScatter;

    [SerializeField] private Transform[] firePoints;
    [SerializeField] private GameObject telegraphPrefab;

    private Vector2[] targetPositions;
    private Vector2 currentTarget;

    [Header("Slam")]

    private bool isAttacking;
    private float attackCooldownTimer;

    private float slamCooldownTimer;

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
        SetFiring(true);

        animator.Play("MissileFire");

        yield return new WaitForSeconds(0.5f); // wait for animation to finish


        targetPositions = new Vector2[missileCount];

        for (int i = 0; i < missileCount; i++)
        {
            bool validTarget = false;
            while (!validTarget)
            {
                //print("generating point");
                Vector2 targetPoint = Random.insideUnitCircle * missileScatter; // 15f is the scatter distance

                validTarget = Physics2D.OverlapPoint(targetPoint, groundLayer) != null;

                if(validTarget)
                    targetPositions[i] = (Vector2)playerTransform.position + targetPoint;
            }

            //print($"Point founnd : {targetPositions[i]}");
        }


        for (int i = 0; i < missileCount; i++)
        {
            FireMissile(i);
            
            if(i < missileCount - 1)
                yield return new WaitForSeconds(missileInterval);
        }

        yield return new WaitForSeconds(0.4f);

        SetFiring(false);
        attackCooldownTimer = attackCooldown;
    }

    private void DoSlam()
    {
        // Raise self up, wait until slam attacking
        // Do damage and stun all around when hitting ground
    }

    private void SetFiring(bool firing)
    {
        animator.SetBool("IsFiring", firing);
        isAttacking = firing;
    }

    private void FireMissile(int count)
    {
        print($"Missile {count} fired");

        if (count > missileCount) return;

        currentTarget = targetPositions[count];

        FireProjectile(projectiles[0], firePoints[count].position, Vector2.up, false);
    }

    public void FireProjectile(ProjectileData projectile, Vector2 firePos, Vector2 direction, bool isPlayer)
    {
        var missile = ProjectileManager.Instance.FireProjectile(projectile, firePos, direction, isPlayer) as MissileProjectile;
        
        if (missile is MissileProjectile mp)
        {
            mp.InitializeAsMissile(currentTarget, telegraphPrefab);
        }
        
        // Missile smoke puff?
    }
}
