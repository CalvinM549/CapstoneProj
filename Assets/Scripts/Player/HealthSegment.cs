using System;
using UnityEngine;

[Serializable]
public class HealthSegment
{
    public float maxHealth;
    public float currentHealth;

    public bool IsDestroyed = false;
    public bool IsActive = false;

    public float PercentHealth => maxHealth > 0 ? (currentHealth / maxHealth) : 0f;

    public HealthSegment(float maxHealth, bool isActive)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
        IsDestroyed = false;
        IsActive = isActive;
    }

    public float ReduceHealth(float amount)
    {
        float overflow = 0f;
        
        if (amount >= currentHealth)
        {
            overflow = amount - currentHealth;
            Destroy();
        }
        else
        {
            currentHealth -= amount;
        }

        return overflow;
    }

    public void RestoreHealth(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void RestoreSegment()
    {
        IsDestroyed = false;
        currentHealth = maxHealth;

        // Trigger event
    }

    public void Destroy()
    {
        currentHealth = 0f;
        IsDestroyed = true;

        // Trigger Event
    }
}
