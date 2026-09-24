using System.Collections;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Player p;

    [SerializeField] 
    private SpriteRenderer sr;
    private Animator animator;
    private Rigidbody2D rb;

    private Coroutine hitRoutine = null;


    private bool heavySwingStarted = false;

    public Vector2 spritePos => sr.transform.position;
    public Sprite currentSprite => sr.sprite;
    public bool IsFacingRight { get; private set; }

    [Header("Config")]

    private Material baseMaterial;
    [SerializeField] private Material hitMaterial;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        p = GetComponent<Player>();

        baseMaterial = sr.material;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDashStart += HandleDashStart;
        GameEvents.OnPlayerDashEnd += HandleDashEnd;

        GameEvents.OnAttackStarted += HandleAttackStart;

        GameEvents.OnPlayerTookDamage += HandleHit;

        GameEvents.OnPlayerDeath += HandleDeath;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDashStart -= HandleDashStart;
        GameEvents.OnPlayerDashEnd -= HandleDashEnd;

        GameEvents.OnAttackStarted -= HandleAttackStart;

        GameEvents.OnPlayerTookDamage -= HandleHit;

        GameEvents.OnPlayerDeath -= HandleDeath;
    }

    private void Update()
    {
        UpdateMovementAnimation();
        UpdateAttackAnimation();
        UpdateAutoFlip();
    }

    private void UpdateMovementAnimation()
    {
        if (TimescaleManager.IsPaused) return;
        if (p.Movement.IsDashing) return;

        animator.SetBool("IsMoving", p.Movement.IsMoving);

        // Facing dir
        bool movingForwards = false;

        Vector2 mouseDir = p.GetMouseDirection();

        if (mouseDir.x > 0 && p.Movement.CurrentMoveDirection.x > 0)
            movingForwards = true;
        else if (mouseDir.x < 0 && p.Movement.CurrentMoveDirection.x < 0)
            movingForwards = true;
        else
            movingForwards = false;

        animator.SetBool("MovingForwards", movingForwards);
    }

    private void UpdateAutoFlip()
    {
        if (TimescaleManager.IsPaused) return;
        if (p.Health.IsHitstunned) return;
        if (p.Combat.CurrentState == CombatState.Startup
            || p.Combat.CurrentState == CombatState.Active) return;

        if (p.Movement.IsDashing)
        {
            if (p.Movement.CurrentMoveDirection.x > 0 && IsFacingRight)
                FlipFacing();
            else if(p.Movement.CurrentMoveDirection.x < 0 && !IsFacingRight)
                FlipFacing();
        }
        else
        {
            Vector2 mouseDir = p.GetMouseDirection();

            if (mouseDir.x > 0 && IsFacingRight)
                FlipFacing();
            else if (mouseDir.x < 0 && !IsFacingRight)
                FlipFacing();
        }
    }

    private void HandleAttackStart(AttackType type, Vector2 direction)
    {
        if (direction.x > 0 && IsFacingRight)
            FlipFacing();
        else if (direction.x < 0 && !IsFacingRight)
            FlipFacing();

        int combo = p.Combat.ComboStep;

        AttackDirection animationDir = AttackDirection.Side;
        if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            if (direction.y > 0)
                animationDir = AttackDirection.Up;
            else if(direction.y < 0)
                animationDir = AttackDirection.Down;
        }

        switch (type)
        {
            case AttackType.Light:
                switch (animationDir)
                {
                    case AttackDirection.Side:
                    case AttackDirection.Down:
                        if (combo <= 1)
                            animator.Play("AttackSideLight1");
                        else if (combo == 2)
                            animator.Play("AttackSideLight2");
                        else if (combo == 3)
                            animator.Play("AttackSideLight3");
                        break;

                    case AttackDirection.Up:
                        if (combo <= 1)
                            animator.Play("AttackUpLight1");
                        else if (combo == 2)
                            animator.Play("AttackUpLight2");
                        else if (combo == 3)
                            animator.Play("AttackUpLight3");
                        break;
                }
                break;

            case AttackType.Heavy:

                heavySwingStarted = false;
                animator.ResetTrigger("DoHeavySwing");

                switch (animationDir)
                {
                    case AttackDirection.Side:
                    case AttackDirection.Down:
                        animator.Play("AttackSideHeavyWindup");
                        break;

                    case AttackDirection.Up:
                        animator.Play("AttackUpHeavyWindup");
                        break;
                }

                break;
        }
    }

    private void UpdateAttackAnimation()
    {
        if (p.Combat.CurrentAttackType == AttackType.Heavy)
        {
            if (p.Combat.CurrentState == CombatState.Active && !heavySwingStarted)
            {
                animator.SetTrigger("DoHeavySwing");
                heavySwingStarted = true;
                return;
            }
        }
    }

    private void HandleDashStart()
    {
        //StartCoroutine(DashAnimationRoutine());

        animator.SetBool("IsDashing", true);
        animator.Play("Dashing");
    }
    
    private void HandleDashEnd()
    {
        animator.SetBool("IsDashing", false);
    }

    private void HandleDeath()
    {
        VFXManager.Instance.PlayVFX(VFXType.ExplosionComplex, transform.position);
        StartCoroutine(HitFeedbackRoutine(1f));
        animator.Play("DeathIdle");
    }

    private void HandleHit(HitData hit)
    {
        if(hitRoutine != null)
            StopCoroutine(hitRoutine);

        hitRoutine = StartCoroutine(HitFeedbackRoutine(hit.hitstunTime));
    }

    private IEnumerator HitFeedbackRoutine(float duration)
    {
        sr.material = hitMaterial;

        yield return new WaitForSeconds(duration);

        sr.material = baseMaterial;
    }

    private void FlipFacing()
    {
        IsFacingRight = !IsFacingRight;

        Vector3 scaler = sr.transform.localScale;
        scaler.x *= -1;
        sr.transform.localScale = scaler;
    }
}
