using UnityEngine;

// A fragment of the old Overseer network: reaching High momentum briefly slows time and refunds a dash.
[CreateAssetMenu(menuName = "Upgrades/Major/Brain/Overseer Fragment")]
public class Brain_OverseerFragment : MajorUpgrade
{
    [SerializeField] private float slowDuration = 0.35f;
    [SerializeField] [Range(0.05f, 1f)] private float slowTimeScale = 0.3f;
    [SerializeField] private bool refundDashCharge = true;
    [Tooltip("Real seconds before it can trigger again, so bouncing around the threshold doesn't spam it.")]
    [SerializeField] private float cooldown = 6f;

    private float nextAllowedTime;

    protected override void OnApply(Player player)
    {
        base.OnApply(player);
        nextAllowedTime = 0f;
        GameEvents.OnEnterHighMomentum += HandleEnterHighMomentum;
    }

    protected override void OnRemove(Player player)
    {
        base.OnRemove(player);
        GameEvents.OnEnterHighMomentum -= HandleEnterHighMomentum;
    }

    private void HandleEnterHighMomentum()
    {
        if (Time.unscaledTime < nextAllowedTime) return;
        nextAllowedTime = Time.unscaledTime + cooldown;

        if (TimescaleManager.Instance != null)
            TimescaleManager.Instance.RequestSlowMotion(slowDuration, slowTimeScale);

        if (refundDashCharge)
            player.Movement.RestoreDashCharge();

        if (ScreenGlitch.Instance != null)
            ScreenGlitch.Instance.Pulse(slowDuration, 0.25f);
    }
}
