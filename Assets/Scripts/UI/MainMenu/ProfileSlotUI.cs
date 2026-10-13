using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileSlotUI : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private GameObject runInProgressTag;

    public void Setup(int slot, PlayerProfile profile, Action<int, PlayerProfile> onSelect, Action<int, PlayerProfile> onDelete)
    {
        bool empty = profile == null;

        titleText.text = empty ? $"[EMPTY SLOT {slot + 1:00}]" : profile.profileName;
        statsText.text = empty ? "> INITIALIZE NEW INSTANCE" : FormatStats(profile);

        deleteButton.gameObject.SetActive(!empty);
        if (runInProgressTag != null)
            runInProgressTag.SetActive(!empty && RunSaveSystem.HasValidRun(profile));

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onSelect(slot, profile));

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => onDelete(slot, profile));
    }

    private static string FormatStats(PlayerProfile p)
    {
        string best = p.bestRunTime > 0f
            ? $"{(int)(p.bestRunTime / 60)}:{(int)(p.bestRunTime % 60):00}"
            : "--:--";

        return $"RUNS {p.runsAttempted} // CLEARS {p.runVictoryCount} // BEST {best}\n" +
            $"ARCHIVES {p.unlockedArchives.Count} // SCRAP {p.metaCurrency}";
    }
}
