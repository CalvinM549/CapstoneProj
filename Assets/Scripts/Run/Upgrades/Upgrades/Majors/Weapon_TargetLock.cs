using UnityEngine;

// Lethality rework: big damage bonus, but only against the enemy you are locked on to.
[CreateAssetMenu(menuName = "Upgrades/Major/Weapon/Target Lock")]
public class Weapon_TargetLock : MajorUpgrade
{
    [SerializeField] private float lockedDamageBonus = 0.5f;

    public override void ModifyHitOnTarget(HitData hit, EnemyController target)
    {
        if (target != null && target.hasPlayerLock)
            hit.AddModifier(lockedDamageBonus, StatModType.PercentAdd, this);
    }
}
