using UnityEngine;

public class UpgradeStation : SimpleInteractable
{
    [SerializeField] private Collider2D triggerVolume;
    [SerializeField] private SpriteRenderer visual;

    [SerializeField] private Vector2 standPos;

    private bool isEnabled = false;

    public void Enable()
    {
        isEnabled = true;
        triggerVolume.enabled = false;
        // Trigger animation
    }

    public void Disable()
    {
        isEnabled = false;
        triggerVolume.enabled = false;
        // Trigger Animation
    }

    protected override void OnInteract()
    {
        if (!isEnabled) return;
        base.OnInteract();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isEnabled || !collision.CompareTag("Player")) return;

        collision.GetComponent<Player>().Movement.MoveToPosition(standPos);
        // Disable movement allow

        // Activate loot system
    }

    private void OnUsed()
    {
        // Trigger Animation
    }
}
