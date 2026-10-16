using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

// Maintenance bay: pick a service on the left, watch the frame diagnostics and service log update live on the right.
// Runs on unscaled time because the screen pauses the game.
public class RestUIScreen : UIScreen
{
    [Serializable]
    public class ActionDisplay
    {
        public RestAction action;
        public string label = "SERVICE";
        [TextArea] public string description;
        public string processingText = "SERVICING";
    }

    [Header("Actions")]
    [SerializeField] private TerminalListItem itemPrefab;
    [SerializeField] private Transform actionContainer;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private ActionDisplay[] actions =
    {
        new() { action = RestAction.RepairStructure, label = "REPAIR STRUCTURE", description = "Rebuild one structure segment.", processingText = "REPAIRING STRUCTURE" },
        new() { action = RestAction.RestoreAmmo, label = "RESTOCK AMMO", description = "Reload the drone's ammunition reserve.", processingText = "RESTOCKING AMMUNITION" },
        new() { action = RestAction.FullRestore, label = "FULL SERVICE", description = "Restore all structure and ammunition.", processingText = "RUNNING FULL SERVICE" },
    };

    [Header("Readouts")]
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI diagnosticsText;
    [SerializeField] private TextMeshProUGUI logText;

    [Header("Timing (unscaled)")]
    [SerializeField] private float processingDuration = 0.7f;
    [SerializeField] private float animateDuration = 0.5f;
    [SerializeField] private float logCharDelay = 0.012f;
    [SerializeField] private float dotCycle = 0.15f;
    [SerializeField] private int maxLogLines = 9;

    [Header("Format")]
    [SerializeField] private int barLength = 10;
    [SerializeField] private int labelWidth = 10;
    [SerializeField] private string freeTag = "FREE";

    [Header("Feedback")]
    [SerializeField] private string successSfx = "UItick";

    private RestStation rest;
    private bool processing;

    private readonly List<string> logLines = new();
    private RestSnapshot shown;

    protected override void OnBeforeOpen(object payload)
    {
        rest = (RestStation)payload;
        processing = false;

        shown = RestSnapshot.Capture(rest.Run);
        RenderDiagnostics(shown);

        logLines.Clear();
        AddLineInstant("> MAINTENANCE BAY ONLINE");
        AddLineInstant(rest.FreeActionAvaliable ? "> COMPLIMENTARY SERVICE AVAILABLE" : "> COMPLIMENTARY SERVICE USED");

        if (descriptionText != null) descriptionText.text = string.Empty;

        BuildActions();
    }

    protected override void OnClosed()
    {
        StopAllCoroutines();
        processing = false;
    }

    public void OnBackClicked()
    {
        if (processing) return;
        UIManager.Instance.CloseTop();
    }

    public override void HandleCancel() => OnBackClicked();

    #region Actions list

    private readonly List<TerminalListItem> items = new();

    private void BuildActions()
    {
        foreach (var item in items) if (item != null) Destroy(item.gameObject);
        items.Clear();

        foreach (var display in actions)
        {
            var d = display;
            var item = Instantiate(itemPrefab, actionContainer);
            item.Setup(d.label,
                onClick: () => HandleActionSelected(d),
                onHover: () => { if (descriptionText != null) descriptionText.text = $"> {d.description}"; },
                data: d);
            items.Add(item);
        }

        RefreshTags();
    }

    private void RefreshTags()
    {
        var run = rest.Run;
        foreach (var item in items)
        {
            var d = (ActionDisplay)item.Data;
            bool free = rest.IsFree(d.action);
            int cost = rest.GetCost(d.action);

            item.SetTag(free ? freeTag : TerminalFormat.Currency(cost), highlight: free || run.currency >= cost);
        }

        if (currencyText != null) currencyText.text = $"FUNDS {TerminalFormat.Currency(run.currency)}";
    }

    private void SetItemsInteractable(bool value)
    {
        foreach (var item in items)
            item.GetComponent<UnityEngine.UI.Button>().interactable = value;
    }

    private void HandleActionSelected(ActionDisplay display)
    {
        if (processing) return;
        StartCoroutine(ProcessRoutine(display));
    }

    #endregion

    #region Realtime processing

