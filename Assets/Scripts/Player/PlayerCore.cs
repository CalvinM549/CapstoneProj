using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCore : MonoBehaviour
{
    public static PlayerCore Instance { get; private set; }
    private InputSystem_Actions inputActions;

    public PlayerMovement movement;
    public PlayerHealth health;
    public PlayerCombat combat;
    public PlayerMomentum momentum;
    public PlayerAnimator playerAnimator;

    public bool ToolEquipped => equippedTool != null;
    public PlayerTool equippedTool;

    // Mutable Stats
    //

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        inputActions = InputManager.Instance.inputActions;
    }

    private void OnEnable()
    {
        inputActions.Player.UseTool.performed += OnUseToolInput;
    }

    private void OnDisable()
    {
        inputActions.Player.UseTool.performed -= OnUseToolInput;
    }

    private void Update()
    {
        if (ToolEquipped) // Update equipped tool
            equippedTool.UpdateTool();
    }

    private void OnUseToolInput(InputAction.CallbackContext context)
    {
        UseEquippedTool();
    }


    public void EquipTool(PlayerTool tool)
    {
        if (tool == null) return;

        UnequipCurrentTool();
        equippedTool = tool;
        equippedTool.OnEquip();
        // Fire Event
    }

    public void UnequipCurrentTool()
    {
        if (equippedTool != null)
        {
            // Fire Event
            equippedTool.OnUnequip();
            equippedTool = null;
        }
    }



    private void UseEquippedTool()
    {
        if (!ToolEquipped) return;

        equippedTool.UseTool(GetMouseDirection());
    }

    #region Utlities

    public Vector2 GetMouseDirection()
    {
        Vector3 mousePos = InputManager.Instance.inputActions.Player.PointerPosition.ReadValue<Vector2>();
        Vector3 mousePosWorld = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3 direction = (mousePosWorld - transform.position).normalized;
        return direction;
    }

    #endregion
}
