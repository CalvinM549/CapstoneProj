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

    [SerializeField] private bool isRoot;

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

        rt = GetComponent<RectTransform>();

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public virtual void Initialize() { }

    public void Open(object payload)
    {
        gameObject.SetActive(true);
        OnBeforeOpen(payload);
        IsTransitioning = true;

        // Play transition, delay next
        cg.DOKill();
        rt.DOKill();

        cg.alpha = 0f;
        rt.localScale = new Vector3(1.04f, 0.02f, 1f);

        DOTween.Sequence()
            .Append(cg.DOFade(1f, 0.06f))
            .Join(rt.DOScaleY(1f, 0.16f).SetEase(Ease.OutExpo))
            .Join(rt.DOScaleX(1f, 0.2f).SetEase(Ease.OutQuad))
            .SetUpdate(true)
            .OnComplete(() =>
            {
                cg.interactable = true;
                cg.blocksRaycasts = true;
                IsTransitioning = false;
                OnOpened();
                OnOpen?.Invoke();
            });

        //cg.DOFade(1f, 0.2f)
        //    .SetUpdate(true)
        //    .OnComplete(() =>
        //{
        //    cg.interactable = true;
        //    cg.blocksRaycasts = true;
        //    IsTransitioning = false;
        //    OnOpened();
        //    OnOpen?.Invoke();
        //});
    }

    public void Close()
    {
        cg.interactable = false;
        cg.blocksRaycasts = false;
        IsTransitioning = true;

        cg.DOKill();
        rt.DOKill();

        DOTween.Sequence()
            .Append(rt.DOScaleY(0.02f, 0.1f).SetEase(Ease.InQuad))
            .Join(cg.DOFade(0f, 0.12f))
            .SetUpdate(true)
            .OnComplete(() =>
            {
                IsTransitioning = false;
                rt.localScale = Vector3.one;
                OnClosed();
                gameObject.SetActive(false);
                OnClose?.Invoke();
            });

        // Play transition, delay next
        //cg.DOKill();
        //cg.DOFade(0f, 0.2f)
        //    .SetUpdate(true)
        //    .OnComplete(() =>
        //{
        //    IsTransitioning = false;
        //    OnClosed();
        //    gameObject.SetActive(false);
        //    OnClose?.Invoke();
        //});
    }

    public virtual void HandleCancel()
    {
        if (isRoot) return;
        UIManager.Instance.CloseTop();
    }

    public void SetFocused(bool focused)
    {
        if (!gameObject.activeSelf || IsTransitioning) return;

        cg.interactable = focused;
        cg.blocksRaycasts = focused;

        if (focused) OnFocusRegained();
    }

    protected virtual void OnFocusRegained() { }

    protected virtual void OnBeforeOpen(object payload) { }
    protected virtual void OnOpened() { }
    protected virtual void OnClosed() { }
}
