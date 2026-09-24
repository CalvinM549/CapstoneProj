using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public WeaponData data;

    private Player p;

    private StatValue globalDamage;
    private StatValue meleeDamage;
    private StatValue rangedDamage;

    [SerializeField] private PlayerMeleeController hitboxes;

    private GameplayInputReader input;

    private InputBuffer meleeBuffer;
    private InputBuffer rangedBuffer;

    private Coroutine holdDetectRoutine;

    public CombatState CurrentState => currentState;
    private CombatState currentState = CombatState.Idle;
    
    private Coroutine currentMeleeRoutine;
    private Coroutine currentRangedRoutine;
    
    private AttackType currentAttackType;
    private bool currentAttackConnected = false;

    public bool IsAttacking { get; private set; }

    // Light Combos
    public int ComboStep => comboStep;
    public AttackType CurrentAttackType => currentAttackType;
    private int comboStep;
    private float comboWindowTimer = 0f;
    private float comboCooldownTimer = 0f;

    // Ranged
    public PlayerWeapon EquippedWeapon { get; private set; }
    public bool RangedWeaponEquipped => EquippedWeapon != null;

    private float rangedCooldownTimer;

    public float rangedCooldownPercent => RangedWeaponEquipped && EquippedWeapon.cooldown > 0f
        ? Mathf.Clamp01(rangedCooldownTimer / EquippedWeapon.cooldown) : 0f;

    private bool dashCancelWindow;
    private Coroutine dashCancelRoutine;

    public int CurrentAmmo => EquippedWeapon.currentAmmo;
    private int currentAmmo;
    public event Action<int> OnAmmoChanged;
    public event Action<int> OnWeaponChanged;
    public event Action<float> OnReloadChanged;

    public void Initialize(PlayerWeapon weapon)
    {
        // Gather statsValue refs
        globalDamage = p.Stats.GetStatValue(StatRef.PlayerOutgoingDamageMult);
        meleeDamage = p.Stats.GetStatValue(StatRef.PlayerMeleeDamageMult);
        rangedDamage = p.Stats.GetStatValue(StatRef.PlayerRangedDamageMult);

        if(weapon != null)
            EquipRangedWeapon(weapon);
    }

    public void RestoreFromSave()
    {

    }

    private void Awake()
    {
        p = GetComponent<Player>();

        meleeBuffer = new InputBuffer(0.1f);
        rangedBuffer = new InputBuffer(0.1f);
    }

    private void Start()
    {

    }

    private void OnEnable()
    {
        input = InputManager.Instance.GameplayInputs;

        input.MeleePressed += OnLightAttackInput;
        input.MeleeCanceled += OnMeleeInputCancelled;

        input.RangedPressed += OnRangedInputStarted;
        input.RangedCanceled += OnRangedAttackCancel;

        // Events
        hitboxes.OnPlayerHitboxContact += HandleHitDetection;

        GameEvents.OnPlayerDashStart += HandleDashStart;
    }

    private void OnDisable()
    {
        // Inputs
        input.MeleePressed -= OnLightAttackInput;
        input.MeleeCanceled -= OnMeleeInputCancelled;

        input.RangedPressed -= OnRangedInputStarted;
        input.RangedCanceled -= OnRangedAttackCancel;

        // Events
        hitboxes.OnPlayerHitboxContact -= HandleHitDetection;

        GameEvents.OnPlayerDashStart -= HandleDashStart;

        if (RangedWeaponEquipped) ProjectilePools.ReleasePool(EquippedWeapon);
    }

    private void Update()
    {
        UpdateComboWindow();
        UpdateComboCooldown();
        UpdateRangedCooldown();

        meleeBuffer.TryConsume(CanMeleeAttack, () => HandleMeleeInput(true));
    }

    #region Input Setup

    private void OnLightAttackInput()
    {
        if (holdDetectRoutine != null)
            StopCoroutine(holdDetectRoutine);

        holdDetectRoutine = StartCoroutine(HeavyHoldRoutine());
    }

    private void OnMeleeInputCancelled()
    {
        if (holdDetectRoutine == null) return;

        StopCoroutine(holdDetectRoutine);
        holdDetectRoutine = null;

        ProcessMeleeInput(true);
    }

    private IEnumerator HeavyHoldRoutine()
    {
        yield return new WaitForSeconds(data.heavyHoldThreshold);
        holdDetectRoutine = null;
        ProcessMeleeInput(false);
    }

    private void ProcessMeleeInput(bool isLightInput)
    {
        if (!CanAttack()) return;

        if (CanMeleeAttack())
        {
            HandleMeleeInput(isLightInput);
        }

        else if (isLightInput ? ShouldBufferLight() : ShouldBufferHeavy())
        {
            if (isLightInput)
                meleeBuffer.Buffer();
            else
                meleeBuffer.Buffer();
        }
    }

    private void OnRangedInputStarted()
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

    private void OnRangedAttackCancel()
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

    #endregion

    #region Attack Input Gates

    private bool CanAttack()
    {
        return (
            !TimescaleManager.IsPaused
            && p.CanAct
            && p.Health.IsAlive
            && !p.Health.IsHitstunned);
    }

    private bool CanMeleeAttack()
    {
        return (
            comboCooldownTimer <= 0f 
            && CurrentState is CombatState.Idle or CombatState.Recovery);
    }

    private bool CanRangedAttack()
    {
        return (
            RangedWeaponEquipped 
            && rangedCooldownTimer <= 0f 
            && RangedAttackCheck() 
            && EquippedWeapon.CanFire());
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

        if(currentMeleeRoutine != null)
            StopCoroutine(currentMeleeRoutine);

        currentMeleeRoutine = StartCoroutine(MeleeAttackRoutine(data.lightAttacks[comboStep - 1]));
    }

    private void PerformHeavyAttack()
    {
        comboStep = 0;
        comboWindowTimer = 0;

        if (currentMeleeRoutine != null)
            StopCoroutine(currentMeleeRoutine);

        currentMeleeRoutine = StartCoroutine(MeleeAttackRoutine(data.heavyAttack));

    }

    // Attack Routine

    private IEnumerator MeleeAttackRoutine(AttackInfo attack)
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

        if (Physics2D.Raycast(transform.position, direction, distance, p.WallLayer))
        {
            // Wall stagger / particles?
            return;
        }


        Vector2 knockbackDir = direction.normalized;

        HitData hitData = new HitData(attack.damage, attack.type, true)
        {
            sourcePos = transform.position,
            knockbackDirection = knockbackDir,
            knockbackForce = attack.knockback,
            hitstopTime = attack.hitstopDuration,
            hitstunTime = attack.hitstunTime,
            isParryable = false
        };

        hitData.AddModifier(globalDamage.Value - 1f, StatModType.PercentAdd, this);
        hitData.AddModifier(meleeDamage.Value - 1f, StatModType.PercentAdd, this);

        p.Upgrades.ModifyOutgoingHit(hitData);

        currentAttackConnected = true;

        GameEvents.HitConfirmed(hitData);

        VFXManager.Instance.PlayVFX(VFXType.HitCross, hit.transform.position);

        // Lock to melee targets
        p.Targeting.SetLock(hit.GetComponent<EnemyController>());

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

        if (EquippedWeapon.fireType == FireType.Independent)
        {
            EquippedWeapon.Fire(GetAttackDirection(AttackType.Projectile));
            return;
        }

        if (currentRangedRoutine != null)
            StopCoroutine(currentRangedRoutine);

        currentRangedRoutine = StartCoroutine(RangedAttackRoutine(EquippedWeapon));
    }

    private IEnumerator RangedAttackRoutine(PlayerWeapon weapon)
    {
        //Vector2 direction = GetAttackDirection(AttackType.Secondary);

        // STARTUP
        currentState = CombatState.Startup;

        GameEvents.AttackStarted(AttackType.Projectile, GetAttackDirection(AttackType.Projectile));

        yield return new WaitForSeconds(weapon.windupTime);

        if (EquippedWeapon != weapon)
        {
            currentState = CombatState.Idle;
            currentRangedRoutine = null;
            yield break;
        }

        // ACTIVE
        currentState = CombatState.Active;
        Vector2 direction = GetAttackDirection(AttackType.Projectile);
        weapon.Fire(direction);

        yield return new WaitForSeconds(weapon.activeTime);

        // RECOVERY
        currentState = CombatState.Recovery;

        yield return new WaitForSeconds(weapon.recoveryTime);

        if(currentState == CombatState.Recovery)
            currentState = CombatState.Idle;

        currentRangedRoutine = null;

    }

    public void EquipRangedWeapon(PlayerWeapon weapon)
    {
        if (weapon == EquippedWeapon) return;

        if (RangedWeaponEquipped)
        {
            EquippedWeapon.OnUnequip();
            ProjectilePools.ReleasePool(weapon);
        }

        EquippedWeapon = weapon;
        currentAmmo = 0;
        rangedCooldownTimer = 0f;

        if (weapon != null)
        {
            currentAmmo = weapon.baseAmmo;

            Debug.Log($"[PlayerCombat] new weapon {weapon.name} equipped");
            ProjectilePools.RequestPool(weapon);
            weapon.OnEquip(p, OnAmmoChanged);
        }

        OnWeaponChanged?.Invoke(currentAmmo);
        OnAmmoChanged?.Invoke(currentAmmo);

        // Fire Event
    }

    public void UnequipRangedWeapon() => EquipRangedWeapon(null);

    public void RestoreAmmo(int amount)
    {
        if (!RangedWeaponEquipped) return;

        var weapon = EquippedWeapon;

        weapon.currentAmmo = Mathf.Min(weapon.currentAmmo + amount, weapon.baseAmmo);
        OnAmmoChanged.Invoke(weapon.currentAmmo);
    }

    public void RestoreAmmo(float percent)
    {

    }

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
        if (rangedCooldownTimer > 0f)
        {
            rangedCooldownTimer -= Time.deltaTime;
            if(RangedWeaponEquipped)
                OnReloadChanged?.Invoke(rangedCooldownTimer / EquippedWeapon.cooldown);
        }
    }

    private void InterruptRecovery()
    {
        if (currentState != CombatState.Recovery) return;

        GameEvents.RecoveryCancel(currentAttackType);

        StopAllAttackRoutines();
        currentState = CombatState.Idle;
    }

    public void InterruptActive()
    {
        if (CurrentState != CombatState.Active) return;

        //Event?

        StopAllAttackRoutines();
        currentState = CombatState.Idle;
    }

    private void StopAllAttackRoutines()
    {
        if (currentMeleeRoutine != null)
        {
            StopCoroutine(currentMeleeRoutine);
            currentMeleeRoutine = null;
            hitboxes.ResetHitboxes();
        }

        if (currentRangedRoutine != null)
        {
            StopCoroutine(currentRangedRoutine);
            currentRangedRoutine = null;
        }
    }

    private Vector2 GetAttackDirection(AttackType type)
    {
        switch (type)
        {
            case AttackType.Light:
            case AttackType.Heavy:
                return p.GetMouseDirection();

            case AttackType.Projectile:
                if (p.Targeting.HasTarget)
                    return p.GetTargetDirection();
                else
                    return p.GetMouseDirection();
        }

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
