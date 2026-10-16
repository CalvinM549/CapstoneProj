using System;
using UnityEngine;

public class MetaProgressionService
{
    private GameDatabase db;
    private PlayerProfile profile;

    public PlayerProfile Profile => profile;
    public int Currency => profile.metaCurrency;

    // Fired whenever currency or unlocks change, so open UI can refresh
    public event Action OnChanged;

    public MetaProgressionService(PlayerProfile profile, GameDatabase db)
    {
        this.profile = profile;
        this.db = db;
    }

    #region Queries

    public bool IsUnlocked(IUnlockable item)
    {
        if (item == null) return false;
        if (item.UnlockedByDefault) return true;

        return item switch
        {
            PlayerWeapon w => profile.unlockedWeapons.Contains(w.Id),
            PlayerTool t => profile.unlockedTools.Contains(t.Id),
            _ => false
        };
    }

    public bool IsArchiveUnlocked(ArchiveData entry) => entry != null && profile.unlockedArchives.Contains(entry.Id);

    public bool CanAfford(int cost) => profile.metaCurrency >= cost;

    #endregion

    #region Granting

    public void GrantArchiveUnlock(string id)
    {
        var entry = db.archives.Get(id);
        if (entry == null || profile.unlockedArchives.Contains(entry.Id)) return;

        profile.unlockedArchives.Add(entry.Id);
        OnChanged?.Invoke();
    }

    public void GrantToolUnlock(string id)
    {
        var entry = db.tools.Get(id);
        if (entry == null || profile.unlockedTools.Contains(entry.Id)) return;

        profile.unlockedTools.Add(entry.Id);
        OnChanged?.Invoke();
    }

    public void GrantWeaponUnlock(string id)
    {
        var entry = db.weapons.Get(id);
        if (entry == null || profile.unlockedWeapons.Contains(entry.Id)) return;

        profile.unlockedWeapons.Add(entry.Id);
        OnChanged?.Invoke();
    }

    public void GrantUnlock(IUnlockable item)
    {
        switch (item)
        {
            case PlayerWeapon w: GrantWeaponUnlock(w.Id); break;
            case PlayerTool t: GrantToolUnlock(t.Id); break;
            default: Debug.LogWarning($"[MetaProgressionService] No unlock storage for {item?.GetType().Name}"); break;
        }
    }

    public void GrantCurrency(int amount)
    {
        if (amount == 0) return;

        profile.metaCurrency = Mathf.Max(0, profile.metaCurrency + amount);
        OnChanged?.Invoke();
    }

    #endregion

    #region Purchasing

    // Spends shards on a permanent unlock and saves the profile. Returns false if owned or unaffordable.
    public bool TryPurchase(IUnlockable item)
    {
        if (item == null || IsUnlocked(item)) return false;
        if (!CanAfford(item.UnlockCost)) return false;

        profile.metaCurrency -= item.UnlockCost;
        GrantUnlock(item);

        GameManager.Instance.SaveActiveProfile();
        OnChanged?.Invoke();
        return true;
    }

    #endregion
}
