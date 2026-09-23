using System;
using UnityEngine;

public class StationBase : MonoBehaviour, IInteractable
{
    [SerializeField] private bool startEnabled;
    [SerializeField] private Sprite indicatorIcon;
    [SerializeField] private Transform indicatorPos;

    [SerializeField] private CanvasGroup interactDisplay;

    private bool indicatorActive;
    protected bool isEnabled;
    protected bool used;

    protected Animator anim;

    protected MapNode node;
    protected RunState run;

    [SerializeField] private string interactPrompt;
    public string InteractPrompt => interactPrompt;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        if (indicatorPos == null)
            indicatorPos = transform;

        isEnabled = startEnabled;
        used = false;
    }

    private void OnEnable()
    {
        GameEvents.OnRoomCompleted += HandleRoomCompleted;    
    }

    private void OnDisable()
    {
        GameEvents.OnRoomCompleted -= HandleRoomCompleted;
        RemoveIndicator();
    }

    public void Setup(MapNode node, RunState run, Action<RoomManager> onRoomComplete)
    {
        Debug.Log($"Setting up: {gameObject.name}");

        this.node = node;
        this.run = run;

        OnSetup(node, run);
    }

    protected virtual void OnSetup(MapNode node, RunState run) { }

    private void HandleRoomCompleted()
    {
        isEnabled = true;

        // anim / feedback stuff
        // Display VFX and UI helper
        ApplyIndicator();

        if (anim != null)
            anim.SetTrigger("Enable");
    }

    protected void ApplyIndicator()
    {
        if (indicatorActive) return;
        indicatorActive = true;
        HUDIndicatorService.Instance.IndicateStation(indicatorPos, indicatorIcon);
    }

    protected void RemoveIndicator()
    {
        if(!indicatorActive) return;
        indicatorActive = false;
        HUDIndicatorService.Instance.RemoveIndicator(indicatorPos);
    }

    public void Interact()
    {
        OnInteract();
    }

    protected virtual void OnInteract() { }

    public void ShowPrompt()
    {
        interactDisplay.gameObject.SetActive(true);
    }

    public void HidePrompt()
    {
        interactDisplay.gameObject.SetActive(false);
    }
}
