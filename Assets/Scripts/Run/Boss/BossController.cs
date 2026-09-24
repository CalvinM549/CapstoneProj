using UnityEngine;

public class BossController : EnemyController
{
    protected override void Awake()
    {
        base.Awake();
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        // phase stuff
    }

    protected override void ApplyDamage(HitData hit)
    {
        base.ApplyDamage(hit);
    }

    protected override void Die()
    {
        // Other stuff to make the death cinematic

        base.Die();
    }
}
