using System.Collections;
using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private TextMeshProUGUI currencyCounter;

    [SerializeField] private float timeToAdjust;

    private int currentCounter;
    private Coroutine tickRoutine;

    private void OnEnable()
    {
        // game event hook
    }

    private void OnDisable()
    {
        
    }

    [ContextMenu("DoChangeTest")]
    public void TestMethod()
    {
        HandleCurrencyChange(currentCounter + Random.Range(1, 500));
    }

    private void HandleCurrencyChange(int newValue)
    {
        if (tickRoutine != null)
        {
            StopCoroutine(tickRoutine);
            tickRoutine = null;
        }

        tickRoutine = StartCoroutine(TickRoutine(currentCounter, newValue, timeToAdjust));
    }

    private IEnumerator TickRoutine(int start, int end, float time)
    {
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);

            float currentFloat = Mathf.Lerp(start, end, t);
            int currentInt = Mathf.RoundToInt(currentFloat);

            currentCounter = currentInt;
            currencyCounter.text = currentInt.ToString();

            yield return null;
        }

        currencyCounter.text = end.ToString();
    }

}
