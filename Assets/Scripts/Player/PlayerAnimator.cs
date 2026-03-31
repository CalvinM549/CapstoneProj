using System.Collections;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{

    [SerializeField] private PlayerMovement movement;

    private Sprite baseSprite;
    [SerializeField] private Sprite dashSprite;

    public bool IsFacingRight {  get; private set; }

    private Animator animator;

    private SpriteRenderer sr;
    private Rigidbody2D rb;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        baseSprite = sr.sprite;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDash += HandleDashAnimation;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDash -= HandleDashAnimation;
    }

    private void Update()
    {
        HandleMovementAnimation();
    }

    private void HandleMovementAnimation()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            if (rb.linearVelocity.x > 0 && sr.flipX == false)
            {
                sr.flipX = true;
            }
            else if(rb.linearVelocity.x < 0 && sr.flipX == true)
            {
                sr.flipX = false;
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

    public void PlayAttackAnimation(AttackType attackType, Vector2 direction)
    {
        // Convert direction to cardinals

        // Play animation based on attack type
    }

    protected void FlipFacing()
    {
        IsFacingRight = !IsFacingRight;

        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
}
