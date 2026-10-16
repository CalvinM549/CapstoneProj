using UnityEngine;

// Shared string helpers so every terminal screen formats values the same way.
public static class TerminalFormat
{
    public const string EmptyTime = "--:--";

    public static string Duration(float seconds, bool hours = false)
    {
        if (seconds <= 0f) return EmptyTime;

        int total = Mathf.FloorToInt(seconds);
        return hours
            ? $"{total / 3600:00}:{total / 60 % 60:00}:{total % 60:00}"
            : $"{total / 60:00}:{total % 60:00}";
    }

    // [||||----] style bar. normalized is clamped to 0..1
    public static string Bar(float normalized, int length, string filled = "|", string empty = "-")
    {
        length = Mathf.Max(1, length);
        int count = Mathf.RoundToInt(Mathf.Clamp01(normalized) * length);

        var sb = new System.Text.StringBuilder(length * Mathf.Max(filled.Length, empty.Length));
        for (int i = 0; i < length; i++)
            sb.Append(i < count ? filled : empty);

        return sb.ToString();
    }

    // "LABEL ........ VALUE" padded to a fixed character width (use with a monospace font)
    public static string Leader(string label, string value, int width, char fill = '.')
    {
        int dots = Mathf.Max(2, width - label.Length - value.Length - 2);
        return $"{label} {new string(fill, dots)} {value}";
    }

    public static string Header(string title) => $"// {title}";

    // Matches the HUD's CurrencyUI ("$[120]")
    public static string Currency(int amount) => $"$[{amount}]";

    public static string Colour(string text, Color colour) => $"<color=#{ColorUtility.ToHtmlStringRGBA(colour)}>{text}</color>";
}
