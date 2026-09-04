using System.Collections;
using UnityEngine;

public class Doorway : MonoBehaviour
{
    [SerializeField] private Collider2D triggerVolume;
    [SerializeField] private Collider2D blockingCollider;

    [SerializeField] private SpriteRenderer doorVisual;

    public Transform entryPoint;
    public Direction direction; // Direction the player must enter from

    private bool preventReentry;
    private Coroutine preventReentryRoutine;

    public RoomNode Destination {  get; private set; }
    public bool IsLocked { get; private set; } = true;

    private void Awake()
    {
        if (blockingCollider != null)
            blockingCollider.enabled = IsLocked;

    }

    public void SetDestination(RoomNode node) => Destination = node;

    public void Lock()
    {
        IsLocked = true;
        triggerVolume.enabled = false;

        if (blockingCollider != null)
            blockingCollider.enabled = true;

        doorVisual.color = Color.red;
        // Trigger door animation
    }

    public void Unlock()
    {
        IsLocked = false;
        triggerVolume.enabled = true;

        if (blockingCollider != null)
            blockingCollider.enabled = false;

        doorVisual.color = Color.white;
        // Trigger animation
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
