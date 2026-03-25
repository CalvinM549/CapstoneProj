using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDManager : MonoBehaviour
{
    public Image[] dashCooldowns;

    public Image healthPointPrefab;
    public Transform healthContainer;

    private List<Image> activeHealthPoints = new();

    private ObjectPool<Image> healthPointPool;

    private int maxHealth;
    private int currentHealth;

    private void Awake()
    {
        healthPointPool = new ObjectPool<Image>(healthPointPrefab, 10, healthContainer);
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerHealthChange += HandlePlayerHealthChange;
    }

    private void OnDisable()
    {
        
    }

    #region Health Bar

    public void HandlePlayerHealthChange(int newHealth, int max)
    {
        bool tookDamage = newHealth < currentHealth;

        if (tookDamage)
        {

        }

        RefreshHealthBar(newHealth);
    }

    public void InititializeHealth(int health)
    {
        BuildHealthBar(health);
    }

    private void BuildHealthBar(int count)
    {
        foreach (var hit in activeHealthPoints)
        {
            healthPointPool.ReturnToPool(hit);
            activeHealthPoints.Remove(hit);
        }

        healthContainer.gameObject.SetActive(true);

        for (int i = 0; i < count; i++)
        {
            activeHealthPoints.Add(healthPointPool.Get());
        }

        RefreshHealthBar(count);
    }

    private void RefreshHealthBar(int currentHealth)
    {

        for (int i = activeHealthPoints.Count; i > currentHealth; i--)
        {
            healthPointPool.ReturnToPool(activeHealthPoints[i]);
            activeHealthPoints.RemoveAt(i);
        }


        // Depending on current health, add effects, i.e. shake health when low
    }

    #endregion

    #region DashCharges



    #endregion

}
