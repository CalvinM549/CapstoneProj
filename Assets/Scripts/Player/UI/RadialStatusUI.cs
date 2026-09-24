using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class RadialStatusUI : MonoBehaviour
{
    [SerializeField] private GameObject statusPrefab;
    [SerializeField] private RectTransform container;

    [Header("Config")]
    [SerializeField] private float radius;
    [SerializeField] private float arcSpanDegrees;
    [SerializeField] private float arcCenterAngle;
    [SerializeField] private float gapBetween;

    private Dictionary<ActiveStatusEffect, Image> statusIcons = new();

    private StatusEffectController statusController;

    private void Awake()
    {
        statusController = GetComponentInParent<StatusEffectController>();
    }

    private void OnEnable()
    {
        statusController.OnEffectsChanged += HandleStatusUpdated;
        
    }

    private void OnDisable()
    {
        if(statusController != null)
            statusController.OnEffectsChanged -= HandleStatusUpdated;
    }

    private void HandleStatusUpdated()
    {
        BuildIcons();
    }

    private void BuildIcons()
    {
        var activeStatuses = statusController.AllActiveEffects;

        int count = activeStatuses.Count;
        if (count == 0) return;

        float startAngle = arcCenterAngle - ((count - 1) * gapBetween * 0.5f);
        float step = gapBetween;

        int current = 0;
        foreach (var effect in activeStatuses)
        {
            float angle = RadialUIHelper.AngleForIndex(current, count, arcCenterAngle, gapBetween);
            var pivot = RadialUIHelper.CreatePivot(container, statusPrefab, -angle, radius, out var instance);

            var image = ConfigureImage(instance, effect.data);

            statusIcons[effect] = image;
            current++;
        }
    }

    private Image ConfigureImage(GameObject instance, StatusEffectData data)
    {
        var img = instance.GetComponent<Image>();

        img.transform.localScale *= 0.5f;

        img.sprite = data.icon;
        // set rotation to be vertical

        return img;
    }
}
