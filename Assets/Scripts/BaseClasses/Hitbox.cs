using System;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{

    private bool active;
    private AttackType currentType;
    private HashSet<Collider2D> hitTargets = new();


    public event Action<Collider2D, AttackType> OnHitDetected;

    public void Activate(AttackType type)
    {
        active = true;
        currentType = type;
        hitTargets.Clear();
    }

    public void Deactivate()
    {
        active = false;
        hitTargets.Clear();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!active) return;
        if (hitTargets.Contains(collision)) return;
        if (!collision.CompareTag("Enemy") && !collision.CompareTag("Hurtbox")) return;

        hitTargets.Add(collision);
        OnHitDetected.Invoke(collision, currentType);
    }
}
