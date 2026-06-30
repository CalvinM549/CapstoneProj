using DG.Tweening;
using UnityEngine;

public class RingController : MonoBehaviour
{
    [SerializeField] private float arcDegrees;
    [SerializeField] private float arcOffset;

    [SerializeField] private Color minColour;
    [SerializeField] private Color maxColour;

    private ProceduralRing ring;
    private Tween fillTween;
    private Tween colourTween;

    private void Awake()
    {
        ring = GetComponent<ProceduralRing>();
        ring.ArcOffset = arcOffset;
        ring.FillAmount = 0f;
        ring.color = minColour;
    }

    private void OnDestroy()
    {
        KillTweens();
    }

    public void Initialize(float arcOffset, float arcSpan, Color baseColour)
    {
        this.arcOffset = arcOffset;
        this.arcDegrees = arcSpan;

        minColour = baseColour;
        maxColour = baseColour;

        ApplyConfig(arcOffset, arcSpan, baseColour);
    }
    public void Initialize(float arcOffset, float arcSpan, Color minColour, Color maxColour)
    {
        this.arcOffset = arcOffset;
        this.arcDegrees = arcSpan;

        this.minColour = minColour;
        this.maxColour = maxColour;

        ApplyConfig(arcOffset, arcSpan, minColour);
    }

    private void ApplyConfig(float offset, float span, Color colour)
    {
        ring = GetComponent<ProceduralRing>();
        ring.ArcOffset = offset;
        ring.FillAmount = 0f;
        ring.color = colour;
    }

    public void SetFillImmediate(float normalized)
    {
        KillTweens();
        ring.FillAmount = NormalizedToFill(normalized);
    }

    public void SetFill(float normalized, float duration, Ease ease = Ease.OutCubic)
    {
        float target = NormalizedToFill(normalized);
        fillTween?.Kill();
        fillTween = DOTween
            .To(() => ring.FillAmount, x => ring.FillAmount = x, target, duration)
            .SetEase(ease);
    }

    public void SetColourImmediate(Color c)
    {
        colourTween?.Kill();
        ring.color = c;
    }

    public void SetColourFromNormal(float normalized)
    {
        ring.color = Color.Lerp(minColour, maxColour, normalized);
    }

    public void FlashColour(Color flashColour, float duration)
    {
        colourTween?.Kill();
        Color restore = ring.color;
        ring.color = flashColour;
        colourTween = ring.DOColor(restore, duration);
    }

    public ProceduralRing Ring => ring;

    #region Helpers

    private float NormalizedToFill(float normalized)
        => Mathf.Clamp01(normalized) * (arcDegrees / 360f);

    private void KillTweens()
    {
        fillTween?.Kill();
        colourTween?.Kill();
    }

    #endregion

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(ring == null) ring = GetComponent<ProceduralRing>();
        if (ring != null) ring.ArcOffset = arcDegrees;
    }
#endif
}
