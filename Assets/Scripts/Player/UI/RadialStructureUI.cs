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

    [SerializeField] private float gapDegrees;

    [SerializeField] private float fillTweenDuration;

    [Serializable]
    private class UISegment
    {
        public float currentHealth;
        public float maxHealth;

        public float PercentHealth => currentHealth / maxHealth;

        private readonly RingController controller;

        public UISegment(HealthSegment segment, RingController controller)
        {
            currentHealth = segment.currentHealth;
            maxHealth = segment.maxHealth;

            this.controller = controller;

        }

        public void UpdateSegment(HealthSegment newValues, float tweenDuration)
        {
            bool valueChanged = newValues.currentHealth != currentHealth;

            currentHealth   = newValues.currentHealth;
            maxHealth       = newValues.maxHealth;

            controller.SetFill(PercentHealth, tweenDuration);

            if (valueChanged)
            {
                controller.FlashColour(Color.red, tweenDuration);
            }

        }

        public void SetFillImmediate()
        {
            controller.SetFillImmediate(PercentHealth);
        }

        public void Kill()
        {
            if (controller != null)
                Destroy(controller.gameObject);
        }
    }

    private readonly List<UISegment> uiSegments = new();

    private void OnEnable()
    {
        GameEvents.OnPlayerHealthChanged += HandleStructureChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerHealthChanged -= HandleStructureChanged;
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
        foreach (var old in uiSegments)
            old.Kill();
        uiSegments.Clear();

        int count = segments.Count;
        if (count <= 0) return;

        float totalArcDeg = 360f - (gapDegrees * count);
        float arcPerSegment = totalArcDeg / count;

        float startOffset = 45f + gapDegrees * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float arcOffset = startOffset + i * (arcPerSegment + gapDegrees);

            var controller = CreateRing(arcOffset, arcPerSegment, segments[i]);
            var uiSegment = new UISegment(segments[i], controller);

            uiSegments.Add(uiSegment);
            uiSegment.SetFillImmediate();
        }
    }

    private RingController CreateRing(float arcOffset, float arcSpan, HealthSegment segment)
    {
        var segmentGO = Instantiate(segmentPrefab, segmentContainer);
        segmentGO.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        var controller = segmentGO.GetComponent<RingController>();
        if (controller == null) return null;

        controller.Initialize(arcOffset,
            arcSpan, 
            controller.Ring != null ? controller.Ring.color : Color.pink);
        return controller;
    }

    //private RectTransform CreateSegmentPivot(float startAngleDegrees)
    //{
    //    var segmentGO = Instantiate(segmentPrefab, segmentContainer);
    //    var segmentRect = segmentGO.GetComponent<RectTransform>();

    //    segmentRect.anchoredPosition = Vector2.zero;
    //    segmentRect.localRotation = Quaternion.Euler(0f, 0f, startAngleDegrees);

    //    return segmentRect;
    //}

    //private Image ConfigureFillImage(RectTransform pivot, float arcFillAmount)
    //{
    //    //var segmentTransform = pivot.GetChild(0);
    //    var images = pivot.GetComponentsInChildren<Image>();

    //    Image fillImage = images.Length > 1 ? images[1] : images[0];

    //    fillImage.type = Image.Type.Filled;
    //    fillImage.fillMethod = Image.FillMethod.Radial360;
    //    fillImage.fillOrigin = (int)Image.Origin360.Top;
    //    fillImage.fillClockwise = true;

    //    fillImage.fillAmount = arcFillAmount;

    //    return fillImage;
    //}
}
