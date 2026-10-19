using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Pre-delve briefing: sector map preview, seed control, loadout summary, deploy.
public class RunPrepScreen : UIScreen
{
    [Header("Map")]
    [SerializeField] private SectorMapView mapView;
    [SerializeField] private TextMeshProUGUI mapSummaryText;
    [SerializeField] private TextMeshProUGUI nodeInfoText;
    [SerializeField] private string nodeInfoIdle = "> HOVER A NODE TO SCAN";

    [Header("Seed")]
    [SerializeField] private TextMeshProUGUI seedText;
    [SerializeField] private TMP_InputField seedInput;

    [Header("Loadout")]
    [SerializeField] private TextMeshProUGUI loadoutText;

    [Header("Actions")]
    [SerializeField] private Button deployButton;

    public override void Initialize()
    {
        base.Initialize();

        HubManager.Instance.OnPendingMapChanged += HandleMapChanged;
        HubManager.Instance.OnPendingLoadoutChanged += HandleLoadoutChanged;

        if (mapView != null)
        {
            mapView.OnNodeHovered += ShowNodeInfo;
            mapView.OnNodeUnhovered += _ => SetNodeInfo(nodeInfoIdle);
        }

        if (seedInput != null)
            seedInput.onSubmit.AddListener(OnSeedSubmitted);
    }

    // Screens are disabled while closed, so unsubscribe on destroy rather than OnDisable
    private void OnDestroy()
    {
        if (HubManager.Instance == null) return;

        HubManager.Instance.OnPendingMapChanged -= HandleMapChanged;
        HubManager.Instance.OnPendingLoadoutChanged -= HandleLoadoutChanged;
    }

    protected override void OnBeforeOpen(object payload)
    {
        SetNodeInfo(nodeInfoIdle);
        RefreshMap(animate: true);
        RefreshLoadout();
    }

    // Returning from the loadout screen
    protected override void OnFocusRegained() => RefreshLoadout();

    #region Buttons

    public void OnRerollClicked() => HubManager.Instance.RerollSeed();

    public void OnApplySeedClicked()
    {
        if (seedInput != null) OnSeedSubmitted(seedInput.text);
    }

    public void OnLoadoutClicked() => UIManager.Instance.OpenScreen("loadoutScreen");

    public void OnDeployClicked()
    {
        if (!HubManager.Instance.CanBeginRun) return;

        SetFocused(false);
        HubManager.Instance.BeginNewRunFromHub();
    }

    public void OnBackClicked() => UIManager.Instance.CloseTop();

    #endregion

    private void OnSeedSubmitted(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        HubManager.Instance.SetCustomSeed(input);
        seedInput.text = string.Empty;
        seedInput.DeactivateInputField();
    }

    private void HandleMapChanged(SectorMap map)
    {
        if (gameObject.activeInHierarchy) RefreshMap(animate: true);
    }

    private void HandleLoadoutChanged(RunLoadout loadout)
    {
        if (gameObject.activeInHierarchy) RefreshLoadout();
    }

    private void RefreshMap(bool animate)
    {
        var hub = HubManager.Instance;
        var map = hub.PendingMap;

        if (seedText != null) seedText.text = $"SEED // {hub.PendingSeed}";
        if (mapView != null) mapView.Build(map, animate);
        if (mapSummaryText != null) mapSummaryText.text = BuildSummary(map);

        RefreshDeploy();
    }

    private void RefreshLoadout()
    {
        var loadout = HubManager.Instance.PendingLoadout;

        if (loadoutText != null)
        {
            string weapon = loadout?.weapon != null ? loadout.weapon.Name.ToUpperInvariant() : "<NONE>";
            string protocol = loadout?.modifier != null ? loadout.modifier.Name.ToUpperInvariant() : "NONE";
            loadoutText.text = $"WEAPON   // {weapon}\nPROTOCOL // {protocol}";
        }

        RefreshDeploy();
    }

    private void RefreshDeploy()
    {
        if (deployButton != null) deployButton.interactable = HubManager.Instance.CanBeginRun;
    }

    private void ShowNodeInfo(MapNode node)
    {
        string type = node.type != null ? node.type.displayName.ToUpperInvariant() : "UNKNOWN";
        string code = string.IsNullOrEmpty(node.sectorCode) ? "" : $" // {node.sectorCode.ToUpperInvariant()}";
        string shard = node.grantShard ? " // SHARD CACHE" : "";
        SetNodeInfo($"> DEPTH {node.depth:00} // {type}{code}{shard}");
    }

    private void SetNodeInfo(string text)
    {
        if (nodeInfoText != null) nodeInfoText.text = text;
    }

    // "18 NODES // DEPTH 9" + a per-type breakdown using the map's glyphs, e.g. "C6 E2 $1 +1"
    private string BuildSummary(SectorMap map)
    {
        if (map == null) return "NO SECTOR DATA";

        var breakdown = map.nodes
            .Where(n => n.type != null && !n.type.isStart && !n.type.isEnd)
            .GroupBy(n => n.type)
            .OrderByDescending(g => g.Count())
            .Select(g => $"{(mapView != null ? mapView.GlyphFor(g.Key) : g.Key.displayName)}{g.Count()}");

        return $"{map.totalNodes} NODES // DEPTH {map.rowCount}\n{string.Join("  ", breakdown)}";
    }
}
