using System;
using System.Collections.Generic;
using UnityEngine;

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

    public float cooldown;
    public int uses;


    protected Player p;
    // event on use?


    public abstract void UseTool(Vector2 direction);

    public virtual bool CanUse()
    {
        if (uses > 0)
            return true;

        // fire fail event
        return false;
    }

    public virtual bool TryIntercept(HitData incoming) => false;

    public virtual void OnEquip(Player player)
    {
        this.p = player;
    }

    public virtual void OnUnequip()
    {
        p = null;
    }

    public virtual void UpdateTool() { }
}
