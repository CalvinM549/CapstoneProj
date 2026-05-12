using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{

    public WeaponData data;
    private Player p;

    [SerializeField] private AttackHitboxes hitboxes;
    [SerializeField] private LayerMask wallLayer;

    private InputSystem_Actions inputActions;
    private BufferedInput bufferedHeavy;
    private BufferedInput bufferedLight;

    public CombatState CurrentState => currentState;

    private CombatState currentState = CombatState.Idle;
    
    private Coroutine currentAttackRoutine;
    private AttackType currentAttackType;
    private bool currentAttackConnected = false;

    
    public bool IsAttacking { get; private set; }

    // Light Combos
    public int ComboStep => comboStep;
    public AttackType CurrentAttackType => currentAttackType;
    private int comboStep;
    private float comboWindowTimer = 0f;
    private float comboCooldownTimer = 0f;
        
    private bool dashAttackWindow;

    private void Awake()
    {
        p = GetComponent<Player>();

        inputActions = InputManager.Instance.inputActions;
    }

    private void OnEnable()
    {
        // Inputs
        inputActions.Player.LightAttack.performed += OnLightAttackInput;
        inputActions.Player.HeavyAttack.performed += OnHeavyAttackInput;

        // Events
        hitboxes.OnPlayerHitboxContact += HandleHitDetection;

        GameEvents.OnPlayerDashStart += HandleDashStart;
        GameEvents.OnPlayerDashEnd += HandleDashEnd;
    }

    private void OnDisable()
    {
        // Inputs
        inputActions.Player.LightAttack.performed -= OnLightAttackInput;
        inputActions.Player.HeavyAttack.performed -= OnHeavyAttackInput;

        // Events
        hitboxes.OnPlayerHitboxContact -= HandleHitDetection;

        GameEvents.OnPlayerDashStart -= HandleDashStart;
        GameEvents.OnPlayerDashEnd -= HandleDashEnd;
    }

    private void Update()
    {
        UpdateComboWindow();
        UpdateComboCooldown();

        FlushInputBuffer();
    }

    #region Input Setup

    private void OnLightAttackInput(InputAction.CallbackContext ctx)
    {
        if (TimescaleManager.IsPaused) return;

        if (CanAttack())
        {
            HandleAttackInput(true);
        }
        else if(ShouldBufferLight())
        {
            bufferedLight = new();
        }
    }

    private void OnHeavyAttackInput(InputAction.CallbackContext ctx)
    {
        if (TimescaleManager.IsPaused) return;

        if (CanAttack())
        {
            HandleAttackInput(false);
        }
        else if(ShouldBufferHeavy())
        {
            bufferedHeavy = new();
        }
    }
    
    private bool ShouldBufferLight()
    {
        return true;
    }

    private bool ShouldBufferHeavy()
    {
        return true;
    }

    private void FlushInputBuffer()
    {
        // Light Buffer
        if (bufferedLight != null && bufferedLight.isValid(0.1f) && CanAttack())
        {
            bufferedLight = null;
            HandleAttackInput(true);
        }

        // Heavy Buffer
        if (bufferedHeavy != null && bufferedHeavy.isValid(0.1f) && CanAttack())
        {
            bufferedHeavy = null;
            HandleAttackInput(false);
        }
        
    }

    #endregion

    private bool CanAttack()
    {
        if (p.Health.IsHitstunned) return false;
        if (comboCooldownTimer > 0) return false;
        if (currentState == CombatState.Active || 
            currentState == CombatState.Startup) return false;

        return true;
    }

    private void HandleAttackInput(bool isLightInput)
    {
        if (currentState == CombatState.Recovery)
            InterruptRecovery();

        if (dashAttackWindow)
            PerformDashAttack();
        else
        {
            if (isLightInput)
                PerformLightAttack();
            else
                PerformHeavyAttack();
        }
    }

    private void PerformLightAttack()
    {
        if (comboWindowTimer > 0 && comboStep < data.maxComboSteps)
            comboStep++;
        else
            comboStep = 1;

        comboWindowTimer = data.comboWindow;

        if(currentAttackRoutine != null)
            StopCoroutine(currentAttackRoutine);

        currentAttackRoutine = StartCoroutine(AttackRoutine(data.lightAttacks[comboStep - 1]));
    }

    private void PerformHeavyAttack()
    {
        comboStep = 0;
        comboWindowTimer = 0;

        if (currentAttackRoutine != null)
            StopCoroutine(currentAttackRoutine);

        currentAttackRoutine = StartCoroutine(AttackRoutine(data.heavyAttack));

    }

    private void PerformDashAttack()
    {
        dashAttackWindow = false; // set to false to prevent multiple attacks in same dash
        comboStep = 0;
        comboWindowTimer = 0;

        if (currentAttackRoutine != null)
            StopCoroutine(currentAttackRoutine);

        currentAttackRoutine = StartCoroutine(AttackRoutine(data.dashAttack));
    }

    // Attack Routine

    private IEnumerator AttackRoutine(AttackInfo attack)
    {
        currentAttackType = attack.type;
        currentAttackConnected = false;

        // STARTUP
        currentState = CombatState.Startup;
        GameEvents.AttackStarted(attack.type);

        yield return new WaitForSeconds(attack.startupTime);

        if (currentAttackType != attack.type) yield break; // Cancel attack if it changes somehow
        //

        // ACTIVE
        currentState = CombatState.Active;
        
        p.Movement.PushPlayer(GetAttackDirection(attack.type), attack.dashForce, attack.activeTime, false);
        hitboxes.EnableHitBox(attack, GetAttackDirection(attack.type), comboStep);

        yield return new WaitForSeconds(attack.activeTime);
        
        hitboxes.ResetHitboxes();

        if (!currentAttackConnected)
            GameEvents.AttackWhiff(currentAttackType);

        if (comboStep == data.maxComboSteps)
            comboCooldownTimer = data.comboCooldown;

        GameEvents.AttackEnded(attack.type);
        //
        
        // RECOVERY
        currentState = CombatState.Recovery;

        yield return new WaitForSeconds(attack.recoveryTime);

        if (currentState == CombatState.Recovery)
            currentState = CombatState.Idle;
        //
    }

    private void HandleHitDetection(Collider2D hit, AttackInfo attack)
    {
        print($"Player Hit {hit.gameObject.name} with {attack.type}");

        //Check if target is damageable
        IDamageable target = hit.GetComponentInParent<IDamageable>();
        if (target == null || !target.IsAlive) return;

        Vector2 direction = hit.transform.position - transform.position;
        float distance = Vector2.Distance(transform.position, hit.transform.position);

        //Raycast Check for walls
        if (Physics2D.Raycast(transform.position, direction, distance, wallLayer)) return;
            // Wall stagger / particles


        Vector2 knockbackDir = direction.normalized;

        HitData hitData = new HitData()
        {
            damage = attack.damage,
            attackType = attack.type,
            sourcePos = transform.position,
            knockbackDirection = knockbackDir,
            knockbackForce = attack.knockback,
            hitstopTime = attack.hitstopDuration,
            hitstunTime = attack.hitstunTime,
            isPlayerAttack = true,
            isParryable = false
        };

        currentAttackConnected = true;

        GameEvents.HitConfirmed(hitData);

        target.RecieveHit(hitData);
    }

    #region Utility

    private void UpdateComboWindow()
    {
        if (comboWindowTimer > 0)
        {
            comboWindowTimer -= Time.deltaTime;
        }
        else if (currentState == CombatState.Idle)
        {
            comboStep = 0;
        }
    }

    private void UpdateComboCooldown()
    {
        if(comboCooldownTimer > 0f)
            comboCooldownTimer -= Time.deltaTime;
    }

    private void InterruptRecovery()
    {
        if (currentState != CombatState.Recovery) return;

        GameEvents.RecoveryCancel(currentAttackType);

        StopAttackRoutine();
        currentState = CombatState.Idle;
    }

    public void InterruptActive()
    {
        if (CurrentState != CombatState.Active) return;

        //Event?

        StopAttackRoutine();
        currentState = CombatState.Idle;
    }

    private void StopAttackRoutine()
    {
        if (currentAttackRoutine != null)
        {
            StopCoroutine(currentAttackRoutine);
            currentAttackRoutine = null;
            hitboxes.ResetHitboxes();
        }
    }

    private Vector2 GetAttackDirection(AttackType type)
    {
        if (type == AttackType.DashAttack)
            return p.Movement.LastMoveDirection;

        else
        {
            return p.GetMouseDirection();
        }
    }

    private void HandleDashStart()
    {
        dashAttackWindow = true;
        InterruptRecovery();
    }

    private void HandleDashEnd()
    {
        StartCoroutine(DashWindowRoutine());
    }

    private IEnumerator DashWindowRoutine()
    {
        yield return new WaitForSeconds(data.dashExtraWindow);
        dashAttackWindow = false;
    }

    #endregion
}
