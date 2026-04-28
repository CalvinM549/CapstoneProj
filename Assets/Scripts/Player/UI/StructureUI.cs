using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureUI : MonoBehaviour
{
    [Serializable]
    private class UISegment
    {
        public float maxHealth;
        public float currentHealth;

        public float percentHealth => currentHealth / maxHealth;

        public Image healthBar;
        // delay fill bar??

        public UISegment(HealthSegment segment, Image imageRef)
        {
            currentHealth = segment.currentHealth;
            maxHealth = segment.maxHealth;

            healthBar = imageRef;
        }

        public void UpdateSegment(HealthSegment newValues)
        {
            currentHealth = newValues.currentHealth;
            maxHealth = newValues.maxHealth;

            // Update fill bar with juice
            healthBar.fillAmount = this.percentHealth;
        }
    }

    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private Transform segmentContainer;

    [SerializeField] private float lerpSpeed;

    [SerializeField]
    private List<UISegment> uiSegments = new();


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
        Debug.Log($"Updating Health UI, {segments.Count}");

        if (segments.Count != uiSegments.Count)
        {
            BuildSegments(segments);
        }

        for (int i = segments.Count -1; i >= 0; i--)
        {
            uiSegments[i].UpdateSegment(segments[i]);
        }
    }

    private void BuildSegments(IReadOnlyList<HealthSegment> segments)
    {
        uiSegments.Clear();
        foreach (var segment in segments)
        {
            var objRef = Instantiate(segmentPrefab, segmentContainer);
            Image imageRef = objRef.GetComponentsInChildren<Image>()[1];

            UISegment newSegment = new UISegment(segment, imageRef);
            uiSegments.Add(newSegment);
            newSegment.UpdateSegment(segment);
        }
    }
}
