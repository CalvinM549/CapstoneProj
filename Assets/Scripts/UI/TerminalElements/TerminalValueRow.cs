using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Base for a settings-style row:  > LABEL_          < [||||----] 050% >
// Lives on the same GameObject as a Button + TerminalButton (which draws the label hover/cursor).
// Left/right (keyboard/gamepad), the mouse wheel, and the optional arrow buttons step the value.
// Clicking the row itself calls Submit (choices cycle, sliders ignore it by default).
public abstract class TerminalValueRow : MonoBehaviour, IMoveHandler, IScrollHandler
{
    [Header("Row Refs")]
    [SerializeField] protected Button row;
    [SerializeField] protected TerminalButton label;
    [SerializeField] protected TextMeshProUGUI valueText;
    [SerializeField] protected Button decrementButton;
    [SerializeField] protected Button incrementButton;

    [Header("Feedback")]
    [SerializeField] private string stepSfx = "UItick";

    public bool Interactable => row == null || row.interactable;

    protected virtual void Awake()
    {
        if (row == null) row = GetComponent<Button>();
        if (label == null) label = GetComponent<TerminalButton>();

        if (row != null) row.onClick.AddListener(Submit);
        if (decrementButton != null) decrementButton.onClick.AddListener(() => Step(-1));
        if (incrementButton != null) incrementButton.onClick.AddListener(() => Step(1));
    }

    protected virtual void OnEnable() => Refresh();

    public void SetLabel(string text)
    {
        if (label != null) label.SetLabel(text);
    }

    public void SetInteractable(bool value)
    {
        if (row != null) row.interactable = value;
        if (decrementButton != null) decrementButton.interactable = value;
        if (incrementButton != null) incrementButton.interactable = value;
    }

    public void OnMove(AxisEventData eventData)
    {
        if (!Interactable) return;

        if (eventData.moveDir == MoveDirection.Left) Step(-1);
        else if (eventData.moveDir == MoveDirection.Right) Step(1);
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (!Interactable) return;

        if (eventData.scrollDelta.y > 0f) Step(1);
        else if (eventData.scrollDelta.y < 0f) Step(-1);
    }

    protected void Step(int direction)
    {
        if (!Interactable) return;
        if (!TryStep(direction)) return;

        Refresh();

        if (AudioManager.Instance != null && !string.IsNullOrEmpty(stepSfx))
            AudioManager.Instance.PlaySFX(stepSfx, Vector3.zero);
    }

    public void Refresh()
    {
        if (valueText != null) valueText.text = FormatValue();
    }

    protected virtual void Submit() { }

    // Apply a step in the given direction. Return false if nothing changed (at a limit, etc).
    protected abstract bool TryStep(int direction);
    protected abstract string FormatValue();
}
