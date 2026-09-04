using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTools : MonoBehaviour
{
    private Player p;

    private GameplayInputReader input;

    private StatValue toolRechargeRate;

    public bool ToolEquipped => equippedTool != null;
    public PlayerTool equippedTool;

    public void Initialize(PlayerTool tool)
    {
        if (equippedTool != null)
            equippedTool.OnEquip(p);
    }

    public void RestoreFromSave()
    {

    }

    private void Awake()
    {
        p = GetComponent<Player>();
    }

    private void OnEnable()
    {
        input = InputManager.Instance.GameplayInputs;

        input.ToolPressed += OnUseToolInput;
    }

    private void OnDisable()
    {
        input.ToolPressed -= OnUseToolInput;
    }

    private void Update()
    {
        // Update equipped tool
        if (ToolEquipped) 
            equippedTool.UpdateTool();
    }

    private void OnUseToolInput()
    {
        UseEquippedTool();
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
            Debug.Log($"[PlayerTools] {equippedTool.toolName} used!");
    }

    public bool TryInterceptWithTool(HitData hit)
    {
        if (!ToolEquipped) return false;
        return equippedTool.TryIntercept(hit);
    }
}
