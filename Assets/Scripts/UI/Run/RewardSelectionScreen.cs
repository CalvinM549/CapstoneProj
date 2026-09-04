using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class RewardSelectionScreen : UIScreen
{
    [SerializeField] private Transform optionContainer;
    [SerializeField] private TextMeshProUGUI promptText;

    [SerializeField] private OptionCardUI cardPrefab;
    private List<OptionCardUI> activeCards = new();
    private Action<LootResult> onSelected;

    private ChoiceRequest activeRequest;

    protected override void OnBeforeOpen(object payload)
    {
        var data = (ChoiceRequest)payload;
        onSelected = data.onSelected;

        foreach(var button in activeCards)
            Destroy(button.gameObject);

        foreach (var option in data.options)
        {
            var button = Instantiate(cardPrefab, optionContainer);
            button.Setup(option, () => HandleSelected(option));
            activeCards.Add(button);
        }

    }

    private void HandleSelected(LootResult selected)
    {
        // Animations?

        onSelected?.Invoke(selected);

        UIManager.Instance.CloseTop();
    }
}

public struct ChoiceRequest
{
    public string prompt;
    public List<LootResult> options;
    public Action<LootResult> onSelected;
}