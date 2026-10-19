using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// Shared victory / failure screen. Types the narrative sequence line by line, then builds and types
// a telemetry read-out from the RunState. First press skips the typing, second returns to the hub.
public class RunEndScreen : UIScreen
{
    [Header("Outcome")]
    [SerializeField] private bool victory;
    [SerializeField] private TerminalSequenceData narrative;

    [Header("Displays")]
    [SerializeField] private TerminalDisplay narrativeDisplay;
    [SerializeField] private TerminalDisplay statsDisplay;
    [SerializeField] private TextMeshProUGUI headerInfo;

    [Header("Telemetry")]
    [Tooltip("Total characters per stat line (label + dots + value).")]
    [SerializeField] private int lineWidth = 30;
    [SerializeField] private float statLineDelay = 0.12f;
    [SerializeField] private string telemetryHeader = "// RUN TELEMETRY";
    [SerializeField] private string compilingLine = "> COMPILING TELEMETRY";
    [SerializeField] private string closingLine = "> TRANSMITTING TO CORE";

    [Header("Action Button")]
    [SerializeField] private TerminalButton actionButton;
    [SerializeField] private string skipLabel = "SKIP";
    [SerializeField] private string returnLabel = "RETURN TO CORE";

    private RunState run;
    private TerminalSequenceData statsSequence;
    private bool finished;

    public override void Initialize()
    {
        if (narrativeDisplay != null) narrativeDisplay.onSequenceComplete.AddListener(StartStats);
        if (statsDisplay != null) statsDisplay.onSequenceComplete.AddListener(HandleStatsComplete);
    }

    protected override void OnBeforeOpen(object payload)
    {
        run = payload as RunState;
        finished = false;

        if (narrativeDisplay != null) narrativeDisplay.Clear();
        if (statsDisplay != null) statsDisplay.Clear();
        if (actionButton != null) actionButton.SetLabel(skipLabel);
        if (headerInfo != null) headerInfo.text = run != null ? $"SEED // {run.Seed}" : string.Empty;
    }

    protected override void OnOpened()
    {
        if (narrativeDisplay != null && narrative != null)
            narrativeDisplay.Play(narrative);
        else
            StartStats();
    }

    private void OnDestroy()
    {
        if (statsSequence != null) Destroy(statsSequence);
    }

    #region Buttons

    public void OnActionClicked()
    {
        if (!finished)
        {
            // Skip: finish the narrative (which starts the stats), then finish the stats
            if (narrativeDisplay != null && narrativeDisplay.IsPlaying) narrativeDisplay.Complete();
            if (statsDisplay != null && statsDisplay.IsPlaying) statsDisplay.Complete();
            return;
        }

        if (RunManager.Instance != null) RunManager.Instance.ReturnToHub();
        else SceneLoader.Instance.LoadHub();
    }

    public override void HandleCancel() => OnActionClicked();

    #endregion

    #region Telemetry

    private void StartStats()
    {
        if (statsDisplay == null || run == null)
        {
            HandleStatsComplete();
            return;
        }

        if (statsSequence != null) Destroy(statsSequence);
        statsSequence = BuildStatsSequence(run);
        statsDisplay.Play(statsSequence);
    }

    private void HandleStatsComplete()
    {
        finished = true;
        if (actionButton != null) actionButton.SetLabel(returnLabel);
    }

    private TerminalSequenceData BuildStatsSequence(RunState r)
    {
        var lines = new List<TerminalLine>
        {
            new() { text = telemetryHeader, lineType = TerminalLineType.Instant, delayOverride = 0.1f },
            new() { text = compilingLine, lineType = TerminalLineType.Loading, loadingDuration = 0.6f, dotCycleSpeed = 0.15f, loadingResultSuffix = "... DONE", delayOverride = 0.15f },
        };

        void Stat(string label, string value) => lines.Add(new TerminalLine
        {
            text = TerminalFormat.Leader(label, value, lineWidth),
            lineType = TerminalLineType.Instant,
            delayOverride = statLineDelay
        });

        int maxDepth = Mathf.Max(0, r.map.rowCount - 1);
        var player = r.player;

        Stat("TIME ELAPSED", TerminalFormat.Duration(r.runDurationTimer));
        Stat("DEPTH REACHED", $"{r.currentDepth:00}/{maxDepth:00}");
        Stat("SECTORS TRAVERSED", r.pathTaken.Count.ToString());
        Stat("HOSTILES DESTROYED", r.EnemiesKilled.ToString());
        Stat("DAMAGE TAKEN", r.DamageTaken.ToString());
        Stat("FUNDS HELD", TerminalFormat.Currency(r.currency));
        Stat("SHARD CACHES", $"+{r.shardsFromSectors}");
        Stat("RECOVERY PAYOUT", $"+{r.shardsFromPayout}");
        Stat("SHARDS BANKED", r.ShardsEarned.ToString());

        if (player != null)
        {
            Stat("UPGRADES INSTALLED", player.Upgrades.AllActiveUpgrades.Count().ToString());
            Stat("WEAPON", player.Combat.HasWeaponEquipped ? player.Combat.EquippedWeapon.data.Name.ToUpperInvariant() : "NONE");
        }

        var profile = GameManager.Instance != null ? GameManager.Instance.ActiveProfile : null;
        if (victory && profile != null && Mathf.Approximately(profile.bestRunTime, r.runDurationTimer))
            lines.Add(new TerminalLine { text = "> NEW BEST CLEAR RECORDED", lineType = TerminalLineType.Typewriter, delayOverride = 0.1f });

        lines.Add(new TerminalLine { text = closingLine, lineType = TerminalLineType.Typewriter, delayOverride = 0f });

        var seq = ScriptableObject.CreateInstance<TerminalSequenceData>();
        seq.name = "RuntimeTelemetry";
        seq.startDelay = 0.2f;
        seq.defaultLineDelay = statLineDelay;
        seq.lines = lines.ToArray();
        return seq;
    }

    #endregion
}
