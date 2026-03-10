using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = InputManager.Instance.inputActions;
    }

    private void OnEnable()
    {
        inputActions.Player.Attack.performed += OnAttackInput;
    }

    private void OnDisable()
    {
        
    }

    private void Update()
    {
        
    }

    private void OnAttackInput(InputAction.CallbackContext ctx)
    {

    }
}
