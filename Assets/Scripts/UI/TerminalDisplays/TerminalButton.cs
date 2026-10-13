using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent (typeof(UnityEngine.UI.Selectable))]
public class TerminalButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Selectable selectable;

    [SerializeField] private Color idleColour = new(0.75f, 0.85f, 0.8f);
    [SerializeField] private Color disabledColour = new(0.35f, 0.4f, 0.38f);
    [SerializeField] private Color hoverColour = Color.white;

    [SerializeField] private string hoverSfx = "UItick";
    [SerializeField] private float blinkRate = 0.45f;

    private bool lastInteractable = true;

    private string baseText;
    private bool hovered;
    private float blinkTimer;
    private bool cursorOn;

    private void Awake()
    {
        baseText = label.text;
        SetHover(false);
    }

    private void OnDisable()
    {
        SetHover(false);
    }

    private void Update()
    {
        if (selectable.interactable != lastInteractable)
        {
            lastInteractable = selectable.interactable;
            SetHover(false);
        }

        if (!hovered) return;

        blinkTimer += Time.unscaledDeltaTime;
        if (blinkTimer < blinkRate) return;

        blinkTimer = 0;
        cursorOn = !cursorOn;
        label.text = $"> {baseText}{(cursorOn ? "_" : "")}";
    }

    public void SetLabel(string text)
    {
        baseText = text;
        SetHover(hovered);
    }

    public void OnPointerEnter(PointerEventData eventData) => SetHover(true, playSound: true);

    public void OnPointerExit(PointerEventData eventData) => SetHover(false);

    public void OnSelect(BaseEventData eventData) => SetHover(true, playSound: true);

    public void OnDeselect(BaseEventData eventData) => SetHover(false);



    private void SetHover(bool value, bool playSound = false)
    {
        if (!selectable.interactable)
        {
            hovered = false;
            label.color = disabledColour;
            label.text = $" {baseText}";
            return;
        }

        hovered = value;
        blinkTimer = 0;
        cursorOn = true;

        label.color = value ? hoverColour : idleColour;
        label.text = value ? $"> {baseText}_" : $" {baseText}";

        if (playSound && value && AudioManager.Instance != null && !string.IsNullOrEmpty(hoverSfx))
        {
            AudioManager.Instance.PlaySFX(hoverSfx, Vector3.zero);
        }
    }
}
