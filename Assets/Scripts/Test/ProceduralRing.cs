using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generates a hollow ring (annulus arc) as a procedural UI mesh.
/// Drop this on a GameObject with a CanvasRenderer — no sprite needed.
///
/// Key parameters (all editable in the Inspector and at runtime via the
/// public setters):
///   Radius       – outer radius in canvas pixels
///   Thickness    – radial width of the ring band
///   FillAmount   – 0..1, how much of the arc is filled (like Image.fillAmount)
///   ArcOffset    – rotation of the arc start point in degrees (0 = top)
///   Colour       – tint applied to the mesh
///   Segments     – triangle smoothness (higher = rounder, more verts)
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class ProceduralRing : MaskableGraphic
{
    // ── Inspector fields ──────────────────────────────────────────────────────

    [Header("Shape")]
    [SerializeField, Min(1f)] private float _radius = 80f;
    [SerializeField, Min(0.1f)] private float _thickness = 12f;
    [SerializeField, Range(0f, 1f)] private float _fillAmount = 1f;

    [Header("Rotation")]
    [Tooltip("Degrees clockwise from the top where the arc begins.")]
    [SerializeField] private float _arcOffset = 0f;

    [Header("Quality")]
    [Tooltip("Number of quad segments around the ring. 64 is smooth for most sizes.")]
    [SerializeField, Range(8, 256)] private int _segments = 64;

    // ── Public properties (call SetX helpers to trigger a mesh rebuild) ───────

    public float Radius
    {
        get => _radius;
        set { _radius = Mathf.Max(1f, value); SetVerticesDirty(); }
    }

    public float Thickness
    {
        get => _thickness;
        set { _thickness = Mathf.Max(0.1f, value); SetVerticesDirty(); }
    }

    /// <summary>0 = empty, 1 = full ring.</summary>
    public float FillAmount
    {
        get => _fillAmount;
        set { _fillAmount = Mathf.Clamp01(value); SetVerticesDirty(); }
    }

    /// <summary>Clockwise degrees from the top (12 o'clock) where the arc starts.</summary>
    public float ArcOffset
    {
        get => _arcOffset;
        set { _arcOffset = value; SetVerticesDirty(); }
    }

    public int Segments
    {
        get => _segments;
        set { _segments = Mathf.Clamp(value, 8, 256); SetVerticesDirty(); }
    }

    // ── Mesh generation ───────────────────────────────────────────────────────

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (_fillAmount <= 0f || _thickness <= 0f || _radius <= 0f)
            return;

        float outerR = _radius;
        float innerR = Mathf.Max(0f, _radius - _thickness);

        // Arc spans fillAmount * 360 degrees, starting at arcOffset (clockwise from top).
        float totalDeg = _fillAmount * 360f;
        int segs = Mathf.Max(1, Mathf.CeilToInt(_segments * _fillAmount));

        // Unity UI uses a coordinate system where +Y is up.
        // "Top" = angle 90° in standard math convention.
        // We subtract because we want clockwise winding.
        float startRad = Mathf.Deg2Rad * (90f - _arcOffset);
        float stepRad = -Mathf.Deg2Rad * (totalDeg / segs);   // negative = clockwise

        UIVertex v = UIVertex.simpleVert;
        v.color = color;

        for (int i = 0; i <= segs; i++)
        {
            float angle = startRad + stepRad * i;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            // UV: u maps inner→outer (0→1), v maps along the arc (0→1)
            float uvV = (float)i / segs;

            // Outer vertex
            v.position = new Vector3(cos * outerR, sin * outerR, 0f);
            v.uv0 = new Vector2(1f, uvV);
            vh.AddVert(v);

            // Inner vertex
            v.position = new Vector3(cos * innerR, sin * innerR, 0f);
            v.uv0 = new Vector2(0f, uvV);
            vh.AddVert(v);
        }

        // Two vertices per ring step, build quads between consecutive steps.
        for (int i = 0; i < segs; i++)
        {
            int baseIdx = i * 2;
            // Quad: outerA, innerA, innerB, outerB  (clockwise winding)
            vh.AddTriangle(baseIdx, baseIdx + 1, baseIdx + 3);
            vh.AddTriangle(baseIdx, baseIdx + 3, baseIdx + 2);
        }
    }

    // ── Editor helpers ────────────────────────────────────────────────────────

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
