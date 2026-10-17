using System.Collections.Generic;
using UnityEngine;

// When a structure segment breaks, the plating detonates outward: damages, knocks back and staggers nearby enemies.
[CreateAssetMenu(menuName = "Upgrades/Major/Frame/Reactive Armour")]
public class Frame_ReactiveArmour : MajorUpgrade
{
    [SerializeField] private float radius = 3.5f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float knockback = 18f;
    [SerializeField] private float hitstun = 0.5f;
    [Tooltip("Brief invulnerability after the blast so the player can reposition.")]
    [SerializeField] private float iFrames = 0.4f;
    [SerializeField] private float cameraShake = 0.6f;

    private readonly List<EnemyController> buffer = new();

    protected override void OnApply(Player player)
    {
        base.OnApply(player);
        GameEvents.OnPlayerSegmentBroken += HandleSegmentBroken;
    }

    protected override void OnRemove(Player player)
    {
        base.OnRemove(player);
        GameEvents.OnPlayerSegmentBroken -= HandleSegmentBroken;
    }

    private void HandleSegmentBroken()
    {
        if (player == null || AIManager.Instance == null) return;

        Vector2 center = player.transform.position;
        AIManager.Instance.GetEnemiesInRadius(center, radius, buffer);

        foreach (var enemy in buffer)
        {
            Vector2 dir = (Vector2)enemy.transform.position - center;
            dir = dir.sqrMagnitude > 0.001f ? dir.normalized : Random.insideUnitCircle.normalized;
            enemy.RecieveHit(BuildPlayerMeleeHit(damage, AttackType.Heavy, dir, knockback, hitstun));
        }

        player.Health.GrantIFrames(iFrames);

        if (VFXManager.Instance != null) VFXManager.Instance.PlayVFX(VFXType.ExplosionSimple, center);
        if (CameraManager.Instance != null) CameraManager.Instance.CameraShake(cameraShake);
    }
}
