using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTools : MonoBehaviour
{
    private Player p;

    private GameplayInputReader input;

    private StatValue toolRechargeRate;

    public bool ToolEquipped => EquippedTool != null;
    public PlayerTool EquippedTool;

    private float toolCooldownTimer;

    public float ToolCooldownPercent => ToolEquipped && EquippedTool.cooldown > 0f 
        ? Mathf.Clamp01(toolCooldownTimer / EquippedTool.cooldown)
        : 0f;

    public void Initialize(PlayerTool tool)
    {
        if (EquippedTool != null)
            EquippedTool.OnEquip(p);
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
            EquippedTool.UpdateTool();

        UpdateToolCooldown();
    }

    private void OnUseToolInput()
    {
        UseEquippedTool();
    }

    public void EquipTool(PlayerTool tool)
    {
        if (tool == null) return;

        UnequipCurrentTool();
        EquippedTool = tool;
        EquippedTool.OnEquip(p);
    }

    public void UnequipCurrentTool()
    {
        if (EquippedTool != null)
        {
            EquippedTool.OnUnequip();
            EquippedTool = null;
        }
    }

    private void UseEquippedTool()
    {
        if (!ToolEquipped) return;
        if (!EquippedTool.CanUse()) return;

        EquippedTool.UseTool(p.GetMouseDirection());
    }

    public bool TryInterceptWithTool(HitData hit)
    {
        if (!ToolEquipped) return false;
        return EquippedTool.TryIntercept(hit);
    }

    public void ReduceCurrentCooldown(float percent)
    {
        toolCooldownTimer *= percent;
    }

    private void UpdateToolCooldown()
    {
        if (toolCooldownTimer > 0f)
        {
            toolCooldownTimer -= Time.deltaTime;
            if (ToolEquipped)
            {
                // fire event for ui
            }
        }
    }
}
