using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TypewriterBox : MonoBehaviour
{
    [SerializeField] private bool playOnStart;

    [SerializeField] private float typewriterCharDelay = 0.03f;

    [SerializeField] private float lineDelay = 0.2f;

    [Tooltip("Characters revealed per tick. Raise for long read-outs so they don't take ages.")]
    [SerializeField] [Min(1)] private int charsPerTick = 1;

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
            bool hitNewLine = false;

            for (int i = 0; i < charsPerTick && counter < totalVisibleCharacters; i++)
            {
                char c = textBox.textInfo.characterInfo[counter].character;
                counter++;

                if (c == '\n' || c == '\r')
                {
                    hitNewLine = true;
                    break;
                }
            }

            textBox.maxVisibleCharacters = counter;

            AudioManager.Instance.PlaySFX("TypeTick", Vector3.zero);

            // Realtime so text still types on screens that pause the game
            yield return new WaitForSecondsRealtime(hitNewLine ? lineDelay : typewriterCharDelay);
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
