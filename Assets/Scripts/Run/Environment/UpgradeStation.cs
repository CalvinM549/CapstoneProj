using UnityEngine;

public class UpgradeStation : SimpleInteractable
{
    [SerializeField] private SpriteRenderer visual;

    [SerializeField] private Vector2 standPos;

    private bool isEnabled = false;

    private void OnEnable()
    {
        GameEvents.OnRoomCompleted += HandleRoomCompleted;
    }

    private void OnDisable()
    {
        GameEvents.OnRoomCompleted -= HandleRoomCompleted;
    }

    public void HandleRoomCompleted()
    {
        isEnabled = true;
        // trigger animation

        GetComponent<SpriteRenderer>().color = Color.green; // replace with anim trigger
    }

    protected override void OnInteract()
    {
        print("Upgrade Interacted");

        if (!isEnabled) return;
        base.OnInteract();

        var current = RunManager.Instance.currentRun;

        var offer = LootManager.Instance.GenerateAuxUpgradeOffer(3, current.rewardContext, current.roomsCleared);

        ChoiceRequest request = new("Select Upgrade", offer, OnSelected);

        UIManager.Instance.OpenScreen("rewardScreen", request);
        // Activate loot system
    }

    private void OnSelected(LootResult chosen)
    {
        print($"{chosen.Item.Name} Chosen");
        RunManager.Instance.currentRun.GrantPlayerUpgrade();
    }
}

