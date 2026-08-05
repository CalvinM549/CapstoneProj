using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestructableObject : MonoBehaviour, IDamageable
{
    private SpriteRenderer sr;
    private Animator animator;

    [SerializeField] private Material damageMaterial;
    private Material baseMaterial;

    private int currentHealth;
    [SerializeField] private int maxHealth;

    [SerializeField] List<AttackType> blockedTypes;

    public bool IsAlive {  get; set; }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        baseMaterial = sr.material;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        IsAlive = true;
    }

    public void RecieveHit(HitData hit)
    {
        if (blockedTypes.Contains(hit.attackType)) 
            return;

        StartCoroutine(HitFXRoutine(hit.hitstunTime));

        currentHealth--;

        if (currentHealth <= 0)
            Invoke(nameof(DestroyObject), hit.hitstunTime/2);
    }

    private IEnumerator HitFXRoutine(float duration)
    {
        sr.material = damageMaterial;

        yield return new WaitForSeconds(duration);

        sr.material = baseMaterial;
    }

    public void DestroyObject()
    {
        IsAlive = false;

        // Do stuff
        Destroy(gameObject);
    }
}
