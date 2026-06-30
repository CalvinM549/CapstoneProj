
using DG.Tweening;
using UnityEngine;

public class TargetReticleUI : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Rings — assign or leave null to auto-generate")]
    [SerializeField] private ProceduralRing[] outerArcs;   // 4 arcs
    [SerializeField] private ProceduralRing innerRing;
    [SerializeField] private ProceduralRing[] cornerTicks; // 4 ticks

    [Header("Geometry")]
    [SerializeField] private float outerRadius = 60f;
    [SerializeField] private float outerThickness = 6f;
    [SerializeField] private float outerArcSpanDeg = 65f;   // each arc covers this many degrees

    [SerializeField] private float innerRadius = 38f;
    [SerializeField] private float innerThickness = 2f;

    [SerializeField] private float tickRadius = 60f;
    [SerializeField] private float tickThickness = 6f;
    [SerializeField] private float tickSpanDeg = 20f;

    [Header("Colours")]
    [SerializeField] private Color acquiredColour = new Color(0.4f, 0.9f, 1f, 1f);   // cyan
    [SerializeField] private Color releasedColour = new Color(0.4f, 0.9f, 1f, 0.4f);
    [SerializeField] private Color innerColour = new Color(0.4f, 0.9f, 1f, 0.5f);

    [Header("Acquire animation")]
    [SerializeField] private float acquireDuration = 0.35f;
    [SerializeField] private float contractAmount = 14f;   // px arcs move inward

    [Header("Release animation")]
    [SerializeField] private float releaseDuration = 0.25f;
    [SerializeField] private float expandAmount = 20f;   // px arcs move outward before destroy

    [Header("Idle animation")]
    [SerializeField] private float rotationSpeed = 18f;   // degrees/sec, outer arcs
    [SerializeField] private float pulsePeriod = 1.4f;  // seconds for one inner pulse cycle

    // ── Runtime state ─────────────────────────────────────────────────────────

    private bool _acquired;
    private float _idleRotation;
    private Tween _innerPulseTween;

    // Cached starting radii so acquire/release deltas compound correctly
    private float _outerRadiusAcquired;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (outerArcs == null || outerArcs.Length == 0 ||
            innerRing == null ||
            cornerTicks == null || cornerTicks.Length == 0)
        {
            BuildRings();
        }

        // Start invisible — Acquire() will animate in
        SetAlpha(0f);
    }

    private void OnDisable()
    {
        KillAllTweens();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_acquired) Release();
            else Acquire();
        }

        if (!_acquired) return;

        // Slowly rotate outer arcs
        _idleRotation += rotationSpeed * Time.deltaTime;
        ApplyOuterRotation(_idleRotation);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Animate the reticle into its acquired state.</summary>
    public void Acquire()
    {
        if (_acquired) return;
        _acquired = true;

        KillAllTweens();
        gameObject.SetActive(true);

        // Fade entire reticle in
        var canvasGroup = GetOrAddCanvasGroup();
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, acquireDuration).SetEase(Ease.OutCubic);

        // Outer arcs contract inward
        float targetRadius = outerRadius - contractAmount;
        _outerRadiusAcquired = targetRadius;
        foreach (var arc in outerArcs)
        {
            arc.Radius = outerRadius + contractAmount;   // start pushed out
            DOTween.To(() => arc.Radius, r => arc.Radius = r, targetRadius, acquireDuration)
                   .SetEase(Ease.OutBack);
            arc.color = acquiredColour;
        }

        // Corner ticks snap in with a punch
        foreach (var tick in cornerTicks)
        {
            tick.transform.localScale = Vector3.zero;
            tick.transform.DOScale(Vector3.one, acquireDuration * 0.7f)
                .SetEase(Ease.OutBack).SetDelay(acquireDuration * 0.2f);
            tick.color = acquiredColour;
        }

        // Inner ring pulse
        innerRing.color = innerColour;
        StartInnerPulse();
    }

    /// <summary>Animate the reticle out and destroy it.</summary>
    public void Release()
    {
        if (!_acquired) return;
        _acquired = false;

        KillAllTweens();

        var canvasGroup = GetOrAddCanvasGroup();

        // Outer arcs expand outward as they fade
        float targetRadius = _outerRadiusAcquired + expandAmount;
        foreach (var arc in outerArcs)
        {
            DOTween.To(() => arc.Radius, r => arc.Radius = r, targetRadius, releaseDuration)
                   .SetEase(Ease.InCubic);
            arc.DOColor(releasedColour, releaseDuration);
        }

        // Ticks shrink out
        foreach (var tick in cornerTicks)
            tick.transform.DOScale(Vector3.zero, releaseDuration * 0.6f).SetEase(Ease.InBack);

        // Fade out then destroy
        canvasGroup.DOFade(0f, releaseDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() => {
                KillAllTweens();
                gameObject.SetActive(false);
            });
    }

    // ── Ring generation ───────────────────────────────────────────────────────

    /// <summary>
    /// Procedurally create all ring children. Called automatically in Awake
    /// if no rings are assigned in the Inspector.
    /// </summary>
    public void BuildRings()
    {
        // Outer arcs — four quadrants, offset 45° so gaps sit at compass points
        outerArcs = new ProceduralRing[4];
        float outerGap = (360f - outerArcSpanDeg * 4f) / 4f;
        for (int i = 0; i < 4; i++)
        {
            float offset = i * (outerArcSpanDeg + outerGap) + outerGap * 0.5f;
            outerArcs[i] = CreateRing(
                $"OuterArc_{i}", outerRadius, outerThickness, outerArcSpanDeg / 360f, offset, acquiredColour);
        }

        // Inner ring — full circle
        innerRing = CreateRing("InnerRing", innerRadius, innerThickness, 1f, 0f, innerColour);

        // Corner ticks — short arcs at 45° offsets between outer arcs
        cornerTicks = new ProceduralRing[4];
        for (int i = 0; i < 4; i++)
        {
            float offset = i * 90f + 45f - tickSpanDeg * 0.5f;
            cornerTicks[i] = CreateRing(
                $"CornerTick_{i}", tickRadius, tickThickness, tickSpanDeg / 360f, offset, acquiredColour);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private ProceduralRing CreateRing(string ringName, float radius, float thickness,
                                       float fillAmount, float arcOffset, Color colour)
    {
        var go = new GameObject(ringName, typeof(RectTransform), typeof(CanvasRenderer));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(transform, false);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;

        var ring = go.AddComponent<ProceduralRing>();
        ring.Radius = radius;
        ring.Thickness = thickness;
        ring.FillAmount = fillAmount;
        ring.ArcOffset = arcOffset;
        ring.color = colour;

        return ring;
    }

    private void ApplyOuterRotation(float degrees)
    {
        if (outerArcs == null) return;
        float outerGap = (360f - outerArcSpanDeg * 4f) / 4f;
        for (int i = 0; i < outerArcs.Length; i++)
        {
            float baseOffset = i * (outerArcSpanDeg + outerGap) + outerGap * 0.5f;
            outerArcs[i].ArcOffset = baseOffset + degrees;
        }
    }

    private void StartInnerPulse()
    {
        _innerPulseTween = innerRing
            .DOFade(innerColour.a * 0.25f, pulsePeriod * 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void KillAllTweens()
    {
        _innerPulseTween?.Kill();
        if (outerArcs != null)
            foreach (var arc in outerArcs) arc?.DOKill();
        if (cornerTicks != null)
            foreach (var tick in cornerTicks) tick?.DOKill();
        innerRing?.DOKill();
    }

    private void SetAlpha(float alpha)
    {
        GetOrAddCanvasGroup().alpha = alpha;
    }

    private CanvasGroup GetOrAddCanvasGroup()
    {
        var cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        return cg;
    }

    // ── Event handler ─────────────────────────────────────────────────────────

    private void HandleTargetChanged(GameObject newTarget)
    {
        // This instance is on the enemy itself — only respond to events
        // that reference our own GameObject.
        if (newTarget == null)
            Release();
    }
}
