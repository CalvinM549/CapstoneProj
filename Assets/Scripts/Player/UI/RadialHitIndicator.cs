using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialHitIndicator : MonoBehaviour
{
    PlayerUI playerUI;

    [SerializeField] private HitMarker markerPrefab;
    [SerializeField] private Transform markerContainer;

    private Transform playerTransform;

    [SerializeField] private int poolSize;

    [Header("Visual")]
    [SerializeField] private float markerRadius;
    [SerializeField] private float maxDamageRef = 5;

    [SerializeField] private Color minDamageColour;
    [SerializeField] private Color maxDamageColour;

    [SerializeField] private float minPunchScale;
    [SerializeField] private float maxPunchScale;

    [SerializeField] private float holdDuration;
    [SerializeField] private float fadeDuration;

    [SerializeField] private float mergeAngleThreshold;
    [SerializeField] private float mergeTimeWindow;

    private ObjectPool<HitMarker> markerPool;
    private readonly List<HitMarker> activeMarkers = new();

    private void Awake()
    {
        markerPool = new ObjectPool<HitMarker>(markerPrefab, poolSize, markerContainer);

        playerUI = GetComponent<PlayerUI>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerHit += HandlePlayerHit;

        playerUI.Initialized += HandleUIReady;
        if (playerUI.player != null) HandleUIReady(playerUI.player);
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerHit -= HandlePlayerHit;
    }

    private void HandleUIReady(Player p)
    {
        playerTransform = p.transform;
    }

    private void HandlePlayerHit(HitData hit)
    {
        if (playerTransform == null) playerTransform = playerUI.playerPos;
                
        Vector2 direction = ((Vector2)playerTransform.position - hit.sourcePos).normalized;
        if (direction == Vector2.zero) return;

        float normalizedDamage = Mathf.Clamp01((float)hit.damage / maxDamageRef);

        SetupMarker(direction, normalizedDamage);
    }

    private void SetupMarker(Vector3 direction, float normalizedDamage)
    {
        float peakScale = Mathf.Lerp(minPunchScale, maxPunchScale, normalizedDamage);
        Color color = Color.Lerp(minDamageColour, maxDamageColour, normalizedDamage);

        HitMarker existing = FindMerge(direction);
        if (existing != null)
        {
            existing.Merge(peakScale, holdDuration, fadeDuration, color);
            return;
        }

        if (activeMarkers.Count >= poolSize)
        {
            HitMarker oldest = GetOldestActive();
            if (oldest != null)
            {
                activeMarkers.Remove(oldest);
                oldest.ForceReturn();
            }
        }

        HitMarker marker = markerPool.Get();
        activeMarkers.Add(marker);

        marker.Activate(
            direction, markerRadius, 
            peakScale, holdDuration,
            fadeDuration, color,
            returnedMarker =>
            {
                activeMarkers.Remove(returnedMarker);
                markerPool.ReturnToPool(returnedMarker);
            }
        );
    }

    private HitMarker FindMerge(Vector3 incomingDir)
    {
        foreach (var marker in activeMarkers)
        {
            if (marker.Age > mergeTimeWindow) continue;
            if(Vector3.Angle(marker.Direction, incomingDir) <= mergeAngleThreshold)
                return marker;
        }
        return null;
    }

    private HitMarker GetOldestActive()
    {
        if(activeMarkers.Count == 0) 
            return null;

        HitMarker oldest = activeMarkers[0];
        foreach (var marker in activeMarkers)
            if(marker.Age > oldest.Age) oldest = marker;

        return oldest;
    }

}
