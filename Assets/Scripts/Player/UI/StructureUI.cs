using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureUI : MonoBehaviour
{
    private class UISegment
    {
        public float maxHealth;
        public float currentHealth;

        public float percentHealth => currentHealth / maxHealth;

        public Image healthBar;
        // delay fill bar??

        public UISegment(HealthSegment segment)
        {
            currentHealth = segment.currentHealth;
            maxHealth = segment.maxHealth;
        }

        public void UpdageSegment(HealthSegment newValues)
        {
            currentHealth = newValues.currentHealth;
            maxHealth = newValues.maxHealth;

            // Update fill bar with juice
            healthBar.fillAmount = percentHealth;
        }
    }

    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private Transform segmentContainer;

    [SerializeField] private float lerpSpeed;


    private List<UISegment> uiSegments = new();


    private void OnEnable()
    {
        // 
    }
    private void OnDisable()
    {
        
    }

    private void Update()
    {
        UpdateSegmentDisplay();
        UpdateActive();
    }

    private void HandleStructureChanged(IReadOnlyList<HealthSegment> segments)
    {
        if (segments.Count != uiSegments.Count)
        {
            BuildSegments(segments);
        }

        for (int i = segments.Count -1; i <= 0; i--)
        {
            uiSegments[i].UpdageSegment(segments[i]);
            if (segments[i].IsDestroyed)
            {
                uiSegments.Remove(uiSegments[i]);
            }
        }
    }

    private void BuildSegments(IReadOnlyList<HealthSegment> segments)
    {
        uiSegments.Clear();
        foreach (var segment in segments)
        {
            uiSegments.Add(new UISegment(segment));

            // Spawn UI Objects
        }
    }

    private void UpdateSegmentDisplay()
    {

    }

    private void UpdateActive()
    {

    }
}
