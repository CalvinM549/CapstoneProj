using System;
using UnityEngine;

public enum RestAction
{
    RepairStructure,
    RestoreAmmo,
    FullRestore
}

public class RestStation : StationBase
{

    [SerializeField] private int costPerStructure;
    [SerializeField] private int ammoRestoreCost;
    [SerializeField] private int fullRestoreCost;

    private bool freeActionAvaliable = true;
    public bool FreeActionAvaliable => freeActionAvaliable;

    protected override void OnSetup(MapNode node, RunState run)
    {
        freeActionAvaliable = true;
    }

    protected override void OnInteract()
    {
        if (used) return;

        UIManager.Instance.OpenScreen("restScreen", this);
    }

    public bool TryPerform(RestAction action)
    {
        Action apply = action switch
        {
            RestAction.RepairStructure => () => run.RestoreStructure(3),
            RestAction.RestoreAmmo => () => run.RestoreAmmo(5),
            RestAction.FullRestore => () =>
            {
                run.RestoreStability(100f, true);
                run.RestoreStructure(3);
                run.RestoreAmmo(5);
            }
            ,
            _ => null
        };

        if (apply == null) return false;

        if (freeActionAvaliable)
        {
            apply();
            freeActionAvaliable = false;
            return true;
        }

        if (!run.TryUseCurrency(GetCost(action))) return false;

        apply();
        return true;
    }

    public int GetCost(RestAction action) => action switch
    {
        RestAction.RepairStructure => costPerStructure,
        RestAction.RestoreAmmo => ammoRestoreCost,
        RestAction.FullRestore => fullRestoreCost,
        _ => 0
    };

}
