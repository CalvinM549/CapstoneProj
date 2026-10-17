using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewardStation : StationBase, IInteractable, IRerollSource
{
    [SerializeField] private int slotIndex = 0;

    [Header("Reroll")]
    [SerializeField] private bool allowReroll = true;
    [SerializeField] private int baseRerollCost = 20;
    [Tooltip("Added to the cost for every reroll already used at this station.")]
    [SerializeField] private int rerollCostStep = 15;
    [Tooltip("-1 = unlimited")]
    [SerializeField] private int maxRerolls = -1;

    private List<LootResult> offer;
    private int rerollsUsed;

    // Player stats can discount rerolls or grant free ones (e.g. Black-Market Firmware)
    public int RerollCost
    {
        get
        {
            var stats = run?.player != null ? run.player.Stats : null;
            if (stats != null && rerollsUsed < stats.GetInt(StatRef.PlayerFreeRerolls)) return 0;

            int discount = stats != null ? stats.GetInt(StatRef.PlayerRerollDiscount) : 0;
            return Mathf.Max(0, baseRerollCost + rerollCostStep * rerollsUsed - discount);
        }
    }
    public int RerollsUsed => rerollsUsed;
    public bool HasRerollsLeft => allowReroll && (maxRerolls < 0 || rerollsUsed < maxRerolls);
    public bool CanAffordReroll => run != null && run.currency >= RerollCost;

    private void Start()
    {
        used = false;
        isEnabled = false;
    }

    protected override void OnSetup(MapNode node, RunState run)
    {
        rerollsUsed = 0;
        offer = services.loot.RollSlot(node, run, slotIndex);
    }

    protected override void OnInteract()
    {
        if (!isEnabled) return;
        if (used) return;

        string requestPrompt = $"Select Reward";
        ChoiceRequest request = new(requestPrompt, offer, OnSelected, allowReroll ? this : null);

        UIManager.Instance.OpenScreen("rewardScreen", request);
    }

    public bool TryReroll(out List<LootResult> newOffer)
    {
        newOffer = offer;
        if (used || !HasRerollsLeft) return false;
        if (!run.TryUseCurrency(RerollCost)) return false;

        rerollsUsed++;

        var current = offer.Select(o => o.Item.Id).ToList();
        offer = services.loot.RollSlot(node, run, slotIndex, rerollsUsed, current);

        newOffer = offer;
        return true;
    }

    private void OnSelected(LootResult chosen)
    {
        print($"{chosen.Item.Name} Chosen");

        run.GrantLoot(chosen);

        used = true;
        RemoveIndicator();
    }

}

