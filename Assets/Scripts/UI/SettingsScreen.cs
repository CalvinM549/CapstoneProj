using System;
using UnityEngine;

// Shared settings screen (main menu + hub). Edits apply live and are saved when the screen closes.
// Any row left unassigned in the inspector is simply skipped, so rows can be removed/added freely.
public class SettingsScreen : UIScreen
{
    [Header("Audio")]
    [SerializeField] private TerminalSlider masterVolume;
    [SerializeField] private TerminalSlider musicVolume;
    [SerializeField] private TerminalSlider sfxVolume;

    [Header("Display")]
    [SerializeField] private TerminalChoice fullscreen;
    [SerializeField] private TerminalChoice vsync;
    [SerializeField] private TerminalChoice quality;

    [Header("Gameplay")]
    [SerializeField] private TerminalSlider screenShake;
    [SerializeField] private TerminalSlider glitchIntensity;
    [SerializeField] private TerminalChoice runTimer;

    private GameSettings working;
    private bool dirty;

    public override void Initialize()
    {
        Bind(masterVolume, (s, v) => s.masterVolume = v);
        Bind(musicVolume, (s, v) => s.musicVolume = v);
        Bind(sfxVolume, (s, v) => s.sfxVolume = v);

        Bind(fullscreen, (s, i) => s.fullscreen = i != 0);
        Bind(vsync, (s, i) => s.vsync = i != 0);
        Bind(quality, (s, i) => s.qualityPreset = i);

        Bind(screenShake, (s, v) => s.screenShake = v);
        Bind(glitchIntensity, (s, v) => s.glitchIntensity = v);
        Bind(runTimer, (s, i) => s.showRunTimer = i != 0);
    }

    protected override void OnBeforeOpen(object payload)
    {
        working = SettingsManager.Instance.Current.Clone();
        dirty = false;
        PushToRows();
    }

    protected override void OnClosed()
    {
        if (!dirty) return;

        SettingsManager.Instance.Save();
        dirty = false;
    }

    #region Buttons

    public void OnDefaultsClicked()
    {
        UIManager.Instance.OpenScreen("confirmScreen", new ConfirmRequest(
            "RESTORE DEFAULT SETTINGS?",
            () =>
            {
                working = new GameSettings();
                Commit();
                PushToRows();
            },
            "RESTORE"));
    }

    public void OnBackClicked() => UIManager.Instance.CloseTop();

    #endregion

    #region Binding

    private void Bind(TerminalSlider slider, Action<GameSettings, float> apply)
    {
        if (slider == null) return;
        slider.OnValueChanged += v => { apply(working, v); Commit(); };
    }

    private void Bind(TerminalChoice choice, Action<GameSettings, int> apply)
    {
        if (choice == null) return;
        choice.OnIndexChanged += i => { apply(working, i); Commit(); };
    }

    private void Commit()
    {
        dirty = true;
        SettingsManager.Instance.Apply(working.Clone());
    }

    private void PushToRows()
    {
        if (masterVolume != null) masterVolume.SetValueWithoutNotify(working.masterVolume);
        if (musicVolume != null) musicVolume.SetValueWithoutNotify(working.musicVolume);
        if (sfxVolume != null) sfxVolume.SetValueWithoutNotify(working.sfxVolume);

        if (fullscreen != null) fullscreen.SetBoolWithoutNotify(working.fullscreen);
        if (vsync != null) vsync.SetBoolWithoutNotify(working.vsync);

        if (quality != null)
        {
            string[] names = QualitySettings.names;
            for (int i = 0; i < names.Length; i++) names[i] = names[i].ToUpperInvariant();

            int index = working.qualityPreset >= 0 ? working.qualityPreset : QualitySettings.GetQualityLevel();
            quality.SetOptions(names, index);
        }

        if (screenShake != null) screenShake.SetValueWithoutNotify(working.screenShake);
        if (glitchIntensity != null) glitchIntensity.SetValueWithoutNotify(working.glitchIntensity);
        if (runTimer != null) runTimer.SetBoolWithoutNotify(working.showRunTimer);
    }

    #endregion
}
