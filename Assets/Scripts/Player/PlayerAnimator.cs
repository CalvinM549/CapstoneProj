using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{

    [SerializeField] private PlayerMovement movement;

    private Animator animator;

    private SpriteRenderer sr;
    private Rigidbody2D rb;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
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


    public void PlayAttackAnimation(AttackType attackType, Vector2 direction)
    {
        // Convert direction to cardinals

        // Play animation based on attack type
    }
}
