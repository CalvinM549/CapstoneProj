using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CaptureZoneHandler : MonoBehaviour, IObjectiveTracker
{
    public event Action OnEncounterCleared;
    public event Action<float> OnProgressChanged;

    [SerializeField] private float timeToCapture;
    private float timer;

    private Collider2D zone;
    private bool insideZone;
    private bool captured;


    public void Reset()
    {

    }

    public void Setup(RoomData room, RunState run)
    {
        zone = GetComponent<Collider2D>();
        zone.isTrigger = true;

        timer = 0f;
        insideZone = false;
        captured = false;
    }

    public void Tick(float dt)
    {
        if (captured) return;

        if (insideZone && timer < timeToCapture)
            timer += dt;
        else if (timer >= timeToCapture)
        {
            OnEncounterCleared?.Invoke();
            captured = true;
        }

        OnProgressChanged?.Invoke(timer / timeToCapture);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        insideZone = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        insideZone = false;
    }
}
