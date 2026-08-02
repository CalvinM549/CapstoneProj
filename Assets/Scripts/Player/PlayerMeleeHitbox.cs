using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerMeleeHitbox : MonoBehaviour
{

    private bool active;
    private HashSet<Collider2D> hitTargets = new();

    private Collider2D col;

    public event Action<Collider2D> OnHitDetected;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        col.enabled = false;
    }

    public void Activate()
    {
        hitTargets.Clear();
        active = true;
        col.enabled = true;
    }

    public void Deactivate()
    {
        active = false;
        col.enabled = false;
        hitTargets.Clear();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!active) return;
        if (hitTargets.Contains(collision)) return;
        if (!collision.CompareTag("Hurtbox") && !collision.CompareTag("Enemy")) return;

        hitTargets.Add(collision);
        OnHitDetected?.Invoke(collision);
    }
}
