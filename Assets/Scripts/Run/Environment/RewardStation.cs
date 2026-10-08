using System.Collections.Generic;
using UnityEngine;

public class RewardStation : StationBase, IInteractable
{
    [SerializeField] private int slotIndex = 0;

    private List<LootResult> offer;

    private void Start()
    {
        used = false;
        isEnabled = false;
    }

    protected override void OnSetup(MapNode node, RunState run)
    {
        offer = services.loot.RollSlot(node, run, slotIndex);

        print($"setting up rewards : {node.offers[slotIndex].category} {offer.Count}");
    }

    protected override void OnInteract()
    {
        if (!isEnabled) return;
        if (used) return;

        string requestPrompt = $"Select Reward";
        ChoiceRequest request = new(requestPrompt, offer, OnSelected);

        UIManager.Instance.OpenScreen("rewardScreen", request);
    }

    private void OnSelected(LootResult chosen)
    {
        print($"{chosen.Item.Name} Chosen");

        run.GrantLoot(chosen);

        used = true;
        RemoveIndicator();
    }

}

