using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    private Player p;


    public Dictionary<UpgradeSlot, Upgrade> slottedUpgrades;
    public Dictionary<Upgrade, int> unslottedUpgrades;

    public IEnumerable<Upgrade> ActiveUpgrades => slottedUpgrades.Values.Concat(unslottedUpgrades.Keys);

    private void Awake()
    {
        p = GetComponent<Player>();
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {

    }

    public bool TryGrantUpgrade(Upgrade upgrade)
    {
        return upgrade.slot == UpgradeSlot.None 
            ? TryGrantSlotted(upgrade) 
            : TryGrantAux(upgrade);
    }

    private bool TryGrantSlotted(Upgrade upgrade)
    {
        if (slottedUpgrades.TryGetValue(upgrade.slot, out var existing))
        {
            existing.RemoveUpgrade();
            // Ping event
        }

        slottedUpgrades[upgrade.slot] = upgrade;
        upgrade.ApplyUpgrade();

        //Ping Event
        return true;
    }

    private bool TryGrantAux(Upgrade upgrade)
    {
        int current = unslottedUpgrades.TryGetValue(upgrade, out int c) ? c : 0;

        if (current >= upgrade.maxStacks)
        {
            return false;
        }

        unslottedUpgrades[upgrade] = current + 1;
        upgrade.ApplyUpgrade();

        // Ping Event
        return true;
    }
}
