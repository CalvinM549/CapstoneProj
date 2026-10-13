using UnityEngine;

public class MouseParallax : MonoBehaviour
{
    [SerializeField] private Vector2 parallaxEffectMultiplier;

    public float smoothTime = 0.3f;

    private Camera mainCam;

    private Vector3 startPos;
    private Vector3 targetPos;
    void Start()
    {
        mainCam = Camera.main;
        startPos = transform.position;
    }

    void Update()
    {
        Vector3 mousePos = mainCam.ScreenToViewportPoint(InputManager.Instance.GetMousePosition());

        Vector2 remapped = new(
            (mousePos.x - 0.5f) * 2f,
            (mousePos.y - 0.5f) * 2f
            );

        Vector3 offset = new(
            remapped.x * parallaxEffectMultiplier.x,
            remapped.y * parallaxEffectMultiplier.y,
            0f
            );

        targetPos = startPos + offset;

        transform.position = Vector3.Lerp(transform.position, targetPos, smoothTime * Time.deltaTime);
    }
}
