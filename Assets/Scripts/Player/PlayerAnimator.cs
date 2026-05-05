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
        rb = GetComponent<Rigidbody2D>();
        p = GetComponent<Player>();

        baseSprite = sr.sprite;
        baseMaterial = sr.material;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDashStart += HandleDashAnimation;
        GameEvents.OnPlayerParryStart += HandleParryAnimation;
        GameEvents.OnPlayerParryEnd += HandleParryEnd;

        GameEvents.OnPlayerHit += HandleHit;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDashStart -= HandleDashAnimation;
        GameEvents.OnPlayerParryStart -= HandleParryAnimation;
        GameEvents.OnPlayerParryEnd -= HandleParryEnd;

        GameEvents.OnPlayerHit -= HandleHit;
    }

    private void Update()
    {
        HandleAutoFlip();
        HandleMovementSprite();
    }

    private void HandleAutoFlip()
    {
        if (p.Health.IsHitstunned) return;

        if (p.Movement.IsMoving)
        {
            if (rb.linearVelocity.x > 0 && IsFacingRight)
            {
                FlipFacing();
            }
            else if(rb.linearVelocity.x < 0 && !IsFacingRight)
            {
                FlipFacing();
            }
        }
    }

    private void HandleMovementSprite()
    {
        if (p.Movement.IsMoving)
        {
            if (p.Movement.MoveInputting)
                sr.sprite = moveSprite;

            else
                sr.sprite = decelSprite;
        }
        else
        {
            sr.sprite = idleSprite;
        }
    }

    private void HandleDashAnimation()
    {
        StartCoroutine(DashAnimationRoutine());
    }

    private IEnumerator DashAnimationRoutine()
    {
        sr.sprite = dashSprite;

        while(p.Movement.IsDashing)
            yield return null;

        sr.sprite = baseSprite;
    }

    private void HandleParryAnimation()
    {
        sr.sprite = parrySprite;
    }

    private void HandleParryEnd()
    {
        sr.sprite = baseSprite;
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

    public void PlayAttackAnimation(AttackType attackType, Vector2 direction)
    {
        // Convert direction to cardinals

        // Play animation based on attack type
    }

    protected void FlipFacing()
    {
        IsFacingRight = !IsFacingRight;

        Vector3 scaler = sr.transform.localScale;
        scaler.x *= -1;
        sr.transform.localScale = scaler;
    }
}
