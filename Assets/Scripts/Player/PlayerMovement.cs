using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private MovementData data;

    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerHealth health;
    [SerializeField] private PlayerVFX vfx;

    private InputSystem_Actions inputActions;
    private Rigidbody2D rb;
    private SpriteRenderer sr; // For Testing

    private Vector2 moveDirection;
    private Vector2 lastMoveDirection = Vector2.right;

    public Vector2 LastMoveDirection => lastMoveDirection;

    private int currentDashCharges;
    private float[] dashRechargeTimers;

    private bool isDashing;
    private bool isPushing;
    public bool lateDashCheck { get; private set; }

    public bool IsDashing => isDashing;
    public float TimeStopped;

    private Coroutine currentDashRoutine;
    private Coroutine currentPushRoutine;

    private int lastBroadcastCharges = -1;
    private float lastBroadcastTimerSum = -1f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        inputActions = InputManager.Instance.inputActions;

        inputActions.Player.Move.performed += OnPlayerMove;
        inputActions.Player.Move.canceled += OnPlayerStop;
        inputActions.Player.Dash.performed += OnPlayerDash;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnPlayerMove;
        inputActions.Player.Move.canceled -= OnPlayerStop;
        inputActions.Player.Dash.performed -= OnPlayerDash;
    }

    private void Start()
    {
        currentDashCharges = data.maxDashCharges;
        dashRechargeTimers = new float[data.maxDashCharges];
    }

    private void Update()
    {
        HandleDashRecharge();
        TryBroadcastDashState();
    }

    private void FixedUpdate()
    {
        if(!IsDashing && !isPushing)  
            ApplyMovement();
    }

    #region BaseMovement

    private void OnPlayerMove(InputAction.CallbackContext ctx)
    {
        moveDirection = ctx.ReadValue<Vector2>();
    }

    private void OnPlayerStop(InputAction.CallbackContext ctx)
    {
        moveDirection = Vector2.zero;
        // call stop movement
    }

    private void OnPlayerDash(InputAction.CallbackContext ctx)
    {
        if (currentDashCharges > 0 && !IsDashing && !isPushing)
            StartDash();
    }

    private void ApplyMovement()
    {
        switch (combat.CurrentState)
        {
            case CombatState.Startup:
            case CombatState.Active:
                rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, data.deceleration * 2.0f * Time.fixedDeltaTime);
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
            Vector2 targetVelocity = moveDirection.normalized * data.baseSpeed * multiplier;
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

    #region DashFunctions

    private void StartDash()
    {
        Vector2 dashDir = moveDirection.magnitude > 0.1f ? moveDirection.normalized : lastMoveDirection;

        currentDashCharges--;
        currentDashRoutine = StartCoroutine(DashRoutine(dashDir));
    }

    private IEnumerator DashRoutine(Vector2 direction)
    {
        isDashing = true;
        lateDashCheck = true;

        rb.linearVelocity = direction * data.dashSpeed;

        health.GrantIFrames(data.dashIFrameDuration);
        vfx.PlayDashTrail();

        yield return new WaitForSeconds(data.dashDuration);

        isDashing = false;
        rb.linearVelocity *= data.dashExitMultiplier;

        StartDashRecharge();

        yield return new WaitForSeconds(0.1f);

        lateDashCheck = false;
    }

    private void CancelDash()
    {
        if (!isDashing) return;
        if(currentDashRoutine != null)
            StopCoroutine(currentDashRoutine);

        isDashing = false;
        lateDashCheck = false;
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
        print("SHOULD NOT REACH");
    }

    private void HandleDashRecharge()
    {
        float rechargeRate = 1.0f;

        for (int i = 0; i < dashRechargeTimers.Length; i++)
        {
            if (dashRechargeTimers[i] <= 0) continue;

            dashRechargeTimers[i] -= Time.deltaTime * rechargeRate;

            if (dashRechargeTimers[i] <= 0)
            {
                dashRechargeTimers[i] = 0;
                currentDashCharges = Mathf.Min(currentDashCharges + 1, data.maxDashCharges);
                BroadcastDashState();
            }
        }
    }

    #endregion

    #region Utilities

    public void PushPlayer(Vector2 direction, float force, float duration)
    {
        if (force <= 0) return;
        if (direction.normalized.magnitude < 0.01f) return;

        if (currentPushRoutine != null)
        {
            StopCoroutine(currentPushRoutine);
            isPushing = false;
        }

        currentPushRoutine = StartCoroutine(PushRoutine(direction.normalized, force, duration));
    }

    private IEnumerator PushRoutine(Vector2 direction, float force, float duration)
    {
        isPushing = true;
        
        rb.linearVelocity = direction * force;

        yield return new WaitForSeconds(duration);

        rb.linearVelocity *= 0.4f;
        
        isPushing = false;
    }

    private void TryBroadcastDashState()
    {
        float timerSum = 0f;
        bool anyActive = false;
        for (int i = 0; i < dashRechargeTimers.Length; i++)
        {
            timerSum += dashRechargeTimers[i];
            if (dashRechargeTimers[i] > 0) anyActive = true;
        }

        if (!anyActive && currentDashCharges == lastBroadcastCharges) return;
        if (currentDashCharges == lastBroadcastCharges &&
            Mathf.Approximately(timerSum, lastBroadcastTimerSum)) return;

        BroadcastDashState();
        lastBroadcastTimerSum = timerSum;
    }

    private void BroadcastDashState()
    {
        lastBroadcastCharges = currentDashCharges;
        GameEvents.DashChargeChange(currentDashCharges, data.maxDashCharges,
                                    dashRechargeTimers, data.dashRechargeTime);
    }

    #endregion
}
