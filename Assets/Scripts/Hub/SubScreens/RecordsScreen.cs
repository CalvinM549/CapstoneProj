using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

// Lifetime service record for the active profile, typed out as a dotted read-out.
public class RecordsScreen : UIScreen
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TypewriterBox readout;

    [Tooltip("Total characters per line (label + dots + value). Tune to the readout box width.")]
    [SerializeField] private int lineWidth = 30;

    protected override void OnBeforeOpen(object payload)
    {
        var profile = GameManager.Instance.ActiveProfile;
        if (profile == null) return;

        if (titleText != null) titleText.text = TerminalFormat.Header($"SERVICE RECORD :: {profile.profileName}");
        if (readout != null) readout.SetText(BuildReadout(profile));
    }

    private string BuildReadout(PlayerProfile p)
    {
        var db = GameManager.Database;

        int clearRate = p.runsAttempted > 0 ? Mathf.RoundToInt(100f * p.runVictoryCount / p.runsAttempted) : 0;

        int gearTotal = db.weapons.All.Count(w => w != null && w.ListInUnlocks) + db.tools.All.Count(t => t != null && t.ListInUnlocks);
        var meta = GameManager.Instance.Meta;
        int gearOwned = db.weapons.All.Count(w => w != null && w.ListInUnlocks && meta.IsUnlocked(w))
                      + db.tools.All.Count(t => t != null && t.ListInUnlocks && meta.IsUnlocked(t));

        var sb = new StringBuilder();
        Line(sb, "DELVES ATTEMPTED", p.runsAttempted.ToString());
        Line(sb, "SECTORS CLEARED", p.runVictoryCount.ToString());
        Line(sb, "SIGNAL LOST", p.runFailureCount.ToString());
        Line(sb, "CLEAR RATE", $"{clearRate}%");
        Line(sb, "BEST CLEAR", TerminalFormat.Duration(p.bestRunTime));
        Line(sb, "TIME DEPLOYED", TerminalFormat.Duration(p.totalTimeDeployed, hours: true));
        sb.AppendLine();
        Line(sb, "HOSTILES DESTROYED", p.totalEnemiesKilled.ToString());
        Line(sb, "DEEPEST DEPTH", p.deepestDepth.ToString());
        sb.AppendLine();
        Line(sb, "SHARDS HELD", p.metaCurrency.ToString());
        Line(sb, "LOGS RECOVERED", $"{p.unlockedArchives.Count}/{db.archives.All.Count}");
        Line(sb, "GEAR UNLOCKED", $"{gearOwned}/{gearTotal}");

        return sb.ToString().TrimEnd();
    }

    private void Line(StringBuilder sb, string label, string value) => sb.AppendLine(TerminalFormat.Leader(label, value, lineWidth));

    public void OnBackClicked() => UIManager.Instance.CloseTop();
}
