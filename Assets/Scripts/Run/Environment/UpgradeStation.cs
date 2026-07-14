using UnityEngine;

public class UpgradeStation : MonoBehaviour
{
    [SerializeField] private Collider2D triggerVolume;
    [SerializeField] private SpriteRenderer visual;

    [SerializeField] private Vector2 standPos;

    public bool IsEnabled { get; private set; } = false;

    public void Enable()
    {
        IsEnabled = true;
        triggerVolume.enabled = false;
        // Trigger animation
    }

    public void Disable()
    {
        IsEnabled = false;
        triggerVolume.enabled = false;
        // Trigger Animation
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsEnabled || !collision.CompareTag("Player")) return;

        collision.GetComponent<Player>().Movement.MoveToPosition(standPos);
        // Disable movement allow

        // Activate loot system
    }

    private void OnUsed()
    {
        // Trigger Animation
    }
}
