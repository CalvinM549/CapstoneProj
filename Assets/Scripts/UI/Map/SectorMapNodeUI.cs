using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One node in a SectorMapView. Shows the room type icon if it has one, otherwise a text glyph.
public class SectorMapNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI glyph;

    [SerializeField] private float hoverScale = 1.4f;

    private Action<MapNode> onHover;
    private Action<MapNode> onExit;

    public MapNode Node { get; private set; }
    public RectTransform Rect => (RectTransform)transform;

    public void Setup(MapNode node, Sprite iconSprite, string glyphText, Color colour, float size, Action<MapNode> onHover, Action<MapNode> onExit)
    {
        Node = node;
        this.onHover = onHover;
        this.onExit = onExit;

        Rect.sizeDelta = new Vector2(size, size);
        transform.localScale = Vector3.one;

        bool useIcon = icon != null && iconSprite != null;

        if (icon != null)
        {
            icon.gameObject.SetActive(useIcon);
            icon.sprite = iconSprite;
            icon.color = colour;
        }

        if (glyph != null)
        {
            glyph.gameObject.SetActive(!useIcon);
            glyph.text = glyphText;
            glyph.color = colour;
        }

        if (background != null)
        {
            Color bg = colour;
            bg.a = background.color.a;
            background.color = bg;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * hoverScale;
        onHover?.Invoke(Node);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
        onExit?.Invoke(Node);
    }
}
