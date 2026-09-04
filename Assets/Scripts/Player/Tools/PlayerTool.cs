using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerTool : DatabaseEntry, IRollableLoot
{
    [Header("Display")]
    public string toolName;
    [TextArea] public string flavourText;
    [TextArea] public string effectText;
    public Sprite icon;

    public string Name => toolName;
    public string FlavourDescription => flavourText;
    public string EffectDescription => effectText;
    public Sprite Icon => icon;

    [Header("Loot")]
    public float baseDropWeight = 1f;
    public string[] synergyTags = Array.Empty<string>();

    public float BaseDropWeight => baseDropWeight;
    public IReadOnlyList<string> SynergyTags => synergyTags;

    [Header("Effect")]

    public float cooldown;
    private float cooldownTimer;

    protected Player player;

    public bool IsReady => cooldownTimer <= 0f;
    public float CooldownPercent => Mathf.Clamp01(cooldownTimer / cooldown);



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

    protected void StartCooldown() => cooldownTimer = cooldown;
    protected void ReduceCooldown(float amount) => cooldownTimer = Mathf.Max(0f, cooldownTimer - amount);
}
