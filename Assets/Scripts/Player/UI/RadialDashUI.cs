using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System;

public class RadialDashUI : MonoBehaviour
{
    [SerializeField] private GameObject chargePrefab;
    [SerializeField] private RectTransform chargeContainer;

    [SerializeField] private float chargeRadius;

    [SerializeField] private float arcSpanDegrees;
    [SerializeField] private float arcCenterAngle;
    [SerializeField] private float gapBetweenCharges;

    private class ChargeIcon
    {
        public RectTransform pivot { get; }
        public Image fillImage { get; }

        public float lastValue;
        private bool charged;

        private Color baseColour;

        public ChargeIcon(RectTransform pivotRef, Image imageRef, float initialValue)
        {
            pivot = pivotRef;
            fillImage = imageRef;
            lastValue = initialValue;
            fillImage.fillAmount = initialValue;
            baseColour = fillImage.color;
        }

        public void UpdateCharge(float newValue)
        {
            //tween?.Kill();
            //tween = fillImage.DOFillAmount(newValue, 0.3f).SetEase(Ease.OutQuart);

            fillImage.fillAmount = newValue;

            if (newValue >= 0.99f && !charged)
            {
                fillImage.DOKill();
                fillImage.transform.localScale = Vector3.one;
                fillImage.transform.DOPunchScale(Vector3.one * 0.2f, 0.35f, 6, 0.4f);
                fillImage.color = Color.white;
                fillImage.DOFade(baseColour.a, 0.35f);
                charged = true;
            }

            if (newValue < lastValue && charged)
            {
                charged = false;
            }

            lastValue = newValue;
        }

        public void Kill()
        {
            pivot.DOKill();
            fillImage.DOKill();
        }
    }

    private readonly List<ChargeIcon> chargeIcons = new();

    private void OnEnable()
    {
        GameEvents.OnDashChargeChange += HandleChargeChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnDashChargeChange -= HandleChargeChanged;
        foreach (var icon in chargeIcons)
            icon.Kill();
    }

    private void HandleChargeChanged(float[] newCharges)
    {
        if (newCharges == null || newCharges.Length == 0) return;

        if (newCharges.Length != chargeIcons.Count)
        {
            BuildCharges(newCharges);
            return;
        }

        for (int i = 0; i < newCharges.Length; i++)
        {
            chargeIcons[i].UpdateCharge(newCharges[i]);
        }
    }

    private void BuildCharges(float[] dashCooldowns)
    {
        foreach (var old in chargeIcons)
        {
            old.Kill();
            if (old.pivot != null)
                Destroy(old.pivot.gameObject);
        }

        chargeIcons.Clear();

        int count = dashCooldowns.Length;
        if (count == 0) return;

        float startAngle = arcCenterAngle - ((count - 1) * gapBetweenCharges * 0.5f);
        float step = gapBetweenCharges;

        for (int i = 0; i < count; i++)
        {
            float angle = RadialUIHelper.AngleForIndex(i, count, arcCenterAngle, gapBetweenCharges);
            var pivot = RadialUIHelper.CreatePivot(chargeContainer, chargePrefab, -angle, chargeRadius, out var instance);

            var fillImage = ConfigureFillImage(instance);
            chargeIcons.Add(new ChargeIcon(pivot, fillImage, dashCooldowns[i]));
        }
    }

    private Image ConfigureFillImage(GameObject iconInstance)
    {
        var img = iconInstance.GetComponent<Image>() ?? iconInstance.GetComponentInChildren<Image>();

        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillOrigin = (int)Image.Origin360.Top;
        img.fillClockwise = true;

        return img;
    }
}
