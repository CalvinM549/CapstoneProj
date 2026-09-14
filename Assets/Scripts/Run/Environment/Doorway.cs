using System.Collections;
using UnityEngine;

public class Doorway : MonoBehaviour
{
    [SerializeField] private Collider2D triggerVolume;
    [SerializeField] private SpriteRenderer doorVisual; // Temp
    [SerializeField] private GameObject wallFiller;

    [SerializeField] private GameObject doorIndicator;
    [SerializeField] private Animator animator;

    private int isOpenHash = Animator.StringToHash("isOpen");
    
    public Transform entryPoint;
    public Direction direction; // Direction the player must enter from

    private bool preventReentry;
    private Coroutine preventReentryRoutine;

    public MapNode Destination {  get; private set; }
    public bool IsLocked { get; private set; } = true;

    private void Awake()
    {
        //if(animator == null) animator = GetComponent<Animator>();
    }

    public void SetDestination(MapNode node) => Destination = node;

    public void Lock()
    {
        IsLocked = true;
        triggerVolume.enabled = false;

        if(wallFiller != null)
            wallFiller.SetActive(false);

        if (doorVisual != null)
            doorVisual.gameObject.SetActive(true);

        if(doorIndicator != null)
            doorIndicator.SetActive(false);

        doorVisual.color = Color.red;

        //animator.SetBool(isOpenHash, false);
        // Trigger door animation
    }

    public void Unlock()
    {
        IsLocked = false;
        triggerVolume.enabled = true;


        //doorIndicator.SetActive(true);

        doorVisual.color = Color.white;

        //animator.SetBool(isOpenHash, true);
        // Trigger animation
    }

    public void Hide()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsLocked || preventReentry || !collision.CompareTag("Player")) return;

        RunManager.Instance.TransitionTo(Destination, direction);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (preventReentry && collision.CompareTag("Player"))
            ClearEntryPrevention();
    }

    public void DisableUntilPlayerExit(float timeout = 2f)
    {
        preventReentry = true;

        if(preventReentryRoutine != null)
            StopCoroutine(preventReentryRoutine);

        preventReentryRoutine = StartCoroutine(EntryDisableRoutine(timeout));
    }

    private IEnumerator EntryDisableRoutine(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        ClearEntryPrevention();
    }

    private void ClearEntryPrevention()
    {
        preventReentry = false;
        if (preventReentryRoutine != null)
        {
            StopCoroutine(preventReentryRoutine);
            preventReentryRoutine = null;
        }
    }
}
