using TMPro;
using UnityEngine;

public class MultiChoiceOverlayUI : MonoBehaviour
{
    [SerializeField] private Transform root;
    [SerializeField] private TextMeshProUGUI promptText;

    [SerializeField] private OptionChoiceUI[] choicePool;

    private ChoiceRequest activeRequest;

    public void ShowWithChoice(ChoiceRequest request)
    {
        activeRequest = request;

        if(promptText != null)
            promptText.text = request.prompt;

        for (int i = 0; i < choicePool.Length; i++)
        {
            bool hasOption = i < request.options.Count;
            choicePool[i].gameObject.SetActive(hasOption);

            if (hasOption)
            {
                int index = i;
                choicePool[i].Setup(request.options[i], () => HandleSelected(index));
            }
        }

        root.gameObject.SetActive(true);
    }

    private void HandleSelected(int index)
    {
        // Animations?
        root.gameObject.SetActive(false);

        activeRequest.onSelected?.Invoke(index);

        activeRequest = null;

        RunUIManager.Instance.CloseChoicePanel();
    }
}
