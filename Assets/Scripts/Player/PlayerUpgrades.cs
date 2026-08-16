using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    private Player p;

    [SerializeField] private MajorUpgrade FrameTemplate;
    [SerializeField] private MajorUpgrade WeaponTemplate;
    [SerializeField] private MajorUpgrade PropulsionTemplate;
    [SerializeField] private MajorUpgrade BrainTemplate;

    public Dictionary<UpgradeSlot, MajorUpgrade> majorSlots = new();

    private readonly Dictionary<AuxUpgrade, int> auxUpgrades = new();

    #region Monobehaviour Basics

    private void Awake()
    {
        p = GetComponent<Player>();
    }

    private void Start()
    {
        ApplyTemplates();
    }

    #endregion

    #region Utilities

    public void RestoreFromSave(PlayerUpgradeSave save)
    {

    }

    public MajorUpgrade GetMajor(UpgradeSlot slot) => 
        majorSlots.TryGetValue(slot, out var upgrade) ? upgrade : null;

    public int GetAuxStacks(AuxUpgrade upgrade) => 
        auxUpgrades.TryGetValue(upgrade, out var amount) ? amount : 0;

    public IEnumerable<UpgradeBase> AllActiveUpgrades =>
        majorSlots.Values
            .Where(m => m != null && !m.isTemplate)
            .Cast<UpgradeBase>()
            .Concat(auxUpgrades.Keys);

    #endregion


    #region Granting Upgrades

    public bool GrantMajor(MajorUpgrade upgrade)
    {
        if (upgrade.slot == UpgradeSlot.None)
        {
            Debug.LogWarning($"[PlayerUpgrades] MajorUpgrade '{upgrade.upgradeName}' has slot=None");
            return false;
        }

        if (majorSlots.TryGetValue(upgrade.slot, out MajorUpgrade existing) && existing != null)
            RemoveMajorInternal(existing);

        majorSlots[upgrade.slot] = upgrade;
        upgrade.Apply(p);

        // Fire event
        return true;
    }

    public bool GrantAux(AuxUpgrade upgrade)
    {
        int current = GetAuxStacks(upgrade);
        if (current >= upgrade.maxStacks)
        {
            Debug.Log($"[PlayerUpgrades] AuxUpgrade '{upgrade.upgradeName}' is at max stacks");
            return false;
        }

        auxUpgrades[upgrade] = current++;

        upgrade.Apply(p);
        // Fire Event
        return true;
    }

    #endregion
    #region Removing Upgrades

    public void RemoveMajor(UpgradeSlot slot)
    {
        if(majorSlots.TryGetValue(slot, out MajorUpgrade upgrade) && upgrade != null)
            RemoveMajorInternal(upgrade);
    }

    private void RemoveMajorInternal(MajorUpgrade upgrade)
    {
        upgrade.Remove(p);
        majorSlots[upgrade.slot] = null;

        // Fire event
    }

    public void RemoveAllAuxStacks(AuxUpgrade upgrade)
    {
        if (!auxUpgrades.ContainsKey(upgrade)) return;

        int stacks = auxUpgrades[upgrade];
        for (int i = 0; i < stacks; i++)
            upgrade.Remove(p);

        auxUpgrades.Remove(upgrade);
        // Fire event
    }

    #endregion


    private void ApplyTemplates()
    {
        if (FrameTemplate != null)      GrantMajor(FrameTemplate);
        if (WeaponTemplate != null)     GrantMajor(WeaponTemplate);
        if (PropulsionTemplate != null) GrantMajor(PropulsionTemplate);
        if (BrainTemplate != null)      GrantMajor(BrainTemplate);
    }
}
