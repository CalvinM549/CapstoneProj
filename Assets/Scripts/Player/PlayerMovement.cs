using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private MovementData data;

    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerHealth health;

    private InputSystem_Actions inputActions;
    private Rigidbody2D rb;
    private SpriteRenderer sr; // For Testing

    private Vector2 _moveDirection;
    private Vector2 lastMoveDirection = Vector2.right;

    private int currentDashCharges;
    private float[] dashRechargeTimers;

    private bool _isDashing;
    public bool isDashing => _isDashing;

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
    }

    private void FixedUpdate()
    {
        if(!isDashing)  
            ApplyMovement();
    }

    #region BaseMovement

    private void OnPlayerMove(InputAction.CallbackContext ctx)
    {
        _moveDirection = ctx.ReadValue<Vector2>();
    }

    private void OnPlayerStop(InputAction.CallbackContext ctx)
    {
        _moveDirection = Vector2.zero;
    }

    private void OnPlayerDash(InputAction.CallbackContext ctx)
    {
        if (currentDashCharges > 0 && !isDashing)
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
        if (_moveDirection.magnitude > 0.1f)
        {
            lastMoveDirection = _moveDirection.normalized;
            float accel = GetAcceleration() * multiplier;
            Vector2 targetVelocity = _moveDirection.normalized * data.baseSpeed * multiplier;
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, accel * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, data.deceleration * Time.fixedDeltaTime);
        }
    }

    private float GetAcceleration()
    {
        bool isTurning = Vector2.Dot(rb.linearVelocity.normalized, _moveDirection.normalized) < -0.3f;
        return data.acceleration * (isTurning ? data.turnMultiplier : 1.0f);
    }

    #endregion

    #region DashFunctions

    private void StartDash()
    {
        Vector2 dashDir = _moveDirection.magnitude > 0.1f ? _moveDirection.normalized : lastMoveDirection;

        currentDashCharges--;
        StartCoroutine(DashRoutine(dashDir));
    }

    private IEnumerator DashRoutine(Vector2 direction)
    {
        _isDashing = true;

        rb.linearVelocity = direction * data.dashSpeed;

        health.GrantIFrames(data.dashIFrameDuration);

        yield return new WaitForSeconds(data.dashDuration);

        _isDashing = false;
        rb.linearVelocity *= data.dashExitMultiplier;

        StartDashRecharge();
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
            }
        }
    }

    #endregion
}
