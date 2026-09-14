using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchiveEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private GameObject lockedOverlay;

    [SerializeField] private string lockedName = "???";
    [SerializeField] private string lockedDescription = "Undiscovered Log";
    [SerializeField] private string lockedIcon;

    public void Setup(ArchiveData entry, bool unlocked)
    {
        if (unlocked)
        {
            nameText.text = entry.Title;
            nameText.text = entry.Text;
        }
        else
        {
            nameText.text = lockedName;
            descriptionText.text = lockedDescription;
        }

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!unlocked);
    }
}
