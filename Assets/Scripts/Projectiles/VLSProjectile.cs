using UnityEngine;

public class VLSProjectile : Projectile
{
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private float warningDelay = 1f;
    [SerializeField] private float fallDuration = 0.6f;

    private Vector2 strikePosition;
    private MissileStrike strikePrefab;


    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        //
    }

    public void InitializeAsMissile(Vector2 target, MissileStrike strikePrefab)
    {
        this.strikePosition = target;
        this.strikePrefab = strikePrefab;
    }

    protected override void OnExpire()
    {
        SpawnStrike();
        ReturnToPool();
    }

    private void SpawnStrike()
    {
        if (strikePrefab == null) return;

        MissileStrike strike = Instantiate(strikePrefab, strikePosition, Quaternion.identity);
        strike.Initialize(strikePosition, hitData, explosionRadius, warningDelay, fallDuration);

        strikePrefab = null;
    }

}
