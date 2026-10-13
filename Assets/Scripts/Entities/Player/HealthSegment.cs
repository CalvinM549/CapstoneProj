using System;
using UnityEngine;

[Serializable]
public class HealthSegment
{
    public float maxHealth;
    public float currentHealth;

    public bool IsActive = false;

    public bool IsDestroyed => currentHealth <= 0f;
    public float PercentHealth => maxHealth > 0 ? (currentHealth / maxHealth) : 0f;

    public HealthSegment(float maxHealth)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
    }

    public float ReduceHealth(float amount)
    {        
        if (amount >= currentHealth)
        {
            float overflow = amount - currentHealth;
            currentHealth = 0f;
            return overflow;
        }

        currentHealth -= amount;
        return 0f;
    }

    public void RestoreHealth(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void RestoreSegment()
    {
        currentHealth = maxHealth;
    }

    public void Destroy()
    {
        currentHealth = 0f;
    }
}
