using TMPro;
using UnityEngine;

// Read-out pane paired with a list (loadout gear, unlocks, archive logs).
// Body text can optionally be typed out with the project's TypewriterBox.
public class TerminalDetailPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI footerText;

    [Tooltip("Optional. If set (on the body text object), body text is typed out instead of shown instantly.")]
    [SerializeField] private TypewriterBox bodyTypewriter;

    [Header("Empty State")]
    [SerializeField] private string emptyTitle = "// NO SELECTION";
    [SerializeField] [TextArea] private string emptyBody = "> HOVER AN ENTRY TO INSPECT";

    private string lastShownKey;

    public void Show(string title, string body, string footer = null)
    {
        string key = title + "|" + body + "|" + footer;
        if (key == lastShownKey) return; // avoid re-typing when hovering the same entry
        lastShownKey = key;

        if (titleText != null) titleText.text = title ?? string.Empty;

        if (footerText != null)
        {
            footerText.gameObject.SetActive(!string.IsNullOrEmpty(footer));
            footerText.text = footer ?? string.Empty;
        }

        SetBody(body);
    }

    public void Clear()
    {
        lastShownKey = null;
        Show(emptyTitle, emptyBody);
    }

    private void SetBody(string body)
    {
        if (bodyText == null) return;

        // TypewriterBox ignores empty strings, so fall back to a direct set
        if (bodyTypewriter != null && bodyTypewriter.isActiveAndEnabled && !string.IsNullOrEmpty(body))
            bodyTypewriter.SetText(body);
        else
        {
            bodyText.text = body ?? string.Empty;
            bodyText.maxVisibleCharacters = 99999;
        }
    }
}
