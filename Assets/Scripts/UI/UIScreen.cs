using DG.Tweening;
using System;
using UnityEngine;

public class UIScreen : MonoBehaviour
{
    public string screenName;

    public bool pausesGame;
    public bool blockPlayerInput;
    public bool blockUIInput;
    public bool showCursor;

    public bool isVisible {  get; protected set; }

    public event Action OnOpen;
    public event Action OnClose;

    [SerializeField] protected CanvasGroup cg;
    private Animator animator;
    private RectTransform rt;

    public bool IsTransitioning {  get; private set; }

    private void Awake()
    {
        if(cg == null) cg = GetComponent<CanvasGroup>();

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public void Initialize() { }

    public void Open(object payload)
    {
        gameObject.SetActive(true);
        OnBeforeOpen(payload);

        IsTransitioning = true;

        // Play transition, delay next
        cg.DOKill();
        cg.DOFade(1f, 0.2f)
            .SetUpdate(true)
            .OnComplete(() =>
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
            IsTransitioning = false;
            OnOpened();
            OnOpen?.Invoke();
        });
    }

    public void Close()
    {
        cg.interactable = false;
        cg.blocksRaycasts = false;
        IsTransitioning = true;

        // Play transition, delay next
        cg.DOKill();
        cg.DOFade(0f, 0.2f)
            .SetUpdate(true)
            .OnComplete(() =>
        {
            IsTransitioning = false;
            OnClosed();
            gameObject.SetActive(false);
            OnClose?.Invoke();
        });
    }

    public virtual void HandleCancel()
    {
        // Depends on the screen, default to close
        UIManager.Instance.CloseTop();
    }

    protected virtual void OnBeforeOpen(object payload) { }
    protected virtual void OnOpened() { }
    protected virtual void OnClosed() { }
}
