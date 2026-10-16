using System;
using UnityEngine;

// Numeric row rendered as an ASCII bar:  [||||||--] 075%
public class TerminalSlider : TerminalValueRow
{
    public enum DisplayMode
    {
        Percent,    // shows position in range as 0-100%
        Integer,    // shows the raw value rounded
        Decimal     // shows the raw value to 2dp
    }

    [Header("Range")]
    [SerializeField] private float min = 0f;
    [SerializeField] private float max = 1f;
    [SerializeField] private float step = 0.1f;
    [SerializeField] private float value = 1f;

    [Header("Display")]
    [SerializeField] private DisplayMode display = DisplayMode.Percent;
    [SerializeField] private int barLength = 8;
    [SerializeField] private string filledChar = "|";
    [SerializeField] private string emptyChar = "-";

    public float Value => value;
    public float Normalized => Mathf.InverseLerp(min, max, value);

    public event Action<float> OnValueChanged;

    public void SetValueWithoutNotify(float newValue)
    {
        value = Snap(newValue);
        Refresh();
    }

    public void SetValue(float newValue)
    {
        float snapped = Snap(newValue);
        if (Mathf.Approximately(snapped, value)) return;

        value = snapped;
        Refresh();
        OnValueChanged?.Invoke(value);
    }

    protected override bool TryStep(int direction)
    {
        float next = Snap(value + direction * step);
        if (Mathf.Approximately(next, value)) return false;

        value = next;
        OnValueChanged?.Invoke(value);
        return true;
    }

    protected override string FormatValue()
    {
        string number = display switch
        {
            DisplayMode.Percent => $"{Mathf.RoundToInt(Normalized * 100f),3}%",
            DisplayMode.Integer => $"{Mathf.RoundToInt(value),3}",
            _ => value.ToString("0.00")
        };

        return $"[{TerminalFormat.Bar(Normalized, barLength, filledChar, emptyChar)}] {number}";
    }

    private float Snap(float v)
    {
        v = Mathf.Clamp(v, min, max);
        if (step > 0f)
            v = min + Mathf.Round((v - min) / step) * step;

        return Mathf.Clamp(v, min, max);
    }

    private void OnValidate()
    {
        if (max < min) max = min;
        value = Mathf.Clamp(value, min, max);
    }
}
