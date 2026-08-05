using System;
using UnityEngine;

public class SurviveTimeHandler : MonoBehaviour, IObjectiveTracker
{
    public event Action OnEncounterCleared;
    public event Action<float> OnProgressChanged;

    private bool timerComplete;

    [SerializeField] private float surviveTime;
    private float timer;

    public void Reset()
    {

    }

    public void Setup(RoomData room, RunState run)
    {
        timer = 0f;
        timerComplete = false;
    }

    public void Tick(float dt)
    {
        if (timerComplete) return;

        if (timer < surviveTime)
            timer += dt;
        else
        {
            OnEncounterCleared?.Invoke();
            timerComplete = true;
        }

        OnProgressChanged?.Invoke(timer / surviveTime);
    }
}
