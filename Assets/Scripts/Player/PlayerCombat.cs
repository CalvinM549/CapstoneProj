using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public bool useDashCancel;
    [Space]

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

    // Ranged

    [SerializeField] private PlayerWeapon tempWeapon;
    public PlayerWeapon EquippedWeapon { get; private set; }
    public bool RangedWeaponEquipped => EquippedWeapon != null;

    private float rangedCooldownTimer;
    public float rangedCooldownPercent => RangedWeaponEquipped && EquippedWeapon.cooldown > 0f
        ? Mathf.Clamp01(rangedCooldownTimer / EquippedWeapon.cooldown) : 0f;

    private bool dashCancelWindow;
    private Coroutine dashCancelRoutine;

    private void Awake()
    {
        p = GetComponent<Player>();

        inputActions = InputManager.Instance.inputActions;
    }

    private void Start()
    {
        EquipRangedWeapon(tempWeapon);
    }

    private void OnEnable()
    {
        // Inputs
        inputActions.Player.LightAttack.performed += OnLightAttackInput;
        inputActions.Player.HeavyAttack.performed += OnHeavyAttackInput;
        inputActions.Player.RangedAttack.performed += OnRangedAttackInput;
        inputActions.Player.RangedAttack.canceled += OnRangedAttackCancel;

        // Events
        hitboxes.OnPlayerHitboxContact += HandleHitDetection;

        GameEvents.OnPlayerDashStart += HandleDashStart;
        //GameEvents.OnPlayerDashEnd += HandleDashEnd;
    }

    private void OnDisable()
    {
        // Inputs
        inputActions.Player.LightAttack.performed -= OnLightAttackInput;
        inputActions.Player.HeavyAttack.performed -= OnHeavyAttackInput;
        inputActions.Player.RangedAttack.performed -= OnRangedAttackInput;
        inputActions.Player.RangedAttack.canceled -= OnRangedAttackCancel;

        // Events
        hitboxes.OnPlayerHitboxContact -= HandleHitDetection;

        GameEvents.OnPlayerDashStart -= HandleDashStart;
        //GameEvents.OnPlayerDashEnd -= HandleDashEnd;

        if (RangedWeaponEquipped) ProjectilePools.ReleasePool(EquippedWeapon);
    }

    private void Update()
    {
        UpdateComboWindow();
        UpdateComboCooldown();
        UpdateRangedCooldown();

        FlushInputBuffer();
    }

    #region Input Setup

    private void OnLightAttackInput(InputAction.CallbackContext ctx)
    {
        if (!CanAttack()) return;

        if (CanMeleeAttack())
        {
            HandleMeleeInput(true);
        }
        else if(ShouldBufferLight())
        {
            bufferedLight = new();
        }
    }

    private void OnHeavyAttackInput(InputAction.CallbackContext ctx)
    {
        if (!CanAttack()) return;

        if (CanMeleeAttack())
        {
            HandleMeleeInput(false);
        }
        else if(ShouldBufferHeavy())
        {
            bufferedHeavy = new();
        }
    }

    private void OnRangedAttackInput(InputAction.CallbackContext ctx)
    {
        if (!CanAttack()) return;

        if (CanRangedAttack())
        {
            FireRangedWeapon();
        }
        else if (ShouldBufferRanged())
        {
            //
        }
    }

    private void OnRangedAttackCancel(InputAction.CallbackContext ctx)
    {

    }

    private bool ShouldBufferLight()
    {
        return true;
    }

    private bool ShouldBufferHeavy()
    {
        return true;
    }

    private bool ShouldBufferRanged()
    {
        return false;
    }

    private void FlushInputBuffer()
    {
        // Light Buffer
        if (bufferedLight != null && bufferedLight.isValid(0.1f) && CanMeleeAttack())
        {
            print("DoingBufferedLight");
            bufferedLight = null;
            HandleMeleeInput(true);
        }

        // Heavy Buffer
        if (bufferedHeavy != null && bufferedHeavy.isValid(0.1f) && CanMeleeAttack())
        {
            print("DoingBufferedHeavy");
            bufferedHeavy = null;
            HandleMeleeInput(false);
        }
        
    }

    #endregion

    #region Attack Input Gates

    private bool CanAttack()
    {
        if (TimescaleManager.IsPaused) return false;
        if (p.Health.IsHitstunned) return false;
        if (!p.Health.IsAlive) return false;

        if (!useDashCancel && p.Movement.IsDashing) return false;

        return true;
    }

    private bool CanMeleeAttack()
    {
        if (comboCooldownTimer > 0) return false;
        if (currentState is CombatState.Active or CombatState.Startup) return false;

        return true;
    }

    private bool CanRangedAttack()
    {
        if (!RangedWeaponEquipped) return false;
        if (rangedCooldownTimer > 0f) return false;
        if (!RangedAttackCheck()) return false;
        if (!EquippedWeapon.CanFire()) return false;

        return true;
    }

    private bool RangedAttackCheck()
    {
        return EquippedWeapon.fireType switch
        {
            FireType.Linked => currentState is not (CombatState.Startup or CombatState.Active),
            FireType.Blocked => currentState is CombatState.Idle,
            FireType.Independent => true,
            _ => true
        };
    }

    #endregion

    #region Melee Attacks

    private void HandleMeleeInput(bool isLightInput)
    {
        if (currentState == CombatState.Recovery)
            InterruptRecovery();

        //if (dashAttackWindow)
        //    PerformDashAttack();

        if (dashCancelWindow)
            p.Movement.InterruptDash();

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
        Vector2 direction = GetAttackDirection(attack.type);

        GameEvents.AttackStarted(attack.type, direction);

        yield return new WaitForSeconds(attack.startupTime);

        if (currentAttackType != attack.type) yield break; // Cancel attack if it changes somehow
        //

        // ACTIVE
        currentState = CombatState.Active;
        
        p.Movement.PushPlayer(direction, attack.dashForce, attack.activeTime, false);
        hitboxes.EnableHitBox(attack, direction, comboStep);

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
        //print($"[PlayerCombat] Player Hit {hit.gameObject.name} with {attack.type}");

        //Check if target is damageable
        IDamageable target = hit.GetComponentInParent<IDamageable>();
        if (target == null || !target.IsAlive) return;

        //Raycast Check for walls
        Vector2 direction = hit.transform.position - transform.position;
        float distance = Vector2.Distance(transform.position, hit.transform.position);

        if (Physics2D.Raycast(transform.position, direction, distance, wallLayer))
        {
            // Wall stagger / particles?
            return;
        }


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

    #endregion

    #region Ranged Attacks

    private void FireRangedWeapon()
    {
        if (dashCancelWindow)
            p.Movement.InterruptDash();

        if (currentState == CombatState.Recovery && EquippedWeapon.fireType == FireType.Linked)
            InterruptRecovery();

        rangedCooldownTimer = EquippedWeapon.cooldown;

        EquippedWeapon.Fire(GetAttackDirection(AttackType.Secondary));
    }

    public void EquipRangedWeapon(PlayerWeapon weapon)
    {
        if (RangedWeaponEquipped)
        {
            EquippedWeapon.OnUnequip();
            ProjectilePools.ReleasePool(weapon);
        }

        EquippedWeapon = weapon;
        rangedCooldownTimer = 0f;

        if (weapon != null)
        {

            Debug.Log($"[PlayerCombat] new weapon {weapon.name} equipped");
            ProjectilePools.RequestPool(weapon);
            weapon.OnEquip(p);
        }

        // Fire Event
    }

    public void UnequipRangedWeapon() => EquipRangedWeapon(null);

    #endregion


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

    private void UpdateRangedCooldown()
    {
        if(rangedCooldownTimer > 0f)
            rangedCooldownTimer -= Time.deltaTime;
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
        switch (type)
        {
            case AttackType.Light:
            case AttackType.Heavy:
                return p.GetMouseDirection();

            case AttackType.Secondary:
                if (p.Targeting.HasTarget)
                    return p.GetTargetDirection();
                else
                    return p.GetMouseDirection();
        }
        if (type == AttackType.DashAttack)
            return p.Movement.LastMoveDirection;

        else
            return p.GetMouseDirection();
    }

    private void HandleDashStart()
    {
        //dashAttackWindow = true;

        StartDashCancelWindow();
        InterruptRecovery();
    }

    //private void HandleDashEnd()
    //{
    //    StartCoroutine(DashWindowRoutine());
    //}

    //private IEnumerator DashWindowRoutine()
    //{
    //    yield return new WaitForSeconds(data.dashExtraWindow);
    //    dashAttackWindow = false;
    //}

    private void StartDashCancelWindow()
    {
        if(dashCancelRoutine != null)
            StopCoroutine(dashCancelRoutine);

        dashCancelRoutine = StartCoroutine(DashCancelRoutine());
    }

    private IEnumerator DashCancelRoutine()
    {
        dashCancelWindow = true;
        yield return new WaitForSeconds(data.dashExtraWindow);
        dashCancelWindow = false;
    }

    #endregion
}
