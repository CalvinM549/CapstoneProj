using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private TextMeshProUGUI currencyCounter;

    [SerializeField] private float timeToAdjust;
    [SerializeField] private float timeBeforeFade;

    [Tooltip("Track shards banked this run instead of run currency.")]
    [SerializeField] private bool trackShards;
    [Tooltip("{0} is replaced with the value.")]
    [SerializeField] private string format = "$[{0}]";

    private int currentCounter;
    private Coroutine tickRoutine;

    private bool visible;
    private float hideTimer;

    private void Start()
    {
        cg.alpha = 0f;
        visible = false;
    }

    private void OnEnable()
    {
        if (trackShards) GameEvents.OnShardsChanged += HandleCurrencyChange;
        else GameEvents.OnCurrencyChanged += HandleCurrencyChange;
    }

    private void OnDisable()
    {
        GameEvents.OnShardsChanged -= HandleCurrencyChange;
        GameEvents.OnCurrencyChanged -= HandleCurrencyChange;
        cg?.DOKill();
    }

    [ContextMenu("DoChangeTest")]
    public void TestMethod()
    {
        HandleCurrencyChange(currentCounter + Random.Range(1, 500));
    }

    private void Update()
    {
        if (visible && tickRoutine == null)
        {
            hideTimer += Time.deltaTime;
        }

        if (hideTimer >= timeBeforeFade && visible)
        {
            FadeGroup();
        }
    }

    private void FadeGroup()
    {
        visible = false;
        cg?.DOKill();
        cg.DOFade(0, 0.8f);
    }

    private void HandleCurrencyChange(int newValue)
    {
        if (tickRoutine != null)
        {
            StopCoroutine(tickRoutine);
            tickRoutine = null;
        }

        visible = true;
        hideTimer = 0f;
        cg.DOKill();
        cg.alpha = 1.0f;

        currencyCounter.DOKill();
        currencyCounter.color = Color.green;
        currencyCounter.DOColor(Color.white, 1f);

        tickRoutine = StartCoroutine(TickRoutine(currentCounter, newValue, timeToAdjust));
    }

    private IEnumerator TickRoutine(int start, int end, float time)
    {
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / time);

            float currentFloat = Mathf.Lerp(start, end, t);
            int currentInt = Mathf.RoundToInt(currentFloat);

            currentCounter = currentInt;
            currencyCounter.text = string.Format(format, currentInt);

            yield return null;
        }

        currencyCounter.text = string.Format(format, end);

        tickRoutine = null;
    }

}
