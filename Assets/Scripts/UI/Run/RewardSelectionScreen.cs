using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardSelectionScreen : UIScreen
{
    [Header("Config")]
    [SerializeField] private float timeBetweenCards = 0.15f;

    [Header("References")]
    [SerializeField] private Transform optionPositionContainer;
    [SerializeField] private Transform optionContainer;
    [SerializeField] private TextMeshProUGUI promptText;

    [SerializeField] private OptionCardUI cardPrefab;
    private List<OptionCardUI> activeCards = new();
    private Action<LootResult> onSelected;

    protected override void OnBeforeOpen(object payload)
    {
        var data = (ChoiceRequest)payload;
        onSelected = data.onSelected;

        promptText.text = $"[{data.prompt}]";

        foreach(var button in activeCards)
            Destroy(button.gameObject);
        
        activeCards.Clear();

        foreach (var option in data.options)
        {
            //var positionLock = new GameObject("optionPos", typeof(RectTransform));
            //positionLock.transform.SetParent(optionPositionContainer);

            var button = Instantiate(cardPrefab, optionPositionContainer);
            //button.GetComponent<RectTransform>().pivot = positionLock.GetComponent<RectTransform>().pivot;
            //button.GetComponent<RectTransform>().position = positionLock.GetComponent<RectTransform>().position ;

            button.Setup(option, () => HandleSelected(option));
            button.cg.alpha = 0f;
            activeCards.Add(button);
        }

        StartCoroutine(SpawnCards());
    }

    private IEnumerator SpawnCards()
    {
        var wait = new WaitForSecondsRealtime(timeBetweenCards);

        for (int i = 0; i < activeCards.Count; i++)
        {
            yield return wait;
            activeCards[i].cg.alpha = 1f;
        }

    }

    private void HandleSelected(LootResult selected)
    {
        // Animations?

        onSelected?.Invoke(selected);

        UIManager.Instance.CloseTop();
    }

    public void OnNoneSelected()
    {
        // remove trigger somehow

        UIManager.Instance.CloseTop();
    }
}

public struct ChoiceRequest
{
    public string prompt;
    public List<LootResult> options;
    public Action<LootResult> onSelected;

    public ChoiceRequest(string prompt, List<LootResult> options, Action<LootResult> onSelected)
    {
        this.prompt = prompt;
        this.options = options;
        this.onSelected = onSelected;
    }
}