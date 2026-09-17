using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeStation : MonoBehaviour, IInteractable
{

    [SerializeField] private string prompt;
    public string InteractPrompt => prompt;

    [SerializeField] private SpriteRenderer visual;

    [SerializeField] private Vector2 standPos;

    [SerializeField] private bool isMajorUpgrade;

    private bool isEnabled;
    private bool used;

    private void Start()
    {
        used = false;
        isEnabled = false;
    }

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

        // setup reminder bs

        Color color = isMajorUpgrade ? Color.blue : Color.green;
        GetComponent<SpriteRenderer>().color = color; // replace with anim trigger
    }

    public void Interact()
    {
        if (!isEnabled) return;
        if (used) return;

        // move player to stand pos

        var current = RunManager.Instance.currentRun;

        List<LootResult> offer = new();
        if (isMajorUpgrade)
        {
            offer = LootManager.Instance.GenerateMajorUpgradeOffer(3, current.rewardContext, current.currentDepth);
        }
        else
        {
            offer = LootManager.Instance.GenerateAuxUpgradeOffer(3, current.rewardContext, current.currentDepth);
        }

        ChoiceRequest request = new("Select Upgrade", offer, OnSelected);

        UIManager.Instance.OpenScreen("rewardScreen", request);

        used = true;
    }

    private void OnSelected(LootResult chosen)
    {
        print($"{chosen.Item.Name} Chosen");

        if (chosen.Item is AuxUpgrade auxRef)
        {
            RunManager.Instance.activePlayer.Upgrades.GrantAux(auxRef);
        }
        else if(chosen.Item is MajorUpgrade majorRef)
        {
            RunManager.Instance.activePlayer.Upgrades.GrantMajor(majorRef);
        }
        else
        {
            Debug.LogError($"[UpgradeStation] upgrade {chosen.Item.Name} isnt aux");
        }

        // reset anim trigger
    }
}

