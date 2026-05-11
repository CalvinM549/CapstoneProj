using UnityEngine;

public class MissileProjectile : Projectile
{
    private Vector2 rainTargetPosition;
    private GameObject telegraphPrefab;

    public void InitializeAsMissile(Vector2 rainTarget, GameObject telegraphPrefab)
    {
        this.rainTargetPosition = rainTarget;
        this.telegraphPrefab = telegraphPrefab;
    }

    protected override void OnExpire()
    {
        if (telegraphPrefab != null)
        {
            var telegraph = Instantiate(telegraphPrefab, rainTargetPosition, Quaternion.identity);

            telegraph.GetComponent<MissileRainTelegraph>()?.Initialize(rainTargetPosition);
        }

        ReturnToPool();
    }

    protected override void HandleHit(IDamageable target)
    {
        return;
    }

}
