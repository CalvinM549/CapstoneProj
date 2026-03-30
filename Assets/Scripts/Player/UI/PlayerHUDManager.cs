using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Image healthPointPrefab;
    [SerializeField] private Transform healthContainer;

    private List<Image> activeHealthPoints = new();

    private int currentHealth;


    [Header("Dash Charges")]
    [SerializeField] private Image dashChargePrefab;
    [SerializeField] private Transform dashChargeContainer;

    private List<Image> activeDashCharges = new();
    private int maxDashCharges;


    private ObjectPool<Image> healthPointPool;
    private ObjectPool<Image> dashChargePool;

    private void Awake()
    {
        healthPointPool = new ObjectPool<Image>(healthPointPrefab, 10, healthContainer);
        dashChargePool = new ObjectPool<Image>(dashChargePrefab, 3, dashChargeContainer);
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerHealthChange += HandlePlayerHealthChange;
        GameEvents.OnDashChargeChange += HandleDashChargeChange;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerHealthChange -= HandlePlayerHealthChange;
        GameEvents.OnDashChargeChange -= HandleDashChargeChange;
    }

    private void Start()
    {
        Initialize(3, 2);
    }

    public void Initialize(int maxHealth, int maxDashes)
    {
        BuildHealthBar(maxHealth);

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

    private void HandleDashChargeChange(int currentCharges, int max, float[] rechargeTimers, float rechargeTime)
    {
        if (max != maxDashCharges)
            BuildDashCharges(max);

        for (int i = 0; i < activeDashCharges.Count; i++)
        {
            if (i < currentCharges)
            {
                activeDashCharges[i].fillAmount = 1f;
                activeDashCharges[i].color = Color.white;
            }
            else
            {
                int timerIndex = i - currentCharges;
                if (timerIndex < rechargeTimers.Length && rechargeTimers[timerIndex] > 0)
                {
                    float progress = 1f - (rechargeTimers[timerIndex] / rechargeTime);
                    activeDashCharges[i].fillAmount = progress;
                    activeDashCharges[i].color = Color.red;
                }
                else
                {
                    activeDashCharges[i].fillAmount = 0f;
                    activeDashCharges[i].color = Color.red;
                }
            }
        }
    }

    private void BuildDashCharges(int count)
    {
        maxDashCharges = count;

        foreach (var charge in activeDashCharges)
        {
            dashChargePool.ReturnToPool(charge);
            activeDashCharges.Remove(charge);
        }

        dashChargeContainer.gameObject.SetActive(true);

        for (int i = 0; i < count; i++)
        {
            Image newCharge = dashChargePool.Get();
            activeDashCharges.Add(newCharge);
            newCharge.fillAmount = 1f;
        }
    }

    #endregion

}
