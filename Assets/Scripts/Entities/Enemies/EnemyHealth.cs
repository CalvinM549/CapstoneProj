using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private EnemyStats stats;
    private StatusEffectController statusController;

    private Material defaultMaterial;

    private StatValue maxHealth;
    private float currentHealth;

    public bool IsAlive { get; set; }

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth?.Value ?? 0f;
    public float PercentHealth => MaxHealth > 0f ? currentHealth / MaxHealth : 0f;

    public event Action<float, float, HitData> OnDamaged;
    public event Action OnDeath;

    public bool IsHitStunned { get; private set; }

    private bool hyperArmourActive;

    public void Initialize(EnemyStats stats, StatusEffectController statusController)
    {
        this.stats = stats;
        this.statusController = statusController;

        maxHealth = stats.GetStatValue(StatRef.EnemyMaxHealth);
        currentHealth = maxHealth.Value;

        IsAlive = true;
    }

    public void ResetForPool()
    {
        currentHealth = MaxHealth;
        IsAlive = true;
    }

    public void RecieveHit(HitData hit)
    {
        if (!IsAlive) return;
        if (!hit.isPlayerAttack) return;

        ApplyDamage(hit);
        ApplyStatuses(hit);


        if (hyperArmourActive && hit.attackType != AttackType.DamageOverTime)
        {
            ApplyStagger(hit);
            ApplyKnockback(hit);
        }
    }

    private void ApplyDamage(HitData hit)
    {
        float defenceMult = stats.Get(StatRef.EnemyIncomingDamageMult);
        hit.AddModifier(defenceMult - 1, StatModType.PercentAdd, this);

        currentHealth = Mathf.Max(0, hit.FinalDamage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ApplyStatuses(HitData hit)
    {
        foreach (var pending in hit.PendingStatusEffects)
        {
            statusController?.ApplyEffects(pending.data, pending.source, pending.appliedByPlayer, pending.stacks);
        }
    }

    private void ApplyStagger(HitData hit)
    {

    }

    private void ApplyKnockback(HitData hit)
    {

    }


    private void Die()
    {
        // fire event
    }
}
