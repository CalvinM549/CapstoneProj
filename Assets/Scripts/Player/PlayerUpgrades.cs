using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    public List<Upgrade> upgrades;

    public Dictionary<UpgradeSlot, Upgrade> slottedUpgrades;

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {

    }

    private void GrantUpgrade(string upgrade)
    {
        // Find upgrade as Upgrade type throguh database prolly

        // GrantUpgrade(finalUpgrade)
    }

    private void GrantUpgrade(Upgrade upgrade)
    {
        if (upgrade.slot != UpgradeSlot.None)
        {
            if (slottedUpgrades[upgrade.slot] != null)
                RemoveUpgrade(slottedUpgrades[upgrade.slot]);

            slottedUpgrades[upgrade.slot] = upgrade;
        }

        upgrade.ApplyUpgrade();
        upgrades.Add(upgrade);
    }

    private void RemoveUpgrade(Upgrade upgrade)
    {
        if (!upgrades.Contains(upgrade)) return;

        upgrade.RemoveUpgrade();
        upgrades.Remove(upgrade);
    }
}
