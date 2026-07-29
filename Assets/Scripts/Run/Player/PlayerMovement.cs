using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public enum MovementCondition
{
    Dashing,
    ForcedPush,
    SelfPush,
    Stunned,
    Interacting,
    UI,


    // Uniques
    Cuscene,
    Parrying
}

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private MovementData data;

    private int playerLayerIndex;
    private int enemyLayerIndex;

    private Player p;

    private InputReader input;
    private Rigidbody2D rb;

    private Vector2 inputDirection;
    private Vector2 lastMoveDirection = Vector2.right;

    public Vector2 LastMoveDirection => lastMoveDirection;
    public Vector2 CurrentMoveDirection => rb.linearVelocity;

    private InputBuffer dashBuffer;

    private int currentDashCharges;
    private float[] dashRechargeTimers;
    private float[] dashRechargePercentages;

    private bool isDashing;
    // Sliding?
    private bool isForcedPush;
    private bool isSelfPush;

    private bool isOverrideMovement;
    private bool isUsingTool;


    private int inputLockSources;
    public bool InputLocked => inputLockSources > 0;

    public bool IsMoving => rb.linearVelocity.magnitude > 0.01;
    public bool IsDashing => isDashing;
    private bool moveInputting;

    private Coroutine currentDashRoutine;
    private Coroutine currentPushRoutine;

    #region MonobehaviourThings

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        p = GetComponent<Player>();

        playerLayerIndex = LayerMask.NameToLayer("Player");
        enemyLayerIndex = LayerMask.NameToLayer("Enemy");

        dashBuffer = new InputBuffer(data.inputBufferWindow);
    }

    private void OnEnable()
    {
        input = InputManager.Instance.PlayerInputs;

        input.MoveChanged += HandleMoveChanged;
        input.DashPressed += HandleDashInput;
    }

    private void OnDisable()
    {
        input.MoveChanged -= HandleMoveChanged;
        input.DashPressed -= HandleDashInput;
    }

    private void Start()
    {
        currentDashCharges = data.maxDashCharges;
        dashRechargeTimers = new float[data.maxDashCharges];
        dashRechargePercentages = new float[data.maxDashCharges];
    }

    private void Update()
    {
        UpdateDashRecharge();
        dashBuffer.TryConsume(CanDash, StartDash);
    }

    private void FixedUpdate()
    {
        if(CanMove())  
            ApplyMovement();
    }

    #endregion

    #region InputHandling

    private void HandleMoveChanged(Vector2 direction)
    {
        inputDirection = direction;
        moveInputting = direction.sqrMagnitude > 0.001f;
    }

    private void HandleDashInput()
    {
        if (CanDash())
        {
            StartDash();
        }
        else if (ShouldBufferDash())
        {
            dashBuffer.Buffer();
        }
    }

    #endregion

    #region InputBuffering

    private bool ShouldBufferDash()
    {
        if (currentDashCharges <= 0) return false;
        if (IsDashing) return false;

        return true;
    }

    #endregion

    #region BaseMovement

    private bool CanMove()
    {
        if (IsDashing) return false;
        if (isForcedPush) return false;
        if (isSelfPush) return false;
        if (isOverrideMovement) return false;
        if (isUsingTool) return false;

        return true;
    }

    private void ApplyMovement()
    {
        if(!p.Health.IsAlive) inputDirection = Vector2.zero;

        switch (p.Combat.CurrentState)
        {
            case CombatState.Startup:
            case CombatState.Active:
                DecelerateToZero(1f);
                break;

            case CombatState.Recovery:
                ApplyModifiedMovement(0.35f);
                break;

            default:
                ApplyModifiedMovement();
                break;
        }
    }

    private void ApplyModifiedMovement(float multiplier = 1.0f)
    {
        if (inputDirection.magnitude > 0.1f)
        {

            lastMoveDirection = inputDirection.normalized;
            
            float accel = GetAcceleration() * multiplier;
            Vector2 targetVelocity = data.baseSpeed * multiplier * p.Momentum.GetMomentumSpeedMultiplier() * inputDirection.normalized;

            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, accel * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, data.deceleration * Time.fixedDeltaTime);
        }
    }

    private void DecelerateToZero(float multiplier = 1.0f)
    {
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, data.deceleration * multiplier * Time.fixedDeltaTime);
    }

    private float GetAcceleration()
    {
        bool isTurning = Vector2.Dot(rb.linearVelocity.normalized, inputDirection.normalized) < -0.3f;
        return data.acceleration * (isTurning ? data.turnMultiplier : 1.0f);
    }

    #endregion

    #region Dash Functions

    private bool CanDash()
    {
        if (isDashing) return false;
        if (isForcedPush) return false;
        if (currentDashCharges <= 0) return false;
        if (!p.Health.IsAlive) return false;
        if (p.Combat.CurrentState == CombatState.Startup) return false;

        return true;
    }

    private void StartDash()
    {
        // Do Interrupts
        if (isSelfPush)
            InterruptCurrentPush();

        if (p.Combat.CurrentState == CombatState.Active)
            p.Combat.InterruptActive();


        Vector2 dashDir = inputDirection.magnitude > 0.1f ? inputDirection.normalized : lastMoveDirection;

        currentDashCharges--;
        currentDashRoutine = StartCoroutine(DashRoutine(dashDir));
    }

    private IEnumerator DashRoutine(Vector2 direction)
    {

        isDashing = true;

        // Start Events
        GameEvents.PlayerDashStart();
        p.Health.GrantIFrames(data.dashIFrameDuration);
        p.VFX.PlayDashTrail();
        Physics2D.IgnoreLayerCollision(playerLayerIndex, enemyLayerIndex, true);
        //

        rb.linearVelocity = direction * data.dashSpeed;

        yield return new WaitForSeconds(data.dashDuration);

        EndDash();
    }

    private void EndDash()
    {

        //Vector2 exitVelocity = moveDirection.magnitude > 0.1f
        //    ? Vector2.Lerp(CurrentMoveDirection, moveDirection.normalized, 0.5f) * data.baseSpeed
        //    : data.baseSpeed * data.dashExitMultiplier * CurrentMoveDirection;

        //rb.linearVelocity = exitVelocity;
        
        //rb.linearVelocity *= data.dashExitMultiplier;

        Vector2 exitDir = inputDirection.magnitude > 0.1f 
            ? Vector2.Lerp(CurrentMoveDirection.normalized, inputDirection.normalized, 0.6f) 
            : CurrentMoveDirection.normalized;
        float exitSpeed = rb.linearVelocity.magnitude * data.dashExitMultiplier;
        rb.linearVelocity = exitDir * exitSpeed;

        Physics2D.IgnoreLayerCollision(playerLayerIndex, enemyLayerIndex, false);

        GameEvents.PlayerDashEnd();

        StartDashRecharge();

        isDashing = false;
    }

    public void InterruptDash()
    {
        if (!isDashing) return;
        if(currentDashRoutine != null)
            StopCoroutine(currentDashRoutine);

        EndDash();
    }

    private void StartDashRecharge()
    {
        for (int i = 0; i < dashRechargeTimers.Length; i++)
        {
            if (dashRechargeTimers[i] <= 0)
            {
                dashRechargeTimers[i] = data.dashRechargeTime;
                return;
            }
        }
    }

    private void UpdateDashRecharge()
    {
        float rechargeRate = 1.0f; // alter based on things ig

        for (int i = 0; i < dashRechargeTimers.Length; i++)
        {
            if (dashRechargeTimers[i] <= 0)
            {
                dashRechargePercentages[i] = 1f;
                
                continue;
            }

            dashRechargeTimers[i] -= Time.deltaTime * rechargeRate;

            if (dashRechargeTimers[i] <= 0)
            {
                dashRechargeTimers[i] = 0;
                currentDashCharges = Mathf.Min(currentDashCharges + 1, data.maxDashCharges);
            }

            dashRechargePercentages[i] = Mathf.Abs((dashRechargeTimers[i] / data.dashRechargeTime) - 1);
        }

        GameEvents.DashChargeChange(dashRechargePercentages);
    }

    #endregion

    #region Push / Hitstun

    public void PushPlayer(Vector2 direction, float force, float duration, bool isForcedMovement = true)
    {
        if (force <= 0) return;
        if (direction.normalized.magnitude < 0.01f) return;

        InterruptDash();

        InterruptCurrentPush();

        currentPushRoutine = StartCoroutine(PushRoutine(direction.normalized, force, duration, isForcedMovement));
    }

    private IEnumerator PushRoutine(Vector2 direction, float force, float duration, bool isForcedMovement)
    {
        if (isForcedMovement)
            isForcedPush = true;
        else
            isSelfPush = true;
        
        rb.linearVelocity = direction * force;

        if (isForcedMovement)
            rb.linearVelocity *= data.knockbackMultiplier;

        yield return new WaitForSeconds(duration);

        rb.linearVelocity *= data.pushExitMultiplier;
        
        isSelfPush = false;
        isForcedPush = false;
    }

    private void InterruptCurrentPush()
    {
        if (currentPushRoutine == null) return;

        StopCoroutine(currentPushRoutine);
        currentPushRoutine = null;

        isSelfPush = false;
        isForcedPush = false;
    }

    #endregion

    #region Meta Utility

    public void MoveToPosition(Vector2 targetPos)
    {
        isOverrideMovement = true;
    }

    public void MoveInDirection(Direction direction, float duration)
    {
        isOverrideMovement = true;
    }

    public void StopMoveOverride()
    {
        // Stop Coroutine

        isOverrideMovement = false;
    }

    private IEnumerator AutoMoveRoutine(Vector3 targetPos)
    {
        yield return new WaitUntil(() => transform.position == targetPos);
    }

    #endregion
}
