using DG.Tweening;
using System;
using UnityEngine;

public class UIScreen : MonoBehaviour
{
    public string screenName;

    public bool pausesGame;
    public bool hidesBelow;
    public bool blockPlayerInput;
    public bool blockUIInput;
    public bool showCursor;
    public bool isRoot;

    public bool isHidden {  get; private set; }
    public bool isVisible {  get; protected set; }
    public bool IsTransitioning { get; private set; }


    public event Action OnOpen;
    public event Action OnClose;

    [SerializeField] protected CanvasGroup cg;
    private Animator animator;
    private RectTransform rt;

    private Sequence transition;

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
        isHidden = false;
        gameObject.SetActive(true);
        OnBeforeOpen(payload);

        PlayIn(() =>
        {
            OnOpened();
            OnOpen?.Invoke();
        });
    }

    public void Close(Action onComplete = null)
    {
        PlayOut(() =>
        {
            isHidden = false;
            OnClosed();
            gameObject.SetActive(false);
            OnClose?.Invoke();
            onComplete?.Invoke();
        });
    }

    public void Hide(Action onComplete = null)
    {
        isHidden = true;

        PlayOut(() =>
        {
            gameObject.SetActive(false);
            OnHidden();
            onComplete?.Invoke();
        });
    }

    public void Show()
    {
        isHidden = false;
        gameObject.SetActive(true);
        PlayIn(OnFocusRegained);
    }

    public void CloseImmediate()
    {
        transition.Kill();
        transition = null;

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        rt.localScale = Vector3.one;

        IsTransitioning = false;
        isHidden = false;

        OnClosed();
        gameObject.SetActive(false);
        OnClose?.Invoke();
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
    protected virtual void OnHidden() { }

    private void PlayIn(Action onComplete)
    {
        transition?.Kill();
        IsTransitioning = true;

        cg.interactable = false;
        cg.blocksRaycasts = false;
        cg.alpha = 0f;
        rt.localScale = new Vector3(1.04f, 0.02f, 1f);

        transition = DOTween.Sequence()
            .Append(cg.DOFade(1f, 0.06f))
            .Join(rt.DOScaleY(1f, 0.16f).SetEase(Ease.OutExpo))
            .Join(rt.DOScaleX(1f, 0.2f).SetEase(Ease.OutQuad))
            .SetUpdate(true)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                transition = null;
                cg.interactable = true;
                cg.blocksRaycasts = true;
                IsTransitioning = false;
                onComplete?.Invoke();
            });
    }

    private void PlayOut(Action onComplete)
    {
        transition?.Kill();
        IsTransitioning = true;

        cg.interactable = false;
        cg.blocksRaycasts = false;

        transition = DOTween.Sequence()
            .Append(rt.DOScaleY(0.02f, 0.1f).SetEase(Ease.InQuad))
            .Join(cg.DOFade(0f, 0.12f))
            .SetUpdate(true)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                transition = null;
                IsTransitioning = false;
                rt.localScale = Vector3.one;
                onComplete?.Invoke();
            });
    }

}
