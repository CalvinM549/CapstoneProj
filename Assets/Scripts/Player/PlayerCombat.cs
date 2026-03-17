using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{

    public WeaponData weaponData;

    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerHealth health;
    
    [SerializeField] private AttackHitboxes hitboxes;

    private InputSystem_Actions inputActions;

    private IPlayerTool equippedTool;

    public CombatState CurrentState => state;

    private CombatState state = CombatState.Idle;
    
    private Coroutine currentAttackRoutine;
    private AttackType currentAttackType;

    private int comboStep;
    private float comboWindowTimer = 0f;
    private bool heavyConnected = false;
    private bool dashAttackUsedInDash = false;

    public int ComboStep => comboStep;


    private void Awake()
    {
        inputActions = InputManager.Instance.inputActions;
        // Link hitboxes to handle hit detection
    }

    private void OnEnable()
    {
        // Inputs
        inputActions.Player.LightAttack.performed += OnLightAttackInput;
        inputActions.Player.HeavyAttack.performed += OnHeavyAttackInput;

        // Events
        hitboxes.OnPlayerHitboxContact += HandleHitDetection;
    }

    private void OnDisable()
    {
        inputActions.Player.LightAttack.performed -= OnLightAttackInput;
        inputActions.Player.HeavyAttack.performed -= OnHeavyAttackInput;

    }

    private void Update()
    {
        UpdateComboWindow();
    }

    private void OnLightAttackInput(InputAction.CallbackContext ctx)
    {
        Debug.Log("Light Attack Pressed!");

        if (!CheckCanAttack()) return;

        if (CheckForDashAttack())
        {
            PerformDashAttack();
            return;
        }
        else
        {
            InterruptRecovery();
            PerformLightAttack();
        }
    }

    private void OnHeavyAttackInput(InputAction.CallbackContext ctx)
    {
        Debug.Log("Heavy Attack Pressed!");

        if (!CheckCanAttack()) return;

        if (CheckForDashAttack())
        {
            PerformDashAttack();
            return;
        }
        else
        {
            InterruptRecovery();
            PerformHeavyAttack();
        }
    }

    private bool CheckForDashAttack()
    {
        return movement.isDashing && !dashAttackUsedInDash;
    }

    private bool CheckCanAttack()
    {
        return (state == CombatState.Idle || state == CombatState.Recovery)
            && !health.IsHitstunned;
    }

    private void PerformLightAttack()
    {
        if (comboWindowTimer > 0 && comboStep < weaponData.maxComboSteps)
            comboStep++;
        else
            comboStep = 1;

        comboWindowTimer = weaponData.comboWindow;

        if(currentAttackRoutine != null)
            StopCoroutine(currentAttackRoutine);

        Debug.Log("Performing Light Attack!");
        currentAttackRoutine = StartCoroutine(AttackRoutine(weaponData.lightAttacks[comboStep - 1]));
    }

    private void PerformHeavyAttack()
    {
        comboStep = 0;
        comboWindowTimer = 0;
        heavyConnected = false;

        if (currentAttackRoutine != null)
            StopCoroutine(currentAttackRoutine);

        Debug.Log("Performing Heavy Attack!");
        currentAttackRoutine = StartCoroutine(AttackRoutine(weaponData.heavyAttack));

    }

    private void PerformDashAttack()
    {
        dashAttackUsedInDash = true;
        comboStep = 0;
        comboWindowTimer = 0;

        if (currentAttackRoutine != null)
            StopCoroutine(currentAttackRoutine);

        Debug.Log("Performing Dash Attack!");
        currentAttackRoutine = StartCoroutine(AttackRoutine(weaponData.dashAttack));
    }

    // Attack Routine

    private IEnumerator AttackRoutine(AttackInfo attack)
    {
        currentAttackType = attack.type;
        // Trigger attack start event

        state = CombatState.Startup;

        yield return new WaitForSeconds(attack.startupTime);

        if (currentAttackType != attack.type) yield break;

        state = CombatState.Active;
        hitboxes.EnableHitBox(attack, comboStep);

        yield return new WaitForSeconds(attack.activeTime);
        
        hitboxes.ResetHitboxes();

        if (attack.type == AttackType.Heavy && !heavyConnected)
            print("Heavy Whiff");
            // Whiff event

        state = CombatState.Recovery;

        yield return new WaitForSeconds(attack.recoveryTime);

        if (state == CombatState.Recovery)
        {
            state = CombatState.Idle;
            // End Attack event
        }
    }

    private void HandleHitDetection(Collider2D hit, AttackInfo attack)
    {
        print("Hit Detected");

        IDamageable target = hit.GetComponentInParent<IDamageable>();
        if (target == null || !target.IsAlive) return;

        Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;

        HitData damageData = new HitData()
        {
            damage = attack.damage,
            attackType = attack.type,
            sourcePos = transform.position,
            knockbackDirection = knockbackDir,
            knockbackForce = attack.knockback,
            hitstopTime = attack.hitstopDuration,
            isPlayerAttack = true,
            isParryable = false
        };

        if(attack.type == AttackType.Heavy)
            heavyConnected = true;

        GameEvents.HitConfirmed(damageData);

        target.RecieveHit(damageData);
    }

    // returns false if hit is ignored
    public bool HandlePlayerHit(HitData damage)
    {
        return true;
    }


    #region Utility

    private void InterruptRecovery()
    {
        if (state != CombatState.Recovery) return;
        if (currentAttackRoutine != null)
            StopCoroutine(currentAttackRoutine);

        // remove hitbox
        state = CombatState.Idle;
    }

    private void UpdateComboWindow()
    {
        if (comboWindowTimer > 0)
        {
            comboWindowTimer -= Time.deltaTime;
        }
        else if (state == CombatState.Idle)
        {
            comboStep = 0;
        }
    }

    private void RestartRoutine(IEnumerator routine)
    {
        StopCurrentRoutine();
        currentAttackRoutine = StartCoroutine(routine);
    }

    private void StopCurrentRoutine()
    {
        if (currentAttackRoutine != null)
        {
            StopCoroutine(currentAttackRoutine);
            currentAttackRoutine = null;
        }
    }

    #endregion
}
