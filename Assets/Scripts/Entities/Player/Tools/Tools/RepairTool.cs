using UnityEngine;

[CreateAssetMenu(fileName = "RepairTool", menuName = "Player/Tools/RepairTool")]
public class RepairTool : PlayerTool
{
    [SerializeField] private int structureRestored = 1;

    public override void OnEnd(ActiveTool tool, ToolEndReason reason)
    {
        if (reason == ToolEndReason.Expired)
            tool.player.Health.RestoreSegments(structureRestored);
    }
}