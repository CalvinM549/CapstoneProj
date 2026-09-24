using UnityEngine;

public class RepairTool : PlayerTool
{
    [SerializeField] private int structureRestored;
    [SerializeField] private float momentumRequired;

    public override void UseTool(Vector2 direction)
    {
        p.Health.RestoreSegments(structureRestored);
    }

    public override bool CanUse()
    {
        return p.Momentum.PercentMomentum >= momentumRequired;
    }
}
