using UnityEngine;

public interface IDamageable
{
    bool IsAlive { get; set; }

    void RecieveHit(HitData hit);
}
