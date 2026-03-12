using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{

    public WeaponData weaponData;

    public PlayerMovement movement;

    private InputSystem_Actions inputActions;

    private CombatState state = CombatState.Idle;
    
    private Coroutine currentAttackRoutine;
    private AttackType currentAttackType;

    private int comboStep;
    private float comboWindowTimer = 0f;
    private bool heavyConnected = false;
    private bool dashAttackUsedInDash = false;




    private void Awake()
    {
        inputActions = InputManager.Instance.inputActions;
    }

    private void OnEnable()
    {
        inputActions.Player.Attack.performed += OnLightAttackInput;

        // Link hitboxes to handle hit detection
    }

    private void OnDisable()
    {
        
    }

    private void Update()
    {
        
    }

    private void OnLightAttackInput(InputAction.CallbackContext ctx)
    {
        if (CheckForDashAttack())
        {
            PerformDashAttack();
            return;
        }

        if (CheckCanAttack())
        {
            InterruptRecovery();
            PerformLightAttack();
        }
    }

    private void OnHeavyAttackInput(InputAction.CallbackContext ctx)
    {
        if (CheckForDashAttack())
        {
            PerformDashAttack();
            return;
        }

        if (CheckCanAttack())
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
        return state == CombatState.Idle || state == CombatState.Recovery;
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

        currentAttackRoutine = StartCoroutine(AttackRoutine(weaponData.lightAttacks[comboStep - 1], comboStep - 1));
    }

    private void PerformHeavyAttack()
    {
        comboStep = 0;
        comboWindowTimer = 0;
        heavyConnected = false;

        if (currentAttackRoutine != null)
            StopCoroutine(currentAttackRoutine);

        currentAttackRoutine = StartCoroutine(AttackRoutine(weaponData.heavyAttack, 0));

    }

    private void PerformDashAttack()
    {
        dashAttackUsedInDash = true;
        comboStep = 0;
        comboWindowTimer = 0;
        // Cancel dash in movement

        if (currentAttackRoutine != null)
            StopCoroutine(currentAttackRoutine);

        currentAttackRoutine = StartCoroutine(AttackRoutine(weaponData.dashAttack, 0));
    }

    // Attack Routine

    private IEnumerator AttackRoutine(HitInfo attack, int step)
    {
        currentAttackType = attack.type;
        // Trigger attack start event

        state = CombatState.Startup;

        yield return new WaitForSeconds(attack.startupTime);

        if (currentAttackType != attack.type) yield break;

        state = CombatState.Active;
        // Hitbox Enabling idk

        yield return new WaitForSeconds(attack.activeTime);

        // De-activate hitbox

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

    private void HandleHitDetection(Collider2D hit, HitInfo attack)
    {
        Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;

        DamageData damageData = new DamageData()
        {
            damage = attack.damage,
            attackType = attack.type,
            sourcePos = transform.position,
            knockbackDirection = knockbackDir,
            knockbackForce = attack.knockback,
            isPlayerAttack = true
        };

        // Fire events

        heavyConnected = attack.type == AttackType.Heavy ? true : heavyConnected;
    }

    private void InterruptRecovery()
    {
        if (state != CombatState.Recovery) return;
        if (currentAttackRoutine != null)
            StopCoroutine(currentAttackRoutine);

        // remove hitbox
        state = CombatState.Idle;
    }
}
