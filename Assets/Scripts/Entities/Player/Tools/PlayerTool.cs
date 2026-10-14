using System;
using System.Collections.Generic;
using UnityEngine;

public enum ToolActivation
{
    Instant,
    Window,
    Channel
}

public enum ToolEndReason
{
    Expired,
    Released,
    Triggered,
    Interrupted,
    Cancelled
}

public class ActiveTool
{
    public readonly PlayerTool data;
    public readonly Player player;

    public int charges;
    public float rechargeTimer;
    public float cooldownTimer;

    public bool isActive;
    public float activeTime;
    public Vector2 direction;

    public int counter;
    public GameObject spawned;


    public event Action Changed;
    public void NotifyChanged() => Changed?.Invoke();


    public ActiveTool(PlayerTool data, Player player, int chargesOverride = -1)
    {
        this.data = data;
        this.player = player;

        charges = chargesOverride > 0 ? Mathf.Min(chargesOverride, data.maxCharges) : data.maxCharges;
        if (charges < data.maxCharges) rechargeTimer = data.rechargeTime;
    }
}

public abstract class PlayerTool : DatabaseEntry, ILootEntry
{
    [Header("Display")]
    public string toolName = "New Tool";
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


    public bool spawnInShops = true;
    public int shopPrice = 50;
    public bool Buyable => spawnInShops;
    public int ShopPrice => shopPrice;

    public float BaseDropWeight => baseDropWeight;
    public IReadOnlyList<string> SynergyTags => synergyTags;

    [Header("Effect")]

    public ToolActivation activation = ToolActivation.Instant;
    public float duration;
    public bool canEndEarly;
    public bool lockMovement;
    public bool interruptOnDamage = true;

    public int maxCharges;
    public float rechargeTime;
    public float cooldown;

    public float momentumCost;

    public virtual bool CanUse(ActiveTool instance)
    {
        return instance.charges > 0
            && instance.cooldownTimer <= 0f
            && instance.player.Momentum.PercentMomentum >= momentumCost;
    }

    public virtual void OnEquip(ActiveTool instance) { }
    public virtual void OnUnequip(ActiveTool instance) { }
    public virtual void OnUse(ActiveTool instance) { }
    public virtual void OnTick(ActiveTool instance, float dt) { }
    public virtual void OnEnd(ActiveTool instance, ToolEndReason reason) { }
    public virtual bool TryIntercept(ActiveTool instance, HitData incoming) => false;


}
