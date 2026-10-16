using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSelectionScreen : UIScreen
{
    [Header("Config")]
    [SerializeField] private float timeBetweenCards = 0.15f;
    [SerializeField] private float rerollGlitchDuration = 0.25f;
    [SerializeField] private string rerollSfx = "UItick";

    [Header("References")]
    [SerializeField] private Transform optionPositionContainer;
    [SerializeField] private Transform optionContainer;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private TextMeshProUGUI currencyText;

    [SerializeField] private OptionCardUI cardPrefab;

    [Header("Reroll (optional)")]
    [SerializeField] private Button rerollButton;
    [SerializeField] private TextMeshProUGUI rerollLabel;
    [SerializeField] private string rerollFormat = "REROLL {0}";
    [SerializeField] private string rerollUnavailable = "NO REROLLS";

    private readonly List<OptionCardUI> activeCards = new();
    private Action<LootResult> onSelected;
    private IRerollSource rerollSource;
    private Coroutine spawnRoutine;
    private bool choiceMade;

    protected override void OnBeforeOpen(object payload)
    {
        var data = (ChoiceRequest)payload;
        onSelected = data.onSelected;
        rerollSource = data.reroll;
        choiceMade = false;

        if (promptText != null) promptText.text = TerminalFormat.Header(data.prompt.ToUpperInvariant());

        BuildCards(data.options);
        RefreshFooter();
    }

    protected override void OnClosed()
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    #region Cards

    private void BuildCards(List<LootResult> options)
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);

        foreach (var card in activeCards)
            if (card != null) Destroy(card.gameObject);

        activeCards.Clear();

        foreach (var option in options)
        {
            var card = Instantiate(cardPrefab, optionPositionContainer);
            var chosen = option;
            card.Setup(chosen, () => HandleSelected(chosen));
            card.cg.alpha = 0f;
            card.SetInteractable(false);
            activeCards.Add(card);
        }

        spawnRoutine = StartCoroutine(SpawnCards());
    }

    private IEnumerator SpawnCards()
    {
        var wait = new WaitForSecondsRealtime(timeBetweenCards);

        for (int i = 0; i < activeCards.Count; i++)
        {
            yield return wait;
            activeCards[i].cg.alpha = 1f;
            activeCards[i].SetInteractable(true);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("TypeTick", Vector3.zero);
        }

        spawnRoutine = null;
    }

    private void HandleSelected(LootResult selected)
    {
        if (choiceMade) return;
        choiceMade = true;

        onSelected?.Invoke(selected);

        UIManager.Instance.CloseTop();
    }

    #endregion

    #region Reroll

    public void OnRerollClicked()
    {
        if (choiceMade || rerollSource == null) return;

        if (!rerollSource.TryReroll(out var newOffer))
        {
            RefreshFooter();
            return;
        }

        if (ScreenGlitch.Instance != null) ScreenGlitch.Instance.Pulse(rerollGlitchDuration, 0.5f);
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(rerollSfx))
            AudioManager.Instance.PlaySFX(rerollSfx, Vector3.zero);

        BuildCards(newOffer);
        RefreshFooter();
    }

    private void RefreshFooter()
    {
        var run = RunManager.CurrentRun;
        if (currencyText != null && run != null) currencyText.text = TerminalFormat.Currency(run.currency);

        if (rerollButton == null) return;

        bool hasSource = rerollSource != null;
        rerollButton.gameObject.SetActive(hasSource);
        if (!hasSource) return;

        bool canReroll = rerollSource.HasRerollsLeft && rerollSource.CanAffordReroll;
        rerollButton.interactable = canReroll;

        if (rerollLabel != null)
        {
            string text = rerollSource.HasRerollsLeft
                ? string.Format(rerollFormat, TerminalFormat.Currency(rerollSource.RerollCost))
                : rerollUnavailable;

            var terminal = rerollButton.GetComponent<TerminalButton>();
            if (terminal != null) terminal.SetLabel(text);
            else rerollLabel.text = text;
        }
    }

    #endregion

    public void OnNoneSelected() => UIManager.Instance.CloseTop();

    public void OnBackClicked() => OnNoneSelected();
}

public struct ChoiceRequest
{
    public string prompt;
    public List<LootResult> options;
    public Action<LootResult> onSelected;
    public IRerollSource reroll;

    public ChoiceRequest(string prompt, List<LootResult> options, Action<LootResult> onSelected, IRerollSource reroll = null)
    {
        this.prompt = prompt;
        this.options = options;
        this.onSelected = onSelected;
        this.reroll = reroll;
    }
}
