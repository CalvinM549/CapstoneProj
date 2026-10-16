using System;
using UnityEngine;

public enum RestAction
{
    RepairStructure,
    RestoreAmmo,
    FullRestore
}

public enum RestFailReason
{
    None,
    InsufficientFunds,
    NoEffect        // already full, nothing to restore
}

// Frame state at a moment in time, so UI can show before -> after
public struct RestSnapshot
{
    public int segmentsIntact;
    public int segmentsTotal;
    public float activeSegmentPercent;
    public int ammo;
    public int maxAmmo;
    public int currency;

    public bool StructureFull => segmentsIntact >= segmentsTotal && activeSegmentPercent >= 0.999f;
    public bool AmmoFull => maxAmmo <= 0 || ammo >= maxAmmo;

    public static RestSnapshot Capture(RunState run)
    {
        var health = run.player.Health;
        var combat = run.player.Combat;

        int total = health.healthSegments.Count;
        int intact = total == 0 ? 0 : health.ActiveHealthIndex + (health.ActiveSegment.IsDestroyed ? 0 : 1);

        return new RestSnapshot
        {
            segmentsIntact = intact,
            segmentsTotal = total,
            activeSegmentPercent = total == 0 ? 0f : health.ActiveSegment.PercentHealth,
            ammo = combat.CurrentAmmo,
            maxAmmo = combat.MaxAmmo,
            currency = run.currency
        };
    }
}

public struct RestResult
{
    public RestAction action;
    public bool success;
    public bool wasFree;
    public int cost;
    public RestFailReason failReason;
    public RestSnapshot before;
    public RestSnapshot after;
}

public class RestStation : StationBase
{

    [SerializeField] private int costPerStructure;
    [SerializeField] private int ammoRestoreCost;
    [SerializeField] private int fullRestoreCost;

    [SerializeField] private int structureRestored = 1;
    [SerializeField] private int ammoRestored = 5;

    private bool freeActionAvaliable = true;
    public bool FreeActionAvaliable => freeActionAvaliable;

    public int StructureRestored => structureRestored;
    public int AmmoRestored => ammoRestored;

    public event Action<RestResult> OnActionPerformed;

    protected override void OnSetup(MapNode node, RunState run)
    {
        freeActionAvaliable = true;
    }

    protected override void OnInteract()
    {
        if (used) return;

        UIManager.Instance.OpenScreen("restScreen", this);
    }

    public bool TryPerform(RestAction action) => Perform(action).success;

    public RestResult Perform(RestAction action)
    {
        var result = new RestResult
        {
            action = action,
            before = RestSnapshot.Capture(run)
        };

        if (!WouldHaveEffect(action, result.before))
        {
            result.failReason = RestFailReason.NoEffect;
            result.after = result.before;
            return result;
        }

        if (IsFree(action))
        {
            ApplyAction(action);
            freeActionAvaliable = false;
            result.wasFree = true;
        }
        else
        {
            result.cost = GetCost(action);
            if (!run.TryUseCurrency(result.cost))
            {
                result.failReason = RestFailReason.InsufficientFunds;
                result.cost = 0;
                result.after = result.before;
                return result;
            }

            ApplyAction(action);
        }

        result.success = true;
        result.after = RestSnapshot.Capture(run);
        OnActionPerformed?.Invoke(result);
        return result;
    }

    public bool IsFree(RestAction action) => freeActionAvaliable && action != RestAction.FullRestore;

    private void ApplyAction(RestAction action)
    {
        switch (action)
        {
            case RestAction.RepairStructure:
                run.RestoreStructure(structureRestored);
                break;

            case RestAction.RestoreAmmo:
                run.RestoreAmmo(ammoRestored);
                break;

            case RestAction.FullRestore:
                run.RestoreStructure(999);
                run.RestoreAmmo(999);
                break;
        }
    }

    private static bool WouldHaveEffect(RestAction action, RestSnapshot state) => action switch
    {
        RestAction.RepairStructure => !state.StructureFull,
        RestAction.RestoreAmmo => !state.AmmoFull,
        RestAction.FullRestore => !state.StructureFull || !state.AmmoFull,
        _ => true
    };

    public int GetCost(RestAction action) => action switch
    {
        RestAction.RepairStructure => costPerStructure,
        RestAction.RestoreAmmo => ammoRestoreCost,
        RestAction.FullRestore => fullRestoreCost,
        _ => 0
    };

}
