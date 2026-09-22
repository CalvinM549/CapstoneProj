using UnityEngine;

public class RestUIScreen : UIScreen
{
    [SerializeField] private RestActionButtonUI repairButton;
    [SerializeField] private RestActionButtonUI ammoButton;
    [SerializeField] private RestActionButtonUI fullRestoreButton;

    private RestStation rest;

    protected override void OnBeforeOpen(object payload)
    {
        rest = (RestStation)payload;


        repairButton.Setup("Repair Structure", () => HandleActionSelected(RestAction.RepairStructure));
        ammoButton.Setup("Restore Ammo", () => HandleActionSelected(RestAction.RestoreAmmo));
        fullRestoreButton.Setup("Full Service", () => HandleActionSelected(RestAction.FullRestore));
    }

    private void HandleActionSelected(RestAction selected)
    {
        if (rest.TryPerform(selected))
        {
            UpdateActionCards();
        }
        else
        {
            GetButton(selected).BuyFailure();
        }
    }

    private void UpdateActionCards()
    {
        bool free = rest.FreeActionAvaliable;

        repairButton.SetCost(free ? "FREE" : rest.GetCost(RestAction.RepairStructure).ToString());
        ammoButton.SetCost(free ? "FREE" : rest.GetCost(RestAction.RestoreAmmo).ToString());
        fullRestoreButton.SetCost(free ? "FREE" : rest.GetCost(RestAction.FullRestore).ToString());
    }

    private RestActionButtonUI GetButton(RestAction selected)
    {
        return selected switch
        {
            RestAction.RepairStructure => repairButton,
            RestAction.RestoreAmmo => ammoButton,
            RestAction.FullRestore => fullRestoreButton,
            _ => null
        };
    }
}
