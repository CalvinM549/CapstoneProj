using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One clickable line in a terminal list:  > SHOTGUN_            [EQUIPPED]
// Built as a variant of the GenericButton prefab: Button + TerminalButton on the root, plus a right-aligned tag.
// Screens fill it via Setup() and get click + hover callbacks back.
[RequireComponent(typeof(Button))]
public class TerminalListItem : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    [SerializeField] private Button button;
    [SerializeField] private TerminalButton label;
    [SerializeField] private TextMeshProUGUI tagText;

    [Header("Tag Colours")]
    [SerializeField] private Color tagColour = new(0.51f, 0.51f, 0.51f);
    [SerializeField] private Color highlightTagColour = Color.white;

    private Action onClick;
    private Action onHover;

    // Whatever the owning screen wants to associate with this row (weapon, archive entry, etc)
    public object Data { get; private set; }

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (label == null) label = GetComponent<TerminalButton>();

        button.onClick.AddListener(() => onClick?.Invoke());
    }

    public TerminalListItem Setup(string text, string tag = null, Action onClick = null, Action onHover = null, object data = null, bool interactable = true)
    {
        label.SetLabel(text);
        SetTag(tag);

        button.interactable = interactable;

        this.onClick = onClick;
        this.onHover = onHover;
        Data = data;

        return this;
    }

    public void SetTag(string tag, bool highlight = false)
    {
        if (tagText == null) return;

        bool show = !string.IsNullOrEmpty(tag);
        tagText.gameObject.SetActive(show);
        tagText.text = show ? tag : string.Empty;
        tagText.color = highlight ? highlightTagColour : tagColour;
    }

    public void OnPointerEnter(PointerEventData eventData) => onHover?.Invoke();

    public void OnSelect(BaseEventData eventData) => onHover?.Invoke();
}
