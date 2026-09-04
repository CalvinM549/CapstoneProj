using UnityEngine;

public class MetaProgressionService
{
    private GameDatabase db;
    private PlayerProfile profile;

    public MetaProgressionService(PlayerProfile profile, GameDatabase db)
    {
        this.profile = profile;
        this.db = db;
    }

    public void GrantArchiveUnlock(string id)
    {
        var entry = db.archives.Get(id);
        if (profile.unlockedArchives.Contains(entry.Id)) return;

        profile.unlockedArchives.Add(entry.Id);
    }

    public void GrantToolUnlock(string id)
    {
        var entry = db.tools.Get(id);
        if (profile.unlockedTools.Contains(entry.Id)) return;

        profile.unlockedTools.Add(entry.Id);

    }

    public void GrantWeaponUnlock(string id)
    {
        var entry = db.weapons.Get(id);
        if (profile.unlockedWeapons.Contains(entry.Id)) return;

        profile.unlockedWeapons.Add(entry.Id);
    }

    public void GrantCurrency(int amount)
    {
        profile.metaCurrency += amount;
        // Fire Event for UI
    }

    public bool TryUnlockWithCurrency(string unlockID, int cost)
    {
        if (profile.metaCurrency < cost) return false;

        return true;
    }
}
