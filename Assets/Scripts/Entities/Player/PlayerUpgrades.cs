using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Upgrades are granted as runtime copies (Instantiate) of their assets, so any state an upgrade keeps
// (player ref, stacks, timers, event subscriptions) lives on the copy and is thrown away with the run.
// Lookups are by asset Id, so callers can keep passing the original asset.
public class PlayerUpgrades : MonoBehaviour
{
    private Player p;

    [SerializeField] private MajorUpgrade FrameTemplate;
    [SerializeField] private MajorUpgrade WeaponTemplate;
    [SerializeField] private MajorUpgrade PropulsionTemplate;
    [SerializeField] private MajorUpgrade BrainTemplate;

    private readonly Dictionary<UpgradeSlot, MajorUpgrade> majorUpgrades = new();

    private readonly Dictionary<string, AuxUpgrade> auxInstances = new();
    private readonly Dictionary<string, int> auxStacks = new();

    public event Action OnUpgradesChanged;

    [Header("Testing")]

    public AuxUpgrade[] autoGrant;

    #region Monobehaviour Basics

    private void Awake()
    {
        p = GetComponent<Player>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerHitLanding += ModifyHitOnTarget;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerHitLanding -= ModifyHitOnTarget;
    }

    private void OnDestroy()
    {
        RemoveAll();
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
        foreach (var majorId in save.majorUpgrades)
        {
            if (db.upgrades.Get(majorId) is MajorUpgrade major) GrantMajor(major);
            else Debug.LogWarning($"[PlayerUpgrades] Saved major '{majorId}' no longer exists");
        }

        foreach (var aux in save.auxUpgrades)
        {
            if (db.upgrades.Get(aux.id) is not AuxUpgrade auxAsset)
            {
                Debug.LogWarning($"[PlayerUpgrades] Saved aux '{aux.id}' no longer exists");
                continue;
            }

            for (int i = 0; i < aux.stacks; i++)
                GrantAux(auxAsset);
        }
    }

    public PlayerUpgradeSave ToSave()
    {
        PlayerUpgradeSave save = new()
        {
            majorUpgrades = majorUpgrades.Values.Where(u => u != null).Select(u => u.Id).ToList()
        };

        foreach (var pair in auxStacks)
        {
            save.auxUpgrades.Add(new AuxUpgradeSave()
            {
                id = pair.Key,
                stacks = pair.Value
            });
        }

        return save;
    }

    public MajorUpgrade GetMajor(UpgradeSlot slot) =>
        majorUpgrades.TryGetValue(slot, out var upgrade) ? upgrade : null;

    public int GetAuxStacks(AuxUpgrade upgrade) =>
        upgrade != null && auxStacks.TryGetValue(upgrade.Id, out var amount) ? amount : 0;

    public IEnumerable<UpgradeBase> AllActiveUpgrades =>
        majorUpgrades.Values
            .Where(m => m != null && !m.isTemplate)
            .Cast<UpgradeBase>()
            .Concat(auxInstances.Values);

    #endregion


    #region Granting Upgrades

    public bool GrantMajor(MajorUpgrade upgrade)
    {
        if (upgrade == null) return false;

        if (upgrade.slot == UpgradeSlot.None)
        {
            Debug.LogWarning($"[PlayerUpgrades] MajorUpgrade '{upgrade.upgradeName}' has slot=None");
            return false;
        }

        if (majorUpgrades.TryGetValue(upgrade.slot, out MajorUpgrade existing) && existing != null)
            RemoveMajorInternal(existing);

        var instance = Instantiate(upgrade);
        instance.name = upgrade.name;

        majorUpgrades[upgrade.slot] = instance;
        instance.Apply(p);

        OnUpgradesChanged?.Invoke();
        return true;
    }

    public bool GrantAux(AuxUpgrade upgrade)
    {
        if (upgrade == null) return false;

        int current = GetAuxStacks(upgrade);
        if (current >= upgrade.maxStacks)
        {
            Debug.Log($"[PlayerUpgrades] AuxUpgrade '{upgrade.upgradeName}' is at max stacks");
            return false;
        }

        auxStacks[upgrade.Id] = current + 1;

        if (current == 0)
        {
            var instance = Instantiate(upgrade);
            instance.name = upgrade.name;
            auxInstances[upgrade.Id] = instance;
            instance.Apply(p);
        }
        else
            auxInstances[upgrade.Id].OnStack();

        OnUpgradesChanged?.Invoke();
        return true;
    }

    #endregion
    #region Removing Upgrades

    public void RemoveMajor(UpgradeSlot slot)
    {
        if(majorUpgrades.TryGetValue(slot, out MajorUpgrade upgrade) && upgrade != null)
            RemoveMajorInternal(upgrade);

        OnUpgradesChanged?.Invoke();
    }

    private void RemoveMajorInternal(MajorUpgrade instance)
    {
        instance.Remove(p);
        majorUpgrades.Remove(instance.slot);
        Destroy(instance);
    }

    public void RemoveAllAuxStacks(AuxUpgrade upgrade)
    {
        if (upgrade == null || !auxInstances.TryGetValue(upgrade.Id, out var instance)) return;

        instance.Remove(p);
        auxInstances.Remove(upgrade.Id);
        auxStacks.Remove(upgrade.Id);
        Destroy(instance);

        OnUpgradesChanged?.Invoke();
    }

    public void RemoveAll()
    {
        foreach (var major in majorUpgrades.Values.Where(m => m != null).ToList())
        {
            major.Remove(p);
            Destroy(major);
        }

        foreach (var aux in auxInstances.Values.ToList())
        {
            aux.Remove(p);
            Destroy(aux);
        }

        majorUpgrades.Clear();
        auxInstances.Clear();
        auxStacks.Clear();
    }

    #endregion


    private void ApplyTemplates()
    {
        if (FrameTemplate != null)      GrantMajor(FrameTemplate);
        if (WeaponTemplate != null)     GrantMajor(WeaponTemplate);
        if (PropulsionTemplate != null) GrantMajor(PropulsionTemplate);
        if (BrainTemplate != null)      GrantMajor(BrainTemplate);
    }

    #region Hit Hooks

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

    // Called as a player hit lands on an enemy (melee or projectile), for target-dependent effects
    private void ModifyHitOnTarget(HitData hit, EnemyController target)
    {
        if (hit.IsResolved) return; // already applied to an earlier target (AOE etc)

        foreach (var upgrade in AllActiveUpgrades)
            upgrade.ModifyHitOnTarget(hit, target);
    }

    public bool PreventsWhiffDrain(AttackType type) => AllActiveUpgrades.Any(u => u.PreventsWhiffDrain(type));

    #endregion
}
