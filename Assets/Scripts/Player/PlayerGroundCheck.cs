using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    private Player p;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform checkPosition; 

    private Vector2 lastSafePosition;

    private bool wasGroundedLastFrame;

    private void Awake()
    {
        p = GetComponent<Player>();
        lastSafePosition = transform.position;
    }

    private void FixedUpdate()
    {
        HandleGroundCheck();
    }

    private void HandleGroundCheck()
    {
        if (p.Movement.IsDashing) return;

        bool isGrounded = IsPlayerOverGround();


        if (isGrounded)
        {
            lastSafePosition = transform.position;
            wasGroundedLastFrame = true;
        }
        else if (wasGroundedLastFrame)
        {
            wasGroundedLastFrame = false;
            FallIntoVoid();
        }
    }

    private bool IsPlayerOverGround()
    {
        return Physics2D.OverlapPoint(checkPosition.position, groundLayer) != null;
    }

    private void FallIntoVoid()
    {
        // Do things

        // Reset player velocity

        print("You have fallen into the abyss");
    }
}
