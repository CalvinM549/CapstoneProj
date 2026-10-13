using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TextStatusUI : MonoBehaviour
{
    [SerializeField] private bool isPlayerUI;
    private PlayerUI playerUI;

    [SerializeField] private TextMeshProUGUI textBox;
    [SerializeField] private Transform statusContainer;

    [SerializeField] private float typewriterCharDelay = 0.01f;
    [SerializeField] private float newLineDelay = 0.1f;
    [SerializeField] private float colourFadeTime = 0.5f;

    private StatusEffectController statusController;

    private List<ActiveStatusEffect> cachedActive = new();

    private static readonly string[] DotFrames = { "", ".", "..", "..." };

    private string commitedText = string.Empty;

    private Coroutine typingRoutine;

    private void Awake()
    {
        if (isPlayerUI) playerUI = GetComponent<PlayerUI>();
        else statusController = GetComponentInParent<StatusEffectController>();
    }

    private void OnEnable()
    {
        if (isPlayerUI)
            playerUI.Initialized += HandleUIReady;
        else
            statusController.OnEffectsChanged += HandleStatusUpdated;
    }

    private void OnDisable()
    {
        if (isPlayerUI)
            playerUI.Initialized -= HandleUIReady;

        if (statusController != null)
            statusController.OnEffectsChanged -= HandleStatusUpdated;
    }

    private void HandleUIReady(Player p)
    {
        statusController = p.GetComponent<StatusEffectController>();
        statusController.OnEffectsChanged += HandleStatusUpdated;

        Initialize();
    }

    public void Initialize()
    {
        textBox.text = string.Empty;
        commitedText = string.Empty;

        cachedActive.Clear();
    }

    private void HandleStatusUpdated()
    {
        var activeStatuses = statusController.AllActiveEffects;
        if (activeStatuses == cachedActive) return;

        var keepActive = new List<string>();
        var typeNew = new List<string>();

        foreach (var activeStatus in activeStatuses)
        {
            //int dotIndex = Mathf.RoundToInt(activeStatus.GetPercentDuration() * (DotFrames.Length - 1));

            string tag = activeStatus.data.statusName
                + $" x{activeStatus.stacks.ToString()}";

            if (cachedActive.Contains(activeStatus))
            {
                keepActive.Add(tag);
            }
            else
            {
                typeNew.Add(tag);
            }
        }

        cachedActive = activeStatuses.ToList();

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(TypeLinesRoutine(keepActive, typeNew));

        textBox.color = Color.white;
        statusContainer.localScale = Vector3.one * 1.2f;

        textBox.DOKill();
        statusContainer.DOKill();

        statusContainer.DOScale(Vector2.one, colourFadeTime);
        textBox.DOColor(Color.green, colourFadeTime);
    }


    
    private IEnumerator TypeLinesRoutine(List<string> keptItems, List<string> newItems)
    {
        textBox.text = string.Empty;
        commitedText = string.Empty;

        foreach (var keptItem in keptItems)
        {
            CommitNewLine(keptItem);
        }

        foreach (var newItem in newItems)
        {
            string baseText = commitedText;

            CommitNewLine(string.Empty);
            for (int i = 0; i <= newItem.Length; i++)
            {
                string partial = newItem.Substring(0, i);
                commitedText = string.IsNullOrEmpty(baseText)
                    ? partial
                    : baseText + "\n" + partial;

                textBox.text = commitedText;
                yield return new WaitForSeconds(typewriterCharDelay);
            }
            yield return new WaitForSeconds(newLineDelay);
        }
    }

    private void CommitNewLine(string text)
    {
        commitedText = string.IsNullOrEmpty(commitedText)
            ? text
            : commitedText + "\n" + text;

        textBox.text = commitedText;
    }
}
