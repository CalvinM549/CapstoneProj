using UnityEngine;

// Safety limiter removed: dashes recharge much faster (asset stat), but every dash burns momentum.
[CreateAssetMenu(menuName = "Upgrades/Major/Propulsion/Bypassed Limiter")]
public class Propulsion_BypassedLimiter : MajorUpgrade
{
    [SerializeField] [Range(0f, 1f)] private float momentumCostPercent = 0.08f;

    protected override void OnApply(Player player)
    {
        base.OnApply(player);
        GameEvents.OnPlayerDashStart += HandleDashStart;
    }

    protected override void OnRemove(Player player)
    {
        base.OnRemove(player);
        GameEvents.OnPlayerDashStart -= HandleDashStart;
    }

    private void HandleDashStart()
    {
        if (player != null) player.Momentum.SpendMomentumPercent(momentumCostPercent);
    }
}
