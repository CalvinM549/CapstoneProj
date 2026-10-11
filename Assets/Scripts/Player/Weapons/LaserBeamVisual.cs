using UnityEngine;

public class LaserBeamVisual : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.15f;
    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    public void Play(Vector2 start, Vector2 end)
    {
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        Destroy(gameObject, lifetime);
    }
}
