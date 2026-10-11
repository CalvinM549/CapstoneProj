using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTools : MonoBehaviour
{
    private Player p;

    private GameplayInputReader input;

    private StatValue toolRechargeRate;

    public bool HasToolEquipped => EquippedTool != null;
    public PlayerTool EquippedTool { get; private set; }

    private float toolCooldownTimer;

    public float ToolCooldownPercent => HasToolEquipped && EquippedTool.cooldown > 0f 
        ? Mathf.Clamp01(toolCooldownTimer / EquippedTool.cooldown)
        : 0f;

    public void Initialize(PlayerTool tool)
    {
        if (EquippedTool != null)
            EquippedTool.OnEquip(p);
    }

    public void RestoreFromSave(PlayerEquipmentSave save, GameDatabase db)
    {
        if (string.IsNullOrEmpty(save.equippedTool)) return;

        EquipTool(db.tools.Get(save.equippedTool));
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
        if (HasToolEquipped) 
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
        if (!HasToolEquipped) return;
        if (!EquippedTool.CanUse()) return;

        EquippedTool.UseTool(p.GetMouseDirection());
    }

    public bool TryInterceptWithTool(HitData hit)
    {
        if (!HasToolEquipped) return false;
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
            if (HasToolEquipped)
            {
                // fire event for ui
            }
        }
    }
}
