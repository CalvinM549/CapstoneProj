using System;
using Unity.VisualScripting;
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

    [SerializeField] private int structureRestored;
    [SerializeField] private int ammoRestored;

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
        if (freeActionAvaliable && action != RestAction.FullRestore)
        {
            ApplyAction(action);
            freeActionAvaliable = false;
            return true;
        }

        if (!run.TryUseCurrency(GetCost(action))) return false;

        ApplyAction(action);
        return true;
    }

    private void ApplyAction(RestAction action)
    {
        switch (action)
        {
            case RestAction.RepairStructure:
                run.RestoreStructure(structureRestored);
                break;

            case RestAction.RestoreAmmo: 
                run.RestoreAmmo(5);
                break;

            case RestAction.FullRestore:
                run.RestoreStructure(999);
                run.RestoreAmmo(999);
                break;
        }
    }

    public int GetCost(RestAction action) => action switch
    {
        RestAction.RepairStructure => costPerStructure,
        RestAction.RestoreAmmo => ammoRestoreCost,
        RestAction.FullRestore => fullRestoreCost,
        _ => 0
    };

}
