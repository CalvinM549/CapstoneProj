using System;
using UnityEngine;

// Option row rendered as  < VALUE >  — use for toggles (OFF/ON) or short lists (quality presets).
// Clicking the row cycles forward; left/right step either way.
public class TerminalChoice : TerminalValueRow
{
    [Header("Options")]
    [SerializeField] private string[] options = { "OFF", "ON" };
    [SerializeField] private int index;
    [SerializeField] private bool wrap = true;

    [Tooltip("{0} = current option. Use \"< {0} >\" if this row has no arrow buttons.")]
    [SerializeField] private string valueFormat = "{0}";

    public int Index => index;
    public bool BoolValue => index != 0; // for OFF/ON style choices
    public string Current => options != null && options.Length > 0 ? options[index] : string.Empty;

    public event Action<int> OnIndexChanged;

    public void SetOptions(string[] newOptions, int newIndex = 0)
    {
        options = newOptions ?? Array.Empty<string>();
        index = options.Length == 0 ? 0 : Mathf.Clamp(newIndex, 0, options.Length - 1);
        Refresh();
    }

    public void SetIndexWithoutNotify(int newIndex)
    {
        index = options.Length == 0 ? 0 : Mathf.Clamp(newIndex, 0, options.Length - 1);
        Refresh();
    }

    public void SetBoolWithoutNotify(bool value) => SetIndexWithoutNotify(value ? 1 : 0);

    protected override void Submit() => Step(1);

    protected override bool TryStep(int direction)
    {
        int count = options?.Length ?? 0;
        if (count < 2) return false;

        int next = wrap
            ? (index + direction + count) % count
            : Mathf.Clamp(index + direction, 0, count - 1);

        if (next == index) return false;

        index = next;
        OnIndexChanged?.Invoke(index);
        return true;
    }

    protected override string FormatValue() => string.Format(valueFormat, options == null || options.Length == 0 ? "--" : options[index]);
}
