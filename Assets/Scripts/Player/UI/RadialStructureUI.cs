using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialStructureUI : MonoBehaviour
{
    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private RectTransform segmentContainer;

    [SerializeField] private float ringRadius;

    [SerializeField] private float gapDegrees;

    [SerializeField] private float fillTweenDuration;

    [Serializable]
    private class UISegment
    {
        public float currentHealth;
        public float maxHealth;

        public float PercentHealth => currentHealth / maxHealth;

        public RectTransform pivot;
        public Image fillImage;
        public float arcFillAmount;

        private Tween activeTween;

        public UISegment(HealthSegment segment, RectTransform pivotRef, Image imageRef, float currentArc)
        {
            currentHealth = segment.currentHealth;
            maxHealth = segment.maxHealth;

            pivot = pivotRef;
            fillImage = imageRef;
            arcFillAmount = currentArc;
        }

        public void UpdateSegment(HealthSegment newValues, float tweenDuration)
        {
            bool wasAlive = currentHealth > 0f;
            bool valueChanged = newValues.currentHealth != currentHealth;

            currentHealth = newValues.currentHealth;
            maxHealth = newValues.maxHealth;

            float targetFill = PercentHealth * arcFillAmount;

            activeTween.Kill();
            fillImage?.DOKill();
            activeTween = fillImage.DOFillAmount(targetFill, tweenDuration).SetEase(Ease.OutCubic);


            if (valueChanged)
            {
                fillImage.color = Color.white;
                fillImage.DOFade(0.15f, tweenDuration);
            }

        }

        public void SetFillImmediate()
        {
            fillImage.fillAmount = PercentHealth * arcFillAmount;
        }
    }

    [SerializeField] private readonly List<UISegment> uiSegments = new();

    private void OnEnable()
    {
        GameEvents.OnPlayerHealthChanged += HandleStructureChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerHealthChanged -= HandleStructureChanged;

        foreach (var segment in uiSegments)
            segment.pivot?.DOKill();
    }

    private void HandleStructureChanged(IReadOnlyList<HealthSegment> segments)
    {
        if (segments == null || segments.Count == 0) return;

        if (segments.Count != uiSegments.Count)
        {
            BuildSegments(segments);
            return;
        }

        for (int i = segments.Count - 1; i >= 0; i--)
        {
            uiSegments[i].UpdateSegment(segments[i], fillTweenDuration);
        }
    }

    private void BuildSegments(IReadOnlyList<HealthSegment> segments)
    {
        foreach (var oldSegment in uiSegments)
        {
            oldSegment.pivot?.DOKill();
            if (oldSegment.pivot != null)
                Destroy(oldSegment.pivot.gameObject);
        }

        uiSegments.Clear();

        int count = segments.Count;
        if (count <= 0) return;

        float totalGap = gapDegrees * count;
        float halfGap = gapDegrees * 0.5f;
        float totalArcDegrees = 360f - totalGap;
        float arcPerSegment = totalArcDegrees / count;
        float arcFillAmount = arcPerSegment / 360;

        for (int i = 0; i < count; i++)
        {
            float startAngle = 45 - (i * (arcPerSegment + gapDegrees)) - halfGap;

            var pivot = CreateSegmentPivot(startAngle);
            var fillImage = ConfigureFillImage(pivot, arcFillAmount);

            var uiSegment = new UISegment(segments[i], pivot, fillImage, arcFillAmount);
            uiSegments.Add(uiSegment);
            uiSegment.SetFillImmediate();
        }
    }

    private RectTransform CreateSegmentPivot(float startAngleDegrees)
    {
        var segmentGO = Instantiate(segmentPrefab, segmentContainer);
        var segmentRect = segmentGO.GetComponent<RectTransform>();

        segmentRect.anchoredPosition = Vector2.zero;
        segmentRect.localRotation = Quaternion.Euler(0f, 0f, startAngleDegrees);

        return segmentRect;
    }

    private Image ConfigureFillImage(RectTransform pivot, float arcFillAmount)
    {
        //var segmentTransform = pivot.GetChild(0);
        var images = pivot.GetComponentsInChildren<Image>();

        Image fillImage = images.Length > 1 ? images[1] : images[0];

        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Radial360;
        fillImage.fillOrigin = (int)Image.Origin360.Top;
        fillImage.fillClockwise = true;

        fillImage.fillAmount = arcFillAmount;

        return fillImage;
    }
}
