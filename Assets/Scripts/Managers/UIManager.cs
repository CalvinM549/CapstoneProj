using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private bool cursorVisibleByDefault;

    [SerializeField] private Transform screenParent;

    [SerializeField] private UIScreen[] screenRefs;

    private readonly Dictionary<string, UIScreen> screens = new();
    private readonly List<UIScreen> stack = new();
    
    public UIScreen Current => stack.Count > 0 ? stack[^1] : null;
    public bool ScreenOpen => stack.Count > 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        InputManager.Instance.ExitPressed += HandleExit;
    }

    private void OnDisable()
    {
        InputManager.Instance.ExitPressed -= HandleExit;
    }

    public void Initialize()
    {
        SetupScreenRefs();
    }

    private void SetupScreenRefs()
    {
        screens.Clear();

        foreach (var screenRef in screenRefs)
        {
            screens.Add(screenRef.screenName, screenRef);
            screenRef.Initialize();
        }
    }

    private void HandleExit()
    {
        if (Current != null && !Current.IsTransitioning)
        {
            Current.HandleCancel();
        }
        else if (!ScreenOpen)
        {
            OpenScreen("pauseScreen");
        }
    }

    public UIScreen OpenScreen(string id, object payload = null)
    {

        if (!screens.TryGetValue(id, out var screen))
        {
            Debug.LogWarning($"[UIManager] id: {id} does not exist / is not active");
            return null;
        }

        if(stack.Contains(screen)) return screen;

        Current?.SetFocused(false);
        stack.Add(screen);
        screen.transform.SetAsLastSibling();

        if(screen.pausesGame) TimescaleManager.Instance.PauseGame(screen);
        RefreshCursor();

        screen.Open(payload);
        return screen;
    }

    public void CloseTop()
    {
        if (stack.Count <= 0) return;

        var top = stack[^1];
        stack.RemoveAt(stack.Count - 1);

        if(top.pausesGame) TimescaleManager.Instance.UnpauseGame(top);

        top.Close();

        Current?.SetFocused(true);
        RefreshCursor();
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

    private void RefreshCursor()
    {
        bool show = cursorVisibleByDefault || stack.Exists(s => s.showCursor);
        GameManager.Instance.SetCursorActive(show);
    }

}
