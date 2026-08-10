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
    public Dictionary<MajorUpgrade, List<SubUpgrade>> subUpgrades = new();

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

    public IReadOnlyList<SubUpgrade> GetSubUpgrades(MajorUpgrade major) => 
        subUpgrades.TryGetValue(major, out var list) ? list : Array.Empty<SubUpgrade>();

    public int GetAuxStacks(AuxUpgrade upgrade) => 
        auxUpgrades.TryGetValue(upgrade, out var amount) ? amount : 0;

    public IEnumerable<UpgradeBase> AllActiveUpgrades =>
        majorSlots.Values
            .Where(m => m != null && !m.isTemplate)
            .Cast<UpgradeBase>()
            .Concat(subUpgrades.Values.SelectMany(l => l))
            .Concat(auxUpgrades.Keys);

    public IEnumerable<SubUpgrade> AvaliableSubUpgrades
    {
        get
        {
            foreach (MajorUpgrade major in majorSlots.Values)
            {
                if (major == null || major.isTemplate) continue;
                foreach (SubUpgrade sub in major.subUpgradesUnlocked)
                {
                    if (!subUpgrades[major].Contains(sub))
                        yield return sub;
                }
            }
        }
    }

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

    public bool GrantSub(SubUpgrade upgrade)
    {
        MajorUpgrade activeMajor = GetMajor(upgrade.requiredMajor?.slot ?? UpgradeSlot.None);
        if (activeMajor != upgrade.requiredMajor)
        {
            Debug.LogWarning($"[PlayerUpgrades] SubUpgrade '{upgrade.upgradeName}' requires {upgrade.requiredMajor.upgradeName} which isnt active");
            return false;
        }

        if (subUpgrades[upgrade.requiredMajor].Contains(upgrade))
        {
            Debug.LogWarning($"[PlayerUpgrades] SubUpgrade '{upgrade.upgradeName}' has already been chosen");
            return false;
        }


        if (!subUpgrades.TryGetValue(upgrade.requiredMajor, out List<SubUpgrade> list))
        {
            list = new List<SubUpgrade>();
            subUpgrades[upgrade.requiredMajor] = list;
        }

        list.Add(upgrade);

        upgrade.Apply(p);
        // Fire Event
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
        if (subUpgrades.TryGetValue(upgrade, out List<SubUpgrade> subs))
        {
            foreach (SubUpgrade sub in subs)
                sub.Remove(p);
            subUpgrades.Remove(upgrade);
        }

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
