using System.Collections.Generic;
using UnityEngine;

public class RewardStation : StationBase, IInteractable
{
    private List<LootResult> offer;

    private void Start()
    {
        used = false;
        isEnabled = false;
    }

    protected override void OnSetup(MapNode node, RunState run)
    {
        node.cachedLoot ??= RunManager.Instance.lootService.Roll(
            new LootRequest
            {
                category = node.rewards.category,
                count = node.rewards.baseOfferCount,
                affectPity = true
            },
            run.rewardContext,
            RNGManager.NodeRng(run.seed, node.coordinates),
            run.currentDepth);
        offer = node.cachedLoot;
    }

    protected override void OnInteract()
    {
        if (!isEnabled) return;
        if (used) return;

        string requestPrompt = $"Select {node.rewards.category.GetRewardCategoryName()}";
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

