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
    private Sprite baseSprite;

    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite moveSprite;
    [SerializeField] private Sprite decelSprite;
    [SerializeField] private Sprite dashSprite;
    [SerializeField] private Sprite parrySprite;

    private Material baseMaterial;
    [SerializeField] private Material hitMaterial;



    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        p = GetComponent<Player>();

        baseSprite = sr.sprite;
        baseMaterial = sr.material;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDashStart += HandleDashStart;
        GameEvents.OnPlayerDashEnd += HandleDashEnd;

        GameEvents.OnAttackStarted += HandleAttackStart;

        GameEvents.OnPlayerHit += HandleHit;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDashStart -= HandleDashStart;

        GameEvents.OnAttackStarted -= HandleAttackStart;

        GameEvents.OnPlayerHit -= HandleHit;
    }

    private void Update()
    {
        //HandleMovementSprite();
        UpdateMovementAnimation();
        UpdateAttackAnimation();
    }

    private void FixedUpdate()
    {
        HandleAutoFlip();
    }

    private void UpdateMovementAnimation()
    {
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

    private void HandleAutoFlip()
    {
        if (p.Health.IsHitstunned) return;

        //// MoveDir based Flip
        //if (p.Movement.IsMoving)
        //{
        //    if (rb.linearVelocity.x > 0 && IsFacingRight)
        //    {
        //        FlipFacing();
        //    }
        //    else if(rb.linearVelocity.x < 0 && !IsFacingRight)
        //    {
        //        FlipFacing();
        //    }
        //}

        // Mouse based flip
        
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

    private void HandleAttackStart(AttackType type)
    {
        if (type == AttackType.DashAttack) return;

        int combo = p.Combat.ComboStep;

        if (type == AttackType.Light)
        {
            if (combo <= 1)
                animator.Play("AttackSideLight1");
            else if(combo == 2)
                animator.Play("AttackSideLight2");
            else if(combo == 3)
                animator.Play("AttackSideLight3");
        }

        if (type == AttackType.Heavy)
        {
            heavySwingStarted = false;
            animator.ResetTrigger("DoHeavySwing");

            animator.Play("AttackSideHeavyWindup");
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


    protected void FlipFacing()
    {
        IsFacingRight = !IsFacingRight;

        Vector3 scaler = sr.transform.localScale;
        scaler.x *= -1;
        sr.transform.localScale = scaler;
    }
}
