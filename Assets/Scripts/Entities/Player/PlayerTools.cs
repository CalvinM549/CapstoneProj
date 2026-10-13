using System;
using UnityEngine;

public class PlayerTools : MonoBehaviour
{
    private Player p;
    private GameplayInputReader input;

    public ActiveTool EquippedTool { get; private set; }
    public bool HasToolEquipped => EquippedTool != null;

    public event Action<ActiveTool> OnToolChanged;   // equip and unequip, for building UI
    public event Action<ActiveTool> OnToolUpdated;   // charges, active state, failed use

    public void Initialize(PlayerTool tool) => EquipTool(tool);

    public void RestoreFromSave(PlayerEquipmentSave save, GameDatabase db)
    {
        if (string.IsNullOrEmpty(save.equippedTool)) return;
        EquipTool(db.tools.Get(save.equippedTool), save.toolCharges);
    }

    private void Awake()
    {
        p = GetComponent<Player>();
    }

    private void OnEnable()
    {
        input = InputManager.Instance.GameplayInputs;

        input.ToolPressed += HandleToolPressed;
        input.ToolCanceled += HandleToolCanceled;
        GameEvents.OnPlayerTookDamage += HandlePlayerDamaged;
    }

    private void OnDisable()
    {
        input.ToolPressed -= HandleToolPressed;
        input.ToolCanceled -= HandleToolCanceled;
        GameEvents.OnPlayerTookDamage -= HandlePlayerDamaged;

        EndActive(ToolEndReason.Cancelled);
    }

    private void Update()
    {
        if (!HasToolEquipped || TimescaleManager.IsPaused) return;

        var t = EquippedTool;
        float dt = Time.deltaTime;

        if (t.cooldownTimer > 0f)
            t.cooldownTimer -= dt;

        TickRecharge(t, dt);

        if (t.isActive)
        {
            t.activeTime += dt;
            t.data.OnTick(t, dt);

            if (t.isActive && t.data.duration > 0f && t.activeTime >= t.data.duration)
                EndActive(ToolEndReason.Expired);
        }
    }

    #region Equip

    public void EquipTool(PlayerTool tool, int chargeOverride = -1)
    {
        if (tool == null) return;

        UnequipCurrentTool();

        EquippedTool = tool != null ? new ActiveTool(tool, p, chargeOverride) : null;

        tool.OnEquip(EquippedTool);

        OnToolChanged?.Invoke(EquippedTool);
    }

    public void UnequipCurrentTool()
    {
        if (!HasToolEquipped) return;

        EndActive(ToolEndReason.Cancelled);
        EquippedTool.data.OnUnequip(EquippedTool);
        EquippedTool = null;

        OnToolChanged?.Invoke(null);
    }

    #endregion

    #region Use

    private bool CanAct() =>
        !TimescaleManager.IsPaused
        && p.CanAct
        && p.Health.IsAlive
        && !p.Health.IsHitstunned;

    private void HandleToolPressed()
    {
        if (!HasToolEquipped || !CanAct()) return;
        var t = EquippedTool;

        if (t.isActive)
        {
            if (t.data.activation == ToolActivation.Window && t.data.canEndEarly)
                EndActive(ToolEndReason.Cancelled);
            return;
        }

        if (!t.data.CanUse(t))
        {
            OnToolUpdated?.Invoke(t); // UI can flash "unavailable"
            return;
        }

        Activate(t, p.GetMouseDirection());
    }

    private void HandleToolCanceled()
    {
        if (!HasToolEquipped) return;
        var t = EquippedTool;

        if (t.isActive && t.data.activation == ToolActivation.Channel)
            EndActive(ToolEndReason.Released);
    }

    private void Activate(ActiveTool t, Vector2 direction)
    {
        var data = t.data;

        // Pay
        if (t.charges == data.maxCharges) t.rechargeTimer = data.rechargeTime;
        t.charges--;
        t.cooldownTimer = data.cooldown;

        if (data.momentumCost > 0f)
            p.Momentum.SpendMomentumPercent(data.momentumCost);

        // Run
        t.direction = direction;
        t.activeTime = 0f;
        t.isActive = data.activation != ToolActivation.Instant;

        if (t.isActive && data.lockMovement)
            p.Movement.SetToolLock(true);

        data.OnUse(t);

        t.NotifyChanged();
        OnToolUpdated?.Invoke(t);
    }

    public void EndActive(ToolEndReason reason)
    {
        if (!HasToolEquipped || !EquippedTool.isActive) return;
        var t = EquippedTool;

        t.isActive = false;

        if (t.data.lockMovement)
            p.Movement.SetToolLock(false);

        t.data.OnEnd(t, reason);

        t.NotifyChanged();
        OnToolUpdated?.Invoke(t);
    }

    public void CancelActive() => EndActive(ToolEndReason.Cancelled);

    #endregion

    #region Charges

    private void TickRecharge(ActiveTool t, float dt)
    {
        if (t.charges >= t.data.maxCharges || t.data.rechargeTime <= 0f) return;

        t.rechargeTimer -= dt;
        if (t.rechargeTimer > 0f) return;

        t.charges++;
        t.rechargeTimer = t.charges < t.data.maxCharges ? t.data.rechargeTime : 0f;

        t.NotifyChanged();
        OnToolUpdated?.Invoke(t);
    }

    /// <summary> Cuts the remaining recharge time by a fraction (0-1). </summary>
    public void RefundRecharge(float fraction)
    {
        if (!HasToolEquipped) return;
        EquippedTool.rechargeTimer *= 1f - Mathf.Clamp01(fraction);
    }

    public void GrantCharge(int amount = 1)
    {
        if (!HasToolEquipped) return;
        var t = EquippedTool;
        t.charges = Mathf.Min(t.charges + amount, t.data.maxCharges);
        if (t.charges >= t.data.maxCharges) t.rechargeTimer = 0f;

        t.NotifyChanged();
        OnToolUpdated?.Invoke(t);
    }

    #endregion

    #region Damage

    public bool TryInterceptWithTool(HitData hit)
    {
        if (!HasToolEquipped || !EquippedTool.isActive) return false;
        return EquippedTool.data.TryIntercept(EquippedTool, hit);
    }

    private void HandlePlayerDamaged(HitData hit)
    {
        if (HasToolEquipped && EquippedTool.isActive && EquippedTool.data.interruptOnDamage)
            EndActive(ToolEndReason.Interrupted);
    }

    #endregion
}