    private IEnumerator ProcessRoutine(ActionDisplay display)
    {
        processing = true;
        SetItemsInteractable(false);

        var result = rest.Perform(display.action);

        // "> REPAIRING STRUCTURE..." with animated dots
        yield return TypeLine($"> {display.processingText}");
        float wait = result.success ? processingDuration : processingDuration * 0.4f;
        yield return Dots(wait);

        if (result.success)
        {
            CompleteLastLine(" OK");
            yield return AnimateDiagnostics(result.before, result.after);

            if (result.after.segmentsIntact != result.before.segmentsIntact || !Mathf.Approximately(result.after.activeSegmentPercent, result.before.activeSegmentPercent))
                yield return TypeLine($"  STRUCTURE {result.before.segmentsIntact}/{result.before.segmentsTotal} -> {result.after.segmentsIntact}/{result.after.segmentsTotal}");

            if (result.after.ammo != result.before.ammo)
                yield return TypeLine($"  AMMO {result.before.ammo}/{result.before.maxAmmo} -> {result.after.ammo}/{result.after.maxAmmo}");

            yield return TypeLine(result.wasFree ? "  COST // COMPLIMENTARY" : $"  COST // -{TerminalFormat.Currency(result.cost)}");

            if (AudioManager.Instance != null && !string.IsNullOrEmpty(successSfx))
                AudioManager.Instance.PlaySFX(successSfx, Vector3.zero);
        }
        else
        {
            CompleteLastLine(" DENIED");
            yield return TypeLine(result.failReason == RestFailReason.InsufficientFunds
                ? $"  INSUFFICIENT FUNDS // NEED {TerminalFormat.Currency(rest.GetCost(display.action))}"
                : "  SYSTEMS ALREADY NOMINAL");
        }

        RefreshTags();
        SetItemsInteractable(true);
        processing = false;
    }

    private IEnumerator AnimateDiagnostics(RestSnapshot from, RestSnapshot to)
    {
        float t = 0f;
        while (t < animateDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / animateDuration));

            shown = new RestSnapshot
            {
                segmentsIntact = Mathf.RoundToInt(Mathf.Lerp(from.segmentsIntact, to.segmentsIntact, k)),
                segmentsTotal = to.segmentsTotal,
                activeSegmentPercent = Mathf.Lerp(from.activeSegmentPercent, to.activeSegmentPercent, k),
                ammo = Mathf.RoundToInt(Mathf.Lerp(from.ammo, to.ammo, k)),
                maxAmmo = to.maxAmmo,
                currency = Mathf.RoundToInt(Mathf.Lerp(from.currency, to.currency, k))
            };

            RenderDiagnostics(shown);
            yield return null;
        }

        shown = to;
        RenderDiagnostics(shown);
    }

    #endregion

    #region Rendering

    private void RenderDiagnostics(RestSnapshot s)
    {
        if (diagnosticsText == null) return;

        string segBar = new string('|', Mathf.Clamp(s.segmentsIntact, 0, s.segmentsTotal))
                      + new string('.', Mathf.Max(0, s.segmentsTotal - s.segmentsIntact));

        var sb = new StringBuilder();
        sb.AppendLine($"{Pad("STRUCTURE")}[{segBar}] {s.segmentsIntact}/{s.segmentsTotal}");
        sb.AppendLine($"{Pad("SEGMENT")}[{TerminalFormat.Bar(s.activeSegmentPercent, barLength)}] {Mathf.RoundToInt(s.activeSegmentPercent * 100f)}%");
        sb.AppendLine(s.maxAmmo > 0
            ? $"{Pad("AMMO")}[{TerminalFormat.Bar((float)s.ammo / s.maxAmmo, barLength)}] {s.ammo}/{s.maxAmmo}"
            : $"{Pad("AMMO")}NO WEAPON");
        sb.Append($"{Pad("FUNDS")}{TerminalFormat.Currency(s.currency)}");

        diagnosticsText.text = sb.ToString();
    }

    private string Pad(string label) => label.PadRight(labelWidth);

    private void AddLineInstant(string line)
    {
        logLines.Add(line);
        TrimLog();
        RenderLog();
    }

    private IEnumerator TypeLine(string line)
    {
        logLines.Add(string.Empty);
        TrimLog();

        for (int i = 1; i <= line.Length; i++)
        {
            logLines[^1] = line.Substring(0, i);
            RenderLog(cursor: true);
            yield return new WaitForSecondsRealtime(logCharDelay);
        }

        RenderLog();
    }

    private IEnumerator Dots(float duration)
    {
        string baseLine = logLines[^1];
        float t = 0f;
        int frame = 0;

        while (t < duration)
        {
            logLines[^1] = baseLine + new string('.', frame % 4);
            RenderLog(cursor: true);

            yield return new WaitForSecondsRealtime(dotCycle);
            t += dotCycle;
            frame++;
        }

        logLines[^1] = baseLine + "...";
    }

    private void CompleteLastLine(string suffix)
    {
        logLines[^1] += suffix;
        RenderLog();
    }

    private void TrimLog()
    {
        while (logLines.Count > maxLogLines) logLines.RemoveAt(0);
    }

    private void RenderLog(bool cursor = false)
    {
        if (logText == null) return;
        logText.text = string.Join("\n", logLines) + (cursor ? "_" : string.Empty);
    }

    #endregion
}
