using System.Collections;
using TMPro;
using UnityEngine;

public class SpeechDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textBox;
    [SerializeField] private CanvasGroup cg;

    [SerializeField] private float typeInterval;

    private Coroutine typeRoutine;
    private WaitForSeconds characterInterval;

    private void Awake()
    {
        if (cg == null) cg = GetComponent<CanvasGroup>();

        characterInterval = new(typeInterval);
    }

    public void DisplayText(string message, Transform speaker)
    {
        if(speaker != null)
            transform.SetParent(speaker);

        if(typeRoutine != null)
            StopCoroutine(typeRoutine);

        typeRoutine = StartCoroutine(TypeText(message));
    }

    private IEnumerator TypeText(string message)
    {
        textBox.text = message;
        textBox.maxVisibleCharacters = 0;

        textBox.ForceMeshUpdate();

        int totalCharacters = textBox.textInfo.characterCount;

        for (int i = 0; i < totalCharacters; i++)
        {
            textBox.maxVisibleCharacters = i;
            yield return characterInterval;
        }

        typeRoutine = null;
    }
}
