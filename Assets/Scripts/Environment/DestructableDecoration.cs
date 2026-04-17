using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestructableDecoration : MonoBehaviour, IDamageable
{
    private int currentHealth;
    [SerializeField] private int maxHealth;

    [SerializeField] List<AttackType> blockedTypes;

    public bool IsAlive {  get; set; }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void RecieveHit(HitData hit)
    {
        if (blockedTypes.Contains(hit.attackType)) 
            return;

        currentHealth--;

        if (currentHealth <= 0)
            DestroyObject();
    }

    public void DestroyObject()
    {
        // Do stuff
        Destroy(gameObject);
    }
}
