using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TypewriterBox : MonoBehaviour
{
    [SerializeField] private bool playOnStart;

    [SerializeField] private float typewriterCharDelay = 0.03f;

    [SerializeField] private float lineDelay = 0.2f;

    private TextMeshProUGUI textBox;
    private Coroutine typeRoutine;

    private void Awake()
    {
        textBox = GetComponent<TextMeshProUGUI>();   
    }

    private void Start()
    {
        if (playOnStart)
            SetText();
    }

    public void SetText(string text = null)
    {
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
        }

        typeRoutine = StartCoroutine(TypeText(text));
    }

    private IEnumerator TypeText(string text = null)
    {
        if (!string.IsNullOrEmpty(text))
        {
            textBox.text = text;
        }

        textBox.maxVisibleCharacters = 0;

        textBox.ForceMeshUpdate();

        int totalVisibleCharacters = textBox.textInfo.characterCount;
        int counter = 0;

        while (counter < totalVisibleCharacters)
        {
            var charInfo = textBox.textInfo.characterInfo[counter];

            textBox.maxVisibleCharacters = counter;

            char c = charInfo.character;

            AudioManager.Instance.PlaySFX("TypeTick", Vector3.zero);

            if (c == '\n' || c == '\r')
            {
                yield return new WaitForSeconds(lineDelay);
            }
            else
            {
                yield return new WaitForSeconds(typewriterCharDelay);
            }
            
            counter++;
        }

        textBox.maxVisibleCharacters = textBox.textInfo.characterCount;
        typeRoutine = null;
    }

    public void SkipToEnd()
    {
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }

        textBox.maxVisibleCharacters = textBox.textInfo.characterCount;
    }
}
