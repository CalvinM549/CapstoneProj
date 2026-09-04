using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public interface IRunUIOverlay
{
    void Show();
    void Hide();
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private Transform screenParent;

    [SerializeField] private PlayerUI playerUIPrefab;
    private PlayerUI activePlayerUI;
    
    [SerializeField] private RewardSelectionScreen choiceUI;


    private Dictionary<string, UIScreen> screens = new();
    private List<UIScreen> stack = new();
    
    public UIScreen Current => stack.Count > 0 ? stack[^1] : null;
    public bool ScreenOpen => stack.Count > 0;



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
        SetupPlayerUI(player);
        InputManager.Instance.ExitPressed += HandleExit;
    }


    private void HandleExit()
    {
        if (Current != null && !Current.IsTransitioning)
        {
            Current.HandleCancel();
        }
    }

    public UIScreen OpenScreen(string id, object payload = null)
    {
        if (screens.TryGetValue(id, out var screen))
        {
            stack.Add(screen);
            screen.transform.SetAsLastSibling();

            if(screen.pausesGame) TimescaleManager.Instance.PauseGame(screen);

            screen.Open(payload);
            return screen;

        }
        else
        {
            Debug.LogWarning($"[UIManager] id: {id} does not exist / is not active");
            return null;
        }
    }

    public void CloseTop()
    {
        if (stack.Count <= 0) return;

        var top = stack[^1];
        stack.RemoveAt(stack.Count - 1);

        if(top.pausesGame) TimescaleManager.Instance.UnpauseGame(top);
        top.Close();
    }

    public void CloseAll()
    {
        while(stack.Count > 0) CloseTop();
    }

    public void Replace(string id, object payload = null)
    {
        CloseAll();
        OpenScreen(id, payload);
    }

    private void SetupPlayerUI(Player player)
    {
        playerTrackingPos = player.transform;

        activePlayerUI = Instantiate(playerUIPrefab, transform);
        activePlayerUI.gameObject.SetActive(true);
        activePlayerUI.Initialize(player);
    }

    // Hold player UI??
}
