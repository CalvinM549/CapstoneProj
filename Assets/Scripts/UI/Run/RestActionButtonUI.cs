using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RestActionButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI cost;
    [SerializeField] private Button button;

    public void Setup(string header, Action onPicked)
    {
        title.text = header;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onPicked?.Invoke());
    }

    public void SetCost(string costAsString)
    {
        cost.text = costAsString;
    }

    public void BuyFailure()
    {
        // play anim
    }
}
