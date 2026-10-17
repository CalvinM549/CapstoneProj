using UnityEngine;

// Every kill spawns a parallel process: the drone fires a free shot at the nearest remaining enemy.
// Kills from fork shots can fork again, limited by an internal cooldown.
[CreateAssetMenu(menuName = "Upgrades/Major/Brain/Fork Process")]
public class Brain_ForkProcess : MajorUpgrade
{
    [SerializeField] private float damageMultiplier = 0.75f;
    [SerializeField] private float searchRange = 10f;
    [SerializeField] private float internalCooldown = 0.2f;

    private float nextAllowedTime;

    protected override void OnApply(Player player)
    {
        base.OnApply(player);
        nextAllowedTime = 0f;
        GameEvents.OnEnemyKilled += HandleEnemyKilled;
    }

    protected override void OnRemove(Player player)
    {
        base.OnRemove(player);
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
    }

    private void HandleEnemyKilled(EnemyController killed)
    {
        if (Time.time < nextAllowedTime) return;

        var combat = player.Combat;
        if (!combat.HasWeaponEquipped || AIManager.Instance == null) return;

        Vector2 origin = combat.DroneFireOrigin;
        if (!AIManager.Instance.TryGetNearestEnemy(origin, searchRange, out var target)) return;

        Vector2 direction = ((Vector2)target.position - origin).normalized;
        combat.EquippedWeapon.data.FireBonusShot(combat.EquippedWeapon, direction, origin, damageMultiplier);

        nextAllowedTime = Time.time + internalCooldown;
    }
}
