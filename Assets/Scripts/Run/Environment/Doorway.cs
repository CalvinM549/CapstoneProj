using System;
using System.Collections;
using UnityEngine;

public class Doorway : MonoBehaviour
{
    [SerializeField] private Collider2D triggerVolume;
    [SerializeField] private SpriteRenderer doorVisual; // Temp

    [SerializeField] private Animator animator;

    [SerializeField] private DoorwayPreviewUI previewUI;

    [SerializeField] private Sprite indicatorIcon;
    [SerializeField] private Transform indicatorPos;


    private int isOpenHash = Animator.StringToHash("isOpen");
    

    public Direction exitDirection; // Direction the player must enter from
    public event Action<Doorway> OnChosen; 

    public MapNode Destination {  get; private set; }
    public bool IsLocked { get; private set; } = true;

    private void OnDisable()
    {
        if(HUDIndicatorService.Instance != null)
            HUDIndicatorService.Instance.RemoveIndicator(transform);
    }

    public void SetDestination(MapNode node)
    {
        Destination = node;
        if(previewUI != null ) 
            previewUI.InitializeWithDestination(Destination);
    }

    public void Lock()
    {
        IsLocked = true;
        triggerVolume.enabled = false;

        if (doorVisual != null)
            doorVisual.gameObject.SetActive(true);

        if (previewUI != null)
            previewUI.gameObject.SetActive(false);

        if (animator != null)
            animator.SetBool(isOpenHash, false);
    }

    public void Unlock()
    {
        IsLocked = false;
        triggerVolume.enabled = true;

        if (previewUI != null)
            previewUI.gameObject.SetActive(true);

        if (animator != null)
            animator.SetBool(isOpenHash, true);

        HUDIndicatorService.Instance.SpawnIndicator(transform, indicatorIcon, doPing: false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsLocked) return;
        if(!collision.CompareTag("Player")) return;
        OnChosen?.Invoke(this);
    }
}
