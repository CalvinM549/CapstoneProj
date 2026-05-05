using UnityEngine;

public abstract class PlayerTool : ScriptableObject
{
    public string ToolName;
    public string ToolDescription;
    public Sprite Icon;

    public float Cooldown;
    private float cooldownTimer;

    protected Player player;

    public bool IsReady => cooldownTimer <= 0f;
    public float CooldownPercent => Mathf.Clamp01(cooldownTimer / Cooldown);

    public bool UseTool(Vector2 direction)
    {
        if (!IsReady) return false;

        bool used = OnUse(direction);

        if (used)
        {
            OnUsed();
            StartCooldown();
        }

        return used;
    }

    protected abstract bool OnUse(Vector2 direction);


    public virtual bool TryIntercept(HitData incoming) => false;

    public virtual void OnEquip(Player player)
    {
        this.player = player;
    }

    public virtual void OnUnequip()
    {
        cooldownTimer = 0f;
        OnReset();
    }

    public virtual void UpdateTool()
    {
        if(cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        OnUpdate();
    }

    protected virtual void OnUpdate() { }
    protected virtual void OnReset() { }
    protected virtual void OnUsed() { }

    protected void StartCooldown() => cooldownTimer = Cooldown;
    protected void ReduceCooldown(float amount) => cooldownTimer = Mathf.Max(0f, cooldownTimer - amount);
}
