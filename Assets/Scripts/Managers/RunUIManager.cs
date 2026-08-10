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

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void Update()
    {
        
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

    public void ToggleOverlay(IRunUIOverlay overlay)
    {
        if (ReferenceEquals(activeOverlay, overlay))
        {
            overlay.Hide();
            activeOverlay = null;
            TimescaleManager.Instance.UnpauseGame(this);
            return;
        }

        activeOverlay?.Hide();
        overlay.Show();
        activeOverlay = overlay;
        TimescaleManager.Instance.PauseGame(this);
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