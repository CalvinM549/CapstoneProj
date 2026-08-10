using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    [Header("Config")]
    [SerializeField] private Vector2 parallaxEffectMultiplier;

    private Vector3 lastCameraPosition;
    private Vector3 startPosition;

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        startPosition = transform.position;
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        float newPosX = transform.position.x + (deltaMovement.x * parallaxEffectMultiplier.x);
        float newPosY = transform.position.y + (deltaMovement.y * parallaxEffectMultiplier.y);

        transform.position = new Vector3(newPosX, newPosY, transform.position.z);

        lastCameraPosition = cameraTransform.position;
    }
}
