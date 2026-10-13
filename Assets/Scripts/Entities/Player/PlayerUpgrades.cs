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

    public Dictionary<UpgradeSlot, MajorUpgrade> majorUpgrades = new();

    private readonly Dictionary<AuxUpgrade, int> auxUpgrades = new();

    [Header("Testing")]

    public AuxUpgrade[] autoGrant;

    #region Monobehaviour Basics

    private void Awake()
    {
        p = GetComponent<Player>();
    }

    private void OnDestroy()
    {

    }

    #endregion

    #region Utilities

    public void Initialize()
    {
        ApplyTemplates();

        if (autoGrant != null && autoGrant.Length > 0)
        {
            foreach (var upgrade in autoGrant)
            {
                GrantAux(upgrade);
            }
        }
    }

    public void RestoreFromSave(PlayerUpgradeSave save, GameDatabase db)
    {
        foreach (var major in save.majorUpgrades)
            GrantMajor((MajorUpgrade)db.upgrades.Get(major));

        foreach (var aux in save.auxUpgrades)
        {
            for (int i = 0; i < aux.stacks; i++)
            {
                GrantAux((AuxUpgrade)db.upgrades.Get(aux.id));
            }
        }
    }

    public PlayerUpgradeSave ToSave()
    {
        PlayerUpgradeSave save = new()
        {
            majorUpgrades = majorUpgrades.Values.Select(u => u.Id).ToList()
        };

        foreach (var upgrade in auxUpgrades)
        {
            save.auxUpgrades.Add(new AuxUpgradeSave()
            {
                id = upgrade.Key.Id,
                stacks = upgrade.Value
            });
        }

        return save;
    }

    public MajorUpgrade GetMajor(UpgradeSlot slot) => 
        majorUpgrades.TryGetValue(slot, out var upgrade) ? upgrade : null;

    public int GetAuxStacks(AuxUpgrade upgrade) => 
        auxUpgrades.TryGetValue(upgrade, out var amount) ? amount : 0;

    public IEnumerable<UpgradeBase> AllActiveUpgrades =>
        majorUpgrades.Values
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

        if (majorUpgrades.TryGetValue(upgrade.slot, out MajorUpgrade existing) && existing != null)
            RemoveMajorInternal(existing);

        majorUpgrades[upgrade.slot] = upgrade;
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

        auxUpgrades[upgrade] = current + 1;

        if (current == 0)
            upgrade.Apply(p);
        else
            upgrade.OnStack();

        return true;
    }

    #endregion
    #region Removing Upgrades

    public void RemoveMajor(UpgradeSlot slot)
    {
        if(majorUpgrades.TryGetValue(slot, out MajorUpgrade upgrade) && upgrade != null)
            RemoveMajorInternal(upgrade);
    }

    private void RemoveMajorInternal(MajorUpgrade upgrade)
    {
        upgrade.Remove(p);
        majorUpgrades[upgrade.slot] = null;

        // Fire event
    }

    public void RemoveAllAuxStacks(AuxUpgrade upgrade)
    {
        if (!auxUpgrades.ContainsKey(upgrade)) return;

        upgrade.Remove(p);
        auxUpgrades.Remove(upgrade);
        // Fire event
    }

    public void RemoveAll()
    {
        foreach(var major in majorUpgrades.Values.Where(m => m.slot != UpgradeSlot.None))
            major.Remove(p);

        foreach(var aux in auxUpgrades.Keys.ToList())
            aux.Remove(p);

        majorUpgrades.Clear();
        auxUpgrades.Clear();
    }

    #endregion


    private void ApplyTemplates()
    {
        if (FrameTemplate != null)      GrantMajor(FrameTemplate);
        if (WeaponTemplate != null)     GrantMajor(WeaponTemplate);
        if (PropulsionTemplate != null) GrantMajor(PropulsionTemplate);
        if (BrainTemplate != null)      GrantMajor(BrainTemplate);
    }

    public void ModifyOutgoingHit(HitData hit)
    {
        foreach (var upgrade in AllActiveUpgrades)
        {
            upgrade.ModifyOutgoingHit(hit);
        }
    }

    public void ModifyIncomingHit(HitData hit)
    {
        foreach (var upgrade in AllActiveUpgrades)
        {
            upgrade.ModifyIncomingHit(hit);
        }
    }
}
