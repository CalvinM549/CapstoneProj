using UnityEngine;

// Melee kills are stripped for parts: guaranteed ammo on every melee kill. Bridges melee and ranged builds.
[CreateAssetMenu(menuName = "Upgrades/Major/Weapon/Kill-Feed Loader")]
public class Weapon_KillFeedLoader : MajorUpgrade
{
    [SerializeField] private int ammoPerKill = 1;

    protected override void OnApply(Player player)
    {
        base.OnApply(player);
        GameEvents.OnEnemyKilled += HandleEnemyKilled;
    }

    protected override void OnRemove(Player player)
    {
        base.OnRemove(player);
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
    }

    private void HandleEnemyKilled(EnemyController enemy)
    {
        if (player == null) return;
        if (enemy.LastHitType is AttackType.Light or AttackType.Heavy)
            player.Combat.RestoreAmmo(ammoPerKill);
    }
}
