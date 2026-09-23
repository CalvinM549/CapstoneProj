using System.Collections.Generic;
using UnityEngine;

public class RewardStation : StationBase, IInteractable
{

    [SerializeField] private SpriteRenderer visual;

    [SerializeField] private Vector2 standPos;

    [SerializeField] private bool isMajorUpgrade;

    private List<LootResult> offer;

    private void Start()
    {
        used = false;
        isEnabled = false;
    }

    protected override void OnSetup(MapNode node, RunState run)
    {
        // determine major type

        node.cachedLoot ??= RunManager.Instance.lootService.Roll(
            new LootRequest
            {
                category = node.rewards.category,
                count = node.rewards.baseOfferCount,
                affectPity = true
            },
            run.rewardContext,
            GameRng.NodeRng(run.seed, node.coordinates),
            run.currentDepth);
        offer = node.cachedLoot;
            
    }

    protected override void OnInteract()
    {
        if (!isEnabled) return;
        if (used) return;

        ChoiceRequest request = new("Select Upgrade", offer, OnSelected);

        UIManager.Instance.OpenScreen("rewardScreen", request);
    }

    private void OnSelected(LootResult chosen)
    {
        print($"{chosen.Item.Name} Chosen");

        run.GrantLoot(chosen);

        used = true;
        RemoveIndicator();
        // reset anim trigger
    }

}

