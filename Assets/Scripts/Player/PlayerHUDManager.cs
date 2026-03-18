using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDManager : MonoBehaviour
{
    public Image[] dashCooldowns;

    public GameObject healthPointPrefab;
    public Transform healthContainer;

    private List<Image> healthPoints = new();

    private int maxHealth;
    private int currentHealth;

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    #region Health Bar

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
            var healthPoint = Instantiate(healthPointPrefab, healthContainer);
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

    #endregion

    #region DashCharges



    #endregion

    #region MomentumBar



    #endregion

}
