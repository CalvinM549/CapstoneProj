using System.Collections;
using UnityEngine;

// A second linked barrel: every weapon shot is echoed by a weaker free shot a moment later.
[CreateAssetMenu(menuName = "Upgrades/Major/Weapon/Twin-Link Protocol")]
public class Weapon_TwinLink : MajorUpgrade
{
    [SerializeField] private float echoDelay = 0.15f;
    [SerializeField] private float echoDamageMultiplier = 0.4f;

    protected override void OnApply(Player player)
    {
        base.OnApply(player);
        GameEvents.OnPlayerWeaponFired += HandleWeaponFired;
    }

    protected override void OnRemove(Player player)
    {
        base.OnRemove(player);
        GameEvents.OnPlayerWeaponFired -= HandleWeaponFired;
    }

    private void HandleWeaponFired(ActiveWeapon weapon, Vector2 direction, Vector2 origin)
    {
        if (player != null) player.StartCoroutine(EchoRoutine(weapon, direction));
    }

    private IEnumerator EchoRoutine(ActiveWeapon weapon, Vector2 direction)
    {
        yield return new WaitForSeconds(echoDelay);

        // Upgrade removed or weapon swapped in the meantime
        if (player == null || player.Combat.EquippedWeapon != weapon) yield break;

        weapon.data.FireBonusShot(weapon, direction, player.Combat.DroneFireOrigin, echoDamageMultiplier);
    }
}
