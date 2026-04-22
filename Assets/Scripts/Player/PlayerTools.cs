using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTools : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    public bool ToolEquipped => equippedTool != null;
    public PlayerTool equippedTool;

    private void Awake()
    {
        inputActions = InputManager.Instance.inputActions;
        if (equippedTool != null)
            equippedTool.OnEquip(this.transform);
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
        // Update equipped tool
        if (ToolEquipped) 
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
        equippedTool.OnEquip(transform);
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

        if (equippedTool.UseTool(GetMouseDirection()))
            Debug.Log($"{equippedTool.ToolName} used!");
    }

    public bool TryInterruptWithTool(HitData hit)
    {
        if (!ToolEquipped) return false;
        return equippedTool.TryIntercept(hit);
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
