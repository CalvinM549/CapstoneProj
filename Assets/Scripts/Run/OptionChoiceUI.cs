using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionChoiceUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Button button;

    public void Setup(ChoiceOption option, Action onPicked)
    {
        if (icon != null)
        {
            icon.sprite = option.icon;
            icon.enabled = option.icon != null;
        }

        if(title != null)
            title.text = option.title;
        
        if(description != null)
            description.text = option.description;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onPicked.Invoke());
    }
}
