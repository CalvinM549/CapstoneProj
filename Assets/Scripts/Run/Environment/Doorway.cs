using UnityEngine;

public class Doorway : MonoBehaviour
{
    [SerializeField] private Collider2D triggerVolume;
    [SerializeField] private SpriteRenderer doorVisual;

    public Transform entryPoint;
    public Direction direction; // Direction the player must enter from

    public MapNode Destination {  get; private set; }
    public bool IsLocked { get; private set; } = true;

    public void SetDestination(MapNode node) => Destination = node;

    public void Lock()
    {
        IsLocked = true;
        triggerVolume.enabled = false;

        doorVisual.color = Color.red;
        // Trigger door animation
    }

    public void Unlock()
    {
        IsLocked = false;
        triggerVolume.enabled = true;

        doorVisual.color = Color.white;
        // Trigger animation
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsLocked || !collision.CompareTag("Player")) return;

        RunManager.Instance.TransitionTo(Destination, direction);
    }
}
