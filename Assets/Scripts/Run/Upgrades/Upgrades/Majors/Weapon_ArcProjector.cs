using System.Collections.Generic;
using UnityEngine;

// Ranged hits on a Shocked enemy arc the charge to the nearest other enemy.
[CreateAssetMenu(menuName = "Upgrades/Major/Weapon/Arc Projector")]
public class Weapon_ArcProjector : MajorUpgrade
{
    [SerializeField] private StatusEffectData shockStatus;
    [SerializeField] private float arcRange = 5f;
    [SerializeField] private int stacksArced = 1;

    private readonly List<EnemyController> buffer = new();

    public override void ModifyHitOnTarget(HitData hit, EnemyController target)
    {
        if (hit.attackType != AttackType.Projectile || shockStatus == null) return;
        if (target == null || target.Statuses == null || !target.Statuses.HasEffect(shockStatus)) return;
        if (AIManager.Instance == null) return;

        Vector2 from = target.transform.position;
        AIManager.Instance.GetEnemiesInRadius(from, arcRange, buffer);

        EnemyController nearest = null;
        float best = float.MaxValue;
        foreach (var enemy in buffer)
        {
            if (enemy == target) continue;
            float d = ((Vector2)enemy.transform.position - from).sqrMagnitude;
            if (d < best) { best = d; nearest = enemy; }
        }

        if (nearest == null || nearest.Statuses == null) return;

        nearest.Statuses.ApplyEffects(shockStatus, this, true, stacksArced);
        if (VFXManager.Instance != null) VFXManager.Instance.PlayVFX(VFXType.LightningStrike, nearest.transform.position);
    }
}
