using TMPro;
using UnityEngine;

public class InteractPromptUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textBox;

    public void SetPrompt(string text) => textBox.text = text;
}
