using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Draws a SectorMap inside this RectTransform: depth runs left -> right, rows spread vertically.
// Nodes reveal column by column like a terminal scan.
public class SectorMapView : MonoBehaviour
{
    [Serializable]
    public class TypeStyle
    {
        public RoomTypeData type;
        public string glyph = "?";
        public Color colour = Color.white;
        public bool useTypeIcon = true;
    }

    [Header("Refs")]
    [SerializeField] private RectTransform lineLayer;   // drawn under nodes
    [SerializeField] private RectTransform nodeLayer;
    [SerializeField] private SectorMapNodeUI nodePrefab;

    [Header("Layout")]
    [SerializeField] private float nodeSize = 11f;
    [SerializeField] private float lineThickness = 1f;
    [SerializeField] private Vector2 padding = new(10f, 10f);

    [Header("Style")]
    [SerializeField] private Color lineColour = new(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color defaultNodeColour = new(0.75f, 0.75f, 0.75f);
    [SerializeField] private TypeStyle[] typeStyles = Array.Empty<TypeStyle>();

    [Header("Reveal")]
    [SerializeField] private float columnRevealDelay = 0.05f;
    [SerializeField] private string revealSfx = "TypeTick";

    public event Action<MapNode> OnNodeHovered;
    public event Action<MapNode> OnNodeUnhovered;

    private readonly List<SectorMapNodeUI> nodes = new();
    private readonly List<Image> lines = new();

    // depth -> objects to reveal at that column
    private readonly Dictionary<int, List<GameObject>> columns = new();
    private Coroutine revealRoutine;

    private RectTransform Rect => (RectTransform)transform;

    private void Awake()
    {
        if (nodeLayer == null) nodeLayer = Rect;
        if (lineLayer == null) lineLayer = Rect;
    }

    private void OnDisable()
    {
        // Coroutines die with the object; make sure nothing is left half-revealed
        if (revealRoutine != null) { RevealAll(); revealRoutine = null; }
    }

    public void Build(SectorMap map, bool animate = true)
    {
        Clear();
        if (map == null || map.nodes.Count == 0) return;

        var rowSizes = new Dictionary<int, int>();
        int maxRowSize = 1;
        foreach (var n in map.nodes)
        {
            rowSizes[n.depth] = rowSizes.GetValueOrDefault(n.depth) + 1;
            maxRowSize = Mathf.Max(maxRowSize, rowSizes[n.depth]);
        }

        int depthCount = Mathf.Max(1, map.rowCount);
        Vector2 area = Rect.rect.size - padding * 2f;
        float stepX = depthCount > 1 ? area.x / (depthCount - 1) : 0f;
        float stepY = maxRowSize > 1 ? area.y / (maxRowSize - 1) : 0f;

        Vector2 PositionOf(MapNode n)
        {
            float x = -area.x * 0.5f + n.depth * stepX;
            float y = ((rowSizes[n.depth] - 1) * 0.5f - n.rowIndex) * stepY;
            return new Vector2(x, y);
        }

        foreach (var n in map.nodes)
        {
            Vector2 from = PositionOf(n);
            foreach (var exit in n.exits)
                AddToColumn(n.depth, CreateLine(from, PositionOf(exit)).gameObject);
        }

        foreach (var n in map.nodes)
        {
            var style = FindStyle(n.type);
            var node = Instantiate(nodePrefab, nodeLayer);
            node.Rect.anchoredPosition = PositionOf(n);
            node.Setup(n,
                style != null && !style.useTypeIcon ? null : n.type?.displayIcon,
                style?.glyph ?? DefaultGlyph(n.type),
                style?.colour ?? defaultNodeColour,
                nodeSize,
                hovered => OnNodeHovered?.Invoke(hovered),
                unhovered => OnNodeUnhovered?.Invoke(unhovered));

            nodes.Add(node);
            AddToColumn(n.depth, node.gameObject);
        }

        if (animate && isActiveAndEnabled && columnRevealDelay > 0f)
        {
            foreach (var column in columns.Values)
                foreach (var go in column) go.SetActive(false);

            revealRoutine = StartCoroutine(RevealRoutine(depthCount));
        }
    }

    public void Clear()
    {
        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        foreach (var n in nodes) if (n != null) Destroy(n.gameObject);
        foreach (var l in lines) if (l != null) Destroy(l.gameObject);

        nodes.Clear();
        lines.Clear();
        columns.Clear();
    }

    #region Helpers

    private IEnumerator RevealRoutine(int depthCount)
    {
        for (int depth = 0; depth < depthCount; depth++)
        {
            if (columns.TryGetValue(depth, out var column))
                foreach (var go in column) go.SetActive(true);

            if (AudioManager.Instance != null && !string.IsNullOrEmpty(revealSfx))
                AudioManager.Instance.PlaySFX(revealSfx, Vector3.zero);

            yield return new WaitForSecondsRealtime(columnRevealDelay);
        }

        revealRoutine = null;
    }

    private void RevealAll()
    {
        foreach (var column in columns.Values)
            foreach (var go in column) if (go != null) go.SetActive(true);
    }

    private void AddToColumn(int depth, GameObject go)
    {
        if (!columns.TryGetValue(depth, out var list))
            columns[depth] = list = new List<GameObject>();

        list.Add(go);
    }

    private Image CreateLine(Vector2 from, Vector2 to)
    {
        var go = new GameObject("Link", typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(lineLayer, false);

        Vector2 delta = to - from;
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = from;
        rt.sizeDelta = new Vector2(delta.magnitude, lineThickness);
        rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

        var image = go.GetComponent<Image>();
        image.color = lineColour;
        image.raycastTarget = false;

        lines.Add(image);
        return image;
    }

    public string GlyphFor(RoomTypeData type) => FindStyle(type)?.glyph ?? DefaultGlyph(type);

    private TypeStyle FindStyle(RoomTypeData type)
    {
        if (type == null) return null;
        foreach (var s in typeStyles)
            if (s != null && s.type == type) return s;
        return null;
    }

    private static string DefaultGlyph(RoomTypeData type)
    {
        if (type == null) return "?";
        if (type.isStart) return "S";
        if (type.isEnd) return "X";
        if (type.isShop) return "$";
        return string.IsNullOrEmpty(type.displayName) ? "?" : type.displayName.Substring(0, 1).ToUpperInvariant();
    }

    #endregion
}
