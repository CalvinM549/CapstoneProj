using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Doorway : MonoBehaviour, IInteractable
{
    [SerializeField] private Collider2D triggerZone;
    [SerializeField] private Collider2D blockingCollider;

    [SerializeField] private SpriteRenderer mainVisual;
    [SerializeField] private SpriteMask entryMask;

    [SerializeField] private Animator animator;

    [SerializeField] private DoorwayPreviewUI previewUI;

    [SerializeField] private Transform entryPoint;
    [SerializeField] private Transform exitPoint;

    public Vector2 EntryPos => entryPoint.position;
    public Vector2 ExitPos => exitPoint.position;

    private readonly int isOpenHash = Animator.StringToHash("isOpen");
    

    public Direction exitDirection; // Direction of force applied to player when walked through
    public event Action<Doorway> OnChosen;

    public bool InUse { get; private set; } = true;
    public MapNode Destination {  get; private set; }
    public bool IsLocked { get; private set; } = true;

    public string InteractPrompt => throw new NotImplementedException();

    public bool CanInteract => false;

    private void OnDisable()
    {
        if(HUDIndicatorService.Instance != null)
            HUDIndicatorService.Instance.RemoveIndicator(transform);
    }

    public void SetDestination(MapNode node)
    {
        Destination = node;
        InUse = true;

        if (mainVisual != null) mainVisual.enabled = true;
        if (previewUI != null ) previewUI.InitializeWithDestination(Destination);
    }

    public void SetMask(bool enable) => entryMask.gameObject.SetActive(enable);

    public void SetTint(Color colour) => mainVisual.color = colour;

    public void HideDoor()
    {
        InUse = false;
        IsLocked = true;
        triggerZone.enabled = false;
        blockingCollider.enabled = true;

        if (mainVisual != null) mainVisual.enabled = false;
        if (previewUI != null) previewUI.gameObject.SetActive(false);
    }

    public void Lock()
    {
        IsLocked = true;
        triggerZone.enabled = false;
        blockingCollider.enabled = true;


        if (previewUI != null) previewUI.gameObject.SetActive(false);


        if (animator != null) animator.SetBool(isOpenHash, false);
    }

    public void Unlock()
    {
        IsLocked = false;
        triggerZone.enabled = true;
        blockingCollider.enabled = false;

        if (previewUI != null)
            previewUI.gameObject.SetActive(true);

        if (animator != null)
            animator.SetBool(isOpenHash, true);

        if(HUDIndicatorService.Instance != null)
            HUDIndicatorService.Instance.SpawnIndicator(transform, Destination.type.displayIcon, doPing: false);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsLocked) return;
        if(!collision.CompareTag("Player")) return;
        OnChosen?.Invoke(this);
    }

    public void Interact()
    {
        throw new NotImplementedException();
    }

    public void ShowPrompt()
    {
        throw new NotImplementedException();
    }

    public void HidePrompt()
    {
        throw new NotImplementedException();
    }
}
