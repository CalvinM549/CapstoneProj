using UnityEngine;

public class CameraTrackingHelper : MonoBehaviour
{
    [SerializeField] private bool doMouseLookahead;
    [SerializeField] private float mouseInfluence;
    [SerializeField] private float maxDistance;

    private Transform player;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        UpdateMouseLook();
    }

    private void UpdateMouseLook()
    {
        if (!doMouseLookahead) return;
        if (player == null) return;

        // Get mouse position in world space
        Vector3 mousePos = InputManager.Instance.GetMousePosition();
        mousePos.z = player.position.z;

        // Find midpoint/offset toward the mouse
        Vector3 targetPos = Vector3.Lerp(player.position, mousePos, mouseInfluence);

        // Clamp distance from player so it doesn't pan too far away
        Vector3 clampedPos = player.position + Vector3.ClampMagnitude(targetPos - player.position, maxDistance);

        // Move this helper object
        transform.position = clampedPos;

    }
}
