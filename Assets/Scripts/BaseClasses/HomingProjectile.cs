using System;
using UnityEngine;
using static UnityEngine.UI.Image;

public class HomingProjectile : Projectile
{
    public Transform HomingTarget {  get; set; }

    [SerializeField] private float homingAccel;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float armingDistance;

    private Vector2 currentVelocity;
    private float distanceTravelled;
    private bool armed;

    public override void Initalize(ProjectileData data, Vector2 direction, Vector2 sourcePos, Action<Projectile> returnToPool, bool playerProjectile)
    {
        base.Initalize(data, direction, sourcePos, returnToPool, playerProjectile);

        currentVelocity = rb.linearVelocity;
        distanceTravelled = 0f;
        armed = false;
    }

    private void FixedUpdate()
    {
        if (HomingTarget == null || !TargetAlive()) return;

        distanceTravelled += currentVelocity.magnitude * Time.fixedDeltaTime;

        if (!armed)
        {
            if (distanceTravelled >= armingDistance)
                armed = true;
            else
                return;
        }

        Vector2 toTarget = ((Vector2)HomingTarget.position - rb.position).normalized;
        currentVelocity = Vector2.MoveTowards(currentVelocity.normalized, toTarget, homingAccel * Time.deltaTime)
            * Mathf.Min(currentVelocity.magnitude + homingAccel * Time.fixedDeltaTime, maxSpeed);
        rb.linearVelocity = currentVelocity;

        float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90);
    }

    private bool TargetAlive()
    {
        var damageable = HomingTarget.GetComponentInParent<IDamageable>();
        return damageable != null && damageable.IsAlive;
    }
}
