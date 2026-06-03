using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTools : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    private Player p;

    public bool ToolEquipped => equippedTool != null;
    public PlayerTool equippedTool;

    private void Awake()
    {
        p = GetComponent<Player>();
        inputActions = InputManager.Instance.inputActions;

        if (equippedTool != null)
            equippedTool.OnEquip(p);
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
        // Re-impliment when tools are more setup
        //UseEquippedTool();
    }


    public void EquipTool(PlayerTool tool)
    {
        if (tool == null) return;

        UnequipCurrentTool();
        equippedTool = tool;
        equippedTool.OnEquip(p);
    }

    public void UnequipCurrentTool()
    {
        if (equippedTool != null)
        {
            equippedTool.OnUnequip();
            equippedTool = null;
        }
    }

    private void UseEquippedTool()
    {
        if (!ToolEquipped) return;

        if (equippedTool.UseTool(p.GetMouseDirection()))
            Debug.Log($"{equippedTool.ToolName} used!");
    }

    public bool TryInterceptWithTool(HitData hit)
    {
        if (!ToolEquipped) return false;
        return equippedTool.TryIntercept(hit);
    }

    #region Utlities


    #endregion
}
