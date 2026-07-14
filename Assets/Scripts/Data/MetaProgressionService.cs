using UnityEngine;

public class MetaProgressionService
{
    private readonly PlayerProfile profile;

    public MetaProgressionService(PlayerProfile profile)
    {
        this.profile = profile;
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
