using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IRunUIOverlay
{
    void Show();
    void Hide();
}

public class RunUIManager : MonoBehaviour
{
    public static RunUIManager Instance { get; private set; }

    [SerializeField] private PlayerUI playerUIPrefab;
    private PlayerUI activePlayerUI;
    
    [SerializeField] private MultiChoiceOverlayUI choiceUI;


    private Transform playerTrackingPos;

    private IRunUIOverlay activeOverlay;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Initialize(Player player)
    {
        playerTrackingPos = player.transform;

        activePlayerUI = Instantiate(playerUIPrefab, transform);
        activePlayerUI.gameObject.SetActive(true);
        activePlayerUI.Initialize(player);
    }   

    // Hold player UI??

    private void HandleChoiceRequest(ChoiceRequest request)
    {
        TimescaleManager.Instance.PauseGame(this);
        choiceUI.ShowWithChoice(request);
    }

    public void CloseChoicePanel()
    {
        TimescaleManager.Instance.PauseGame(this);
    }

    public void ToggleOverlay(IRunUIOverlay overlay, bool doPause)
    {
        if (ReferenceEquals(activeOverlay, overlay))
        {
            overlay.Hide();
            activeOverlay = null;

            if(doPause)
                TimescaleManager.Instance.UnpauseGame(overlay);
    
            return;
        }

        activeOverlay?.Hide();
        overlay.Show();
        activeOverlay = overlay;

        if(doPause)
            TimescaleManager.Instance.PauseGame(overlay);
    }
}

[Serializable]
public struct ChoiceOption
{
    public string title;
    public string description;
    public Sprite icon;
}

public class ChoiceRequest
{
    public string prompt;
    public List<ChoiceOption> options;
    public Action<int> onSelected;
}