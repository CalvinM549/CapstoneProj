using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashUI : MonoBehaviour
{
    [SerializeField] private GameObject chargePrefab;
    [SerializeField] private Transform chargeContainer;

    [SerializeField] private List<Image> charges;

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        GameEvents.OnDashChargeChange += HandleChargeChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnDashChargeChange -= HandleChargeChanged;
    }

    private void HandleChargeChanged(float[] newCharges)
    {
        Debug.Log("1");
        if (newCharges.Length <= 0) return;
        if (newCharges.Length != charges.Count)
        {
            BuildDashCharges(newCharges);
        }

        int i = 0;

        foreach (var charge in charges)
        {
            charge.fillAmount = newCharges[i];

            i++;
        }
    }

    private void BuildDashCharges(float[] dashCooldowns)
    {
        charges.Clear();
        foreach (var dash in dashCooldowns)
        {
            var objRef = Instantiate(chargePrefab, chargeContainer);
            Image imageRef = objRef.GetComponent<Image>();

            charges.Add(imageRef);
            imageRef.fillAmount = dash;
        }
    }
}
