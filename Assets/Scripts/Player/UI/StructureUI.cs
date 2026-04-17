using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class StructureUI : MonoBehaviour
{
    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private Transform segmentContainer;

    [SerializeField] private float lerpSpeed;

    private class SegmentUI
    {

    }

    private List<SegmentUI> segmentUIs = new();

    private int activeIndex = -1;


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

    }

    private void RebuildSegments(IReadOnlyList<HealthSegment> segments)
    {

    }

    private void UpdateSegmentDisplay()
    {

    }

    private void UpdateActive()
    {

    }
}
