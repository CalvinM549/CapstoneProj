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
    
    public Transform entryPoint;
    public Direction direction; // Direction the player must enter from

    public MapNode Destination {  get; private set; }
    public bool IsLocked { get; private set; } = true;

    private void OnDisable()
    {
        HUDIndicatorService.Instance?.RemoveIndicator(transform);
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

        //doorVisual.color = Color.red;

        if (animator != null)
            animator.SetBool(isOpenHash, false);
        // Trigger door animation
    }

    public void Unlock()
    {
        IsLocked = false;
        triggerVolume.enabled = true;


        //doorIndicator.SetActive(true);

        if (previewUI != null)
            previewUI.gameObject.SetActive(true);


        //doorVisual.color = Color.white;

        if (animator != null)
            animator.SetBool(isOpenHash, true);
        // Trigger animation

        HUDIndicatorService.Instance.SpawnIndicator(transform, indicatorIcon, doPing: false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsLocked) return;
        if(!collision.CompareTag("Player")) return;

        RunManager.Instance.TransitionTo(Destination, direction);
    }
}
