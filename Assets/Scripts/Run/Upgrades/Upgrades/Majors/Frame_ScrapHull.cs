using UnityEngine;

// Collected scrap is fed straight into the hull: each currency pickup repairs part of the active segment.
[CreateAssetMenu(menuName = "Upgrades/Major/Frame/Scrap Hull")]
public class Frame_ScrapHull : MajorUpgrade
{
    [Tooltip("Fraction of the active segment's max integrity restored per pickup.")]
    [SerializeField] [Range(0f, 1f)] private float repairPerPickup = 0.05f;

    protected override void OnApply(Player player)
    {
        base.OnApply(player);
        GameEvents.OnCurrencyCollected += HandleCurrencyCollected;
    }

    protected override void OnRemove(Player player)
    {
        base.OnRemove(player);
        GameEvents.OnCurrencyCollected -= HandleCurrencyCollected;
    }

    private void HandleCurrencyCollected(int amount)
    {
        if (player != null) player.Health.RestoreActiveSegmentFraction(repairPerPickup);
    }
}
