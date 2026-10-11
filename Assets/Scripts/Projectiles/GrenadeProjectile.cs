using System.Collections.Generic;
using UnityEngine;

public class GrenadeProjectile : Projectile
{
    [SerializeField] private float explosionRadius;

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        //
    }

    protected override void OnExpire()
    {

        base.OnExpire();
    }

    private void DealAreaDamage()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, GameManager.EnemyLayer);
        var alreadyHit = new HashSet<IDamageable>();

        foreach (var col in hits)
        {
            if (!col.TryGetComponent<IDamageable>(out var target)) continue;
            if (!alreadyHit.Add(target)) continue;

            target.RecieveHit(hitData.Clone());
        }

    }
}
