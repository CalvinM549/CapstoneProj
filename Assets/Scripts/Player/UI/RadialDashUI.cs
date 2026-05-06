using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using NUnit.Framework;
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
        public RectTransform pivot;
        public Image fillImage;
        public float lastValue;

        private bool charged;

        private Tween tween;

        public ChargeIcon(RectTransform pivotRef, Image imageRef, float initialValue)
        {
            pivot = pivotRef;
            fillImage = imageRef;
            lastValue = initialValue;
            fillImage.fillAmount = initialValue;
        }

        public void UpdateCharge(float newValue)
        {
            bool consumed = newValue < lastValue - 0.5f;

            tween?.Kill();
            tween = fillImage.DOFillAmount(newValue, 0.3f).SetEase(Ease.OutQuart);

            if (newValue >= 0.99f && !charged)
            {
                fillImage.DOKill();
                fillImage.transform.DOPunchScale(Vector3.one * 0.2f, 0.35f, 6, 0.4f);
                fillImage.color = Color.white;
                fillImage.DOFade(0.15f, 0.35f);
                charged = true;
            }
            else if (consumed)
            {
                fillImage.DOKill();
                fillImage.transform.DOPunchScale(Vector3.one * 0.2f, 0.35f, 4, 0.3f);
                charged = false;
            }

            lastValue = newValue;
        }

        public void Kill()
        {
            tween?.Kill();
            pivot?.DOKill();
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
        foreach (var oldCharge in chargeIcons)
        {
            oldCharge.Kill();
            if (oldCharge.pivot != null)
                Destroy(oldCharge.pivot.gameObject);
        }

        chargeIcons.Clear();

        int count = dashCooldowns.Length;
        if (count == 0) return;

        float startAngle = arcCenterAngle - ((count - 1) * gapBetweenCharges * 0.5f);
        float step = gapBetweenCharges;

        for (int i = 0; i < count; i++)
        {
            float angleDeg = count > 1 ? startAngle + step * i : arcCenterAngle;

            float pivotAngle = -angleDeg;
            var pivot = CreateChargePivot(pivotAngle);
            var fillImage = ConfigureFillImage(pivot);

            var icon = new ChargeIcon(pivot, fillImage, dashCooldowns[i]);
            chargeIcons.Add(icon);
        }
    }

    private RectTransform CreateChargePivot(float pivotAngle)
    {
        var pivotGO = new GameObject("ChargePivot", typeof(RectTransform));
        var pivot = pivotGO.GetComponent<RectTransform>();

        pivot.SetParent(chargeContainer, false);
        pivot.anchoredPosition = Vector2.zero;
        pivot.sizeDelta = Vector2.zero;
        pivot.localRotation = Quaternion.Euler(0f, 0f, pivotAngle);

        var iconGO = Instantiate(chargePrefab, pivot);
        var iconRect = iconGO.GetComponent<RectTransform>();

        iconRect.anchoredPosition = new Vector2(0f, chargeRadius);
        iconRect.localRotation = Quaternion.Euler(0f, 0f, -pivotAngle);

        return pivot;
    }

    private Image ConfigureFillImage(RectTransform pivot)
    {
        var iconTransform = pivot.GetChild(0);
        var img = iconTransform.GetComponent<Image>();
        if(img == null)
            img = iconTransform.GetComponentInChildren<Image>();

        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillOrigin = (int)Image.Origin360.Top;
        img.fillClockwise = true;

        return img;
    }
}
