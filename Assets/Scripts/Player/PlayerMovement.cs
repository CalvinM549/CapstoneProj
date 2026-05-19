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

    private InputSystem_Actions inputActions;
    private Rigidbody2D rb;

    private Vector2 moveDirection;
    private Vector2 lastMoveDirection = Vector2.right;

    public Vector2 LastMoveDirection => lastMoveDirection;
    public Vector2 CurrentMoveDirection => rb.linearVelocity;

    private BufferedInput bufferedDash;

    private int currentDashCharges;
    private float[] dashRechargeTimers;

    private bool isDashing;
    private bool isForcedPush;
    private bool isSelfPush;
    private bool isParrying = false;

    private int inputLockSources;
    public bool InputLocked => inputLockSources > 0;

    public bool IsMoving => rb.linearVelocity.magnitude > 0.01;
    public bool IsDashing => isDashing;
    public bool MoveInputting;

    public float TimeStopped;

    private Coroutine currentDashRoutine;
    private Coroutine currentPushRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        p = GetComponent<Player>();


        playerLayerIndex = LayerMask.NameToLayer("Player");
        enemyLayerIndex = LayerMask.NameToLayer("Enemy");
        
    }

    private void OnEnable()
    {
        inputActions = InputManager.Instance.inputActions;

        inputActions.Player.Move.performed += HandleMoveStart;
        inputActions.Player.Move.canceled += HandleMoveStop;
        inputActions.Player.Dash.performed += HandleDashInput;

        GameEvents.OnPlayerParryStart += HandleParryStart;
        GameEvents.OnPlayerParryEnd += HandleParryEnd;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= HandleMoveStart;
        inputActions.Player.Move.canceled -= HandleMoveStop;
        inputActions.Player.Dash.performed -= HandleDashInput;

        GameEvents.OnPlayerParryStart -= HandleParryStart;
        GameEvents.OnPlayerParryEnd -= HandleParryEnd;
    }

    private void Start()
    {
        currentDashCharges = data.maxDashCharges;
        dashRechargeTimers = new float[data.maxDashCharges];
        
    }

    private void Update()
    {
        UpdateDashRecharge();
        FlushInputBuffer();
    }

    private void FixedUpdate()
    {
        if(!IsDashing && !isForcedPush && !isSelfPush && !isParrying)  
            ApplyMovement();
    }

    #region InputHandling

    private void HandleMoveStart(InputAction.CallbackContext ctx)
    {
        moveDirection = ctx.ReadValue<Vector2>();
        MoveInputting = true;
    }

    private void HandleMoveStop(InputAction.CallbackContext ctx)
    {
        moveDirection = Vector2.zero;
        MoveInputting = true;
    }

    private void HandleDashInput(InputAction.CallbackContext ctx)
    {
        if (CanDash())
        {
            StartDash();
        }
        else if (ShouldBufferDash())
        {
            bufferedDash = new();
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

    private void FlushInputBuffer()
    {
        //Dash Buffer

        if (bufferedDash == null) return;
        if (!bufferedDash.isValid(data.inputBufferWindow))
        {
            bufferedDash = null;
            return;
        }

        if (!CanDash()) return;

        bufferedDash = null;
        StartDash();
    }

    #endregion

    #region BaseMovement

    private void ApplyMovement()
    {
        if(!p.Health.IsAlive) moveDirection = Vector2.zero;

        switch (p.Combat.CurrentState)
        {
            case CombatState.Startup:
            case CombatState.Active:
                rb.linearVelocity = Vector2.MoveTowards(
                    rb.linearVelocity, Vector2.zero, 
                    data.deceleration * 2.0f * Time.fixedDeltaTime);
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
        if (moveDirection.magnitude > 0.1f)
        {
            lastMoveDirection = moveDirection.normalized;
            float accel = GetAcceleration() * multiplier;
            Vector2 targetVelocity = moveDirection.normalized * data.baseSpeed * multiplier * p.Momentum.GetMomentumSpeedMultiplier();
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, accel * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, data.deceleration * Time.fixedDeltaTime);
        }
    }

    private float GetAcceleration()
    {
        bool isTurning = Vector2.Dot(rb.linearVelocity.normalized, moveDirection.normalized) < -0.3f;
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


        Vector2 dashDir = moveDirection.magnitude > 0.1f ? moveDirection.normalized : lastMoveDirection;

        currentDashCharges--;
        currentDashRoutine = StartCoroutine(DashRoutine(dashDir));
    }

    private IEnumerator DashRoutine(Vector2 direction)
    {

        isDashing = true;

        GameEvents.PlayerDashStart();
        p.Health.GrantIFrames(data.dashIFrameDuration);
        p.VFX.PlayDashTrail();


        Physics2D.IgnoreLayerCollision(playerLayerIndex, enemyLayerIndex, true);
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
        
        rb.linearVelocity *= data.dashExitMultiplier;

        Physics2D.IgnoreLayerCollision(playerLayerIndex, enemyLayerIndex, false);

        GameEvents.PlayerDashEnd();

        StartDashRecharge();

        isDashing = false;
    }

    private void CancelDash()
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

        float[] dashPercentages = new float[dashRechargeTimers.Length];

        for (int i = 0; i < dashRechargeTimers.Length; i++)
        {
            if (dashRechargeTimers[i] <= 0)
            {
                dashPercentages[i] = 1f;
                
                continue;
            }

            dashRechargeTimers[i] -= Time.deltaTime * rechargeRate;

            if (dashRechargeTimers[i] <= 0)
            {
                dashRechargeTimers[i] = 0;
                currentDashCharges = Mathf.Min(currentDashCharges + 1, data.maxDashCharges);
            }

            dashPercentages[i] = Mathf.Abs((dashRechargeTimers[i] / data.dashRechargeTime) - 1);
        }

        GameEvents.DashChargeChange(dashPercentages);
    }

    #endregion

    #region Push / Hitstun

    public void PushPlayer(Vector2 direction, float force, float duration, bool isForcedMovement = true)
    {
        if (force <= 0) return;
        if (direction.normalized.magnitude < 0.01f) return;

        CancelDash();

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

    #region Utilities

    private void HandleParryStart()
    {
        isParrying = true;
        rb.linearVelocity = Vector2.zero;
    }

    private void HandleParryEnd()
    {
        isParrying = false;
    }

    #endregion
}
