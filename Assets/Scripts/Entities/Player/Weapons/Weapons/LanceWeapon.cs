using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Weapon/Lance")]
public class LanceWeapon : PlayerWeapon
{
    [SerializeField] private float beamLength;
    [SerializeField] private float beamWidth = 0.4f;

    [SerializeField] private LaserBeamVisual visualPrefab;

    public override void Fire(ActiveWeapon instance, Vector2 direction, Vector2 firePoint)
    {
        // Fire effects
        VFXManager.Instance.PlayVFX(VFXType.ShootIntense, firePoint, direction);
        TimescaleManager.Instance.RequestHitStop(0.2f);

        HitData hitData = BuildOutgoingHit(instance, projectiles[0], direction, firePoint);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Vector2 boxSize = new(beamWidth, beamWidth);

        LayerMask combinedMask = GameManager.WallLayer | GameManager.EnemyLayer;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(firePoint, boxSize, angle, direction, beamLength, combinedMask);

        float visualLength = beamLength;

        var alreadyHit = new HashSet<IDamageable>();

        foreach (var hit in hits)
        {
            bool isWall = ((1 << hit.collider.gameObject.layer) & GameManager.WallLayer) != 0;
            if (isWall && hit.distance > 0.5f)
            {
                visualLength = hit.distance;
                break;
            }

            if (!hit.collider.TryGetComponent<IDamageable>(out var target)) continue;
            if(!alreadyHit.Add(target)) continue;

            target.RecieveHit(hitData.Clone());
        }

        var endPoint = firePoint + direction * visualLength;

        SpawnVisual(firePoint, endPoint);

        VFXManager.Instance.PlayVFX(VFXType.ShootIntense, endPoint, -direction);

        instance.SpendAmmo();
    }

    private void SpawnVisual(Vector2 start, Vector2 end)
    {
        if (visualPrefab == null) return;

        LaserBeamVisual visual = Instantiate(visualPrefab);
        visual.Play(start, end);
    }
}
