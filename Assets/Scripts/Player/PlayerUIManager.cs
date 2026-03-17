using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public Image[] dashCooldowns;

    public GameObject hitPrefab;
    public Transform healthContainer;

    private List<Image> healthPoints = new();

    private int maxHealth;
    private int currentHealth;


    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    public void ManageHealthChange(int newHealth, int max)
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
        foreach (var hit in healthPoints)
        {
            if (hit != null) Destroy(hit);
        }
        healthPoints.Clear();


        healthContainer.gameObject.SetActive(true);

        for (int i = 0; i < count; i++)
        {
            var healthPoint = Instantiate(hitPrefab, healthContainer);
            healthPoints.Add(healthPoint.GetComponent<Image>());
        }

        RefreshHealthBar(count);
    }

    private void RefreshHealthBar(int currentHealth)
    {

        for (int i = 0; i < healthPoints.Count; i++)
        {
            if (healthPoints[i] == null) continue;
            // Swap image sprites based on amount of health
        }
    }

}
