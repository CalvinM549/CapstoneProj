using UnityEngine;

// Corrupted logic: hits much harder while running on the last structure segment.
// The "-1 max segment" cost lives in the asset's stat modifiers.
[CreateAssetMenu(menuName = "Upgrades/Major/Brain/Corrupted Directive")]
public class Brain_CorruptedDirective : MajorUpgrade
{
    [SerializeField] private float lastSegmentDamageBonus = 0.4f;

    public override void ModifyOutgoingHit(HitData hit)
    {
        if (player != null && player.Health.ActiveHealthIndex == 0)
            hit.AddModifier(lastSegmentDamageBonus, StatModType.PercentAdd, this);
    }
}
