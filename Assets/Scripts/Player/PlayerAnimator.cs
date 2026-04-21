using System.Collections;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{

    [SerializeField] private PlayerMovement movement;
    [SerializeField] private SpriteRenderer sr;

    public Sprite currentSprite => sr.sprite;

    private Sprite baseSprite;
    [SerializeField] private Sprite dashSprite;
    [SerializeField] private Sprite parrySprite;

    public bool IsFacingRight {  get; private set; }

    private Animator animator;
    
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        baseSprite = sr.sprite;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDashStart += HandleDashAnimation;
        GameEvents.OnPlayerParryStart += HandleParryAnimation;
        GameEvents.OnPlayerParryEnd += HandleParryEnd;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDashStart -= HandleDashAnimation;
        GameEvents.OnPlayerParryStart -= HandleParryAnimation;
        GameEvents.OnPlayerParryEnd -= HandleParryEnd;
    }

    private void Update()
    {
        HandleMovementAnimation();
    }

    private void HandleMovementAnimation()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            if (rb.linearVelocity.x > 0 && !IsFacingRight)
            {
                FlipFacing();
            }
            else if(rb.linearVelocity.x < 0 && IsFacingRight)
            {
                FlipFacing();
            }
        }
    }

    private void HandleDashAnimation()
    {
        StartCoroutine(DashAnimationRoutine());
    }

    private IEnumerator DashAnimationRoutine()
    {
        sr.sprite = dashSprite;

        while(movement.IsDashing)
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
