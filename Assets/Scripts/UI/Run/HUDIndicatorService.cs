using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class HUDIndicatorService : MonoBehaviour
{
    public static HUDIndicatorService Instance { get; private set; }

    [SerializeField] private Camera worldCam;
    [SerializeField] private RectTransform root;
    [SerializeField] private float edgePadding;

    [Header("Config")]
    
    [SerializeField] private IndicatorIconUI indicatorPrefab;
    [SerializeField] private float proximityFadeMin = 1.5f;
    [SerializeField] private float proximityFadeMax = 4.0f;

    [Space]
    [SerializeField] private RingController pingRingPrefab;
    [SerializeField] private float pingStartRadius;
    [SerializeField] private float pingEndRadius;
    [SerializeField] private float pingDuration;
    [SerializeField] private Color pingColour;

    private readonly Dictionary<Transform, IndicatorIconUI> activeIndicators = new();
    private readonly Dictionary<Transform, RingController> activePings = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        worldCam = Camera.main;
    }

    private void LateUpdate()
    {
        foreach (var kvp in activeIndicators)
            UpdateIndicator(kvp.Key, kvp.Value);

        foreach (var kvp in activePings)
        {
            Vector2 point = GetClampedScreenPoint(kvp.Key.position, out _, out _);
            ((RectTransform)kvp.Value.transform).anchoredPosition = point;
        }
    }

    public void SpawnIndicator(Transform anchor, Sprite icon, bool doPing = true)
    {
        if (activeIndicators.ContainsKey(anchor)) return;

        var indicator = Instantiate(indicatorPrefab, root);
        indicator.SetIcon(icon);
        activeIndicators[anchor] = indicator;

        if(doPing)
            PlayPing(anchor);
    }

    public void RemoveIndicator(Transform anchor)
    {
        if(!activeIndicators.TryGetValue(anchor, out var indicator)) return;
        activeIndicators.Remove(anchor);
        if(indicator != null)
            Destroy(indicator.gameObject);
    }

    public void ClearAll()
    {
        foreach (var indicator in activeIndicators)
            Destroy(indicator.Value.gameObject);
        
        foreach (var ping in activePings)
            Destroy(ping.Value.gameObject);

        activeIndicators.Clear();
        activePings.Clear();
    }

    private Vector2 GetClampedScreenPoint(Vector3 worldPos, out bool offscreen, out Vector2 direction)
    {
        Vector3 viewport = worldCam.WorldToViewportPoint(worldPos);
        bool behind = viewport.z < 0f;

        Vector2 centered = new Vector2(viewport.x - 0.5f, viewport.y - 0.5f);
        if (behind) centered = -centered;

        offscreen = behind || viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f;

        Rect rect = root.rect;
        Vector2 point = new Vector2(centered.x * rect.width, centered.y * rect.height);
        direction = point.normalized;

        if (offscreen)
        {
            Vector2 half = new Vector2(rect.width * 0.5f - edgePadding, rect.height * 0.5f - edgePadding);
            float scale = Mathf.Min(half.x / Mathf.Max(Mathf.Abs(direction.x), 0.0001f),
                                     half.y / Mathf.Max(Mathf.Abs(direction.y), 0.0001f));
            point = direction * scale;
        }

        return point;
    }

    private void UpdateIndicator(Transform anchor, IndicatorIconUI indicator)
    {
        Vector2 point = GetClampedScreenPoint(anchor.position, out bool offscreen, out Vector2 dir);
        indicator.RectTransform.anchoredPosition = point;
        indicator.SetPointing(offscreen, dir);

        float distance = Vector2.Distance(anchor.position, RunManager.Instance.activePlayer.transform.position);
        float alpha = Mathf.InverseLerp(proximityFadeMin, proximityFadeMax, distance);
        indicator.SetAlpha(alpha);
    }

    public void PlayPing(Transform anchor)
    {
        if (activePings.TryGetValue(anchor, out var existing)) return;

        Vector2 point = GetClampedScreenPoint(anchor.position, out _, out _);

        var ring = Instantiate(pingRingPrefab, root);
        activePings[anchor] = ring;
        
        ((RectTransform)ring.transform).anchoredPosition = point;

        ring.Initialize(0, 360f, pingColour);
        ring.Ring.Thickness = 15f;
        ring.SetFillImmediate(1f);
        ring.SetRadiusImmediate(pingStartRadius);

        ring.SetRadius(pingEndRadius, pingDuration, Ease.InOutCubic);
        ring.Ring
            .DOFade(0f, pingDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() => CleanupPing(anchor));
    }

    private void CleanupPing(Transform anchor)
    {
        if (!activePings.TryGetValue(anchor, out var existing)) return;

        existing?.DOKill();
        Destroy(existing.gameObject);
        activePings.Remove(anchor);
    }
}
