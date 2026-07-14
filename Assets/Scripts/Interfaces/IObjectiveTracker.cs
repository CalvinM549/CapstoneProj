using System;
using UnityEngine;

public interface IObjectiveTracker
{
    event Action OnEncounterCleared;
    event Action<float> OnProgressChanged;

    void Setup(RoomData room, CurrentRun run);
    void Tick(float dt);
    void Reset();
}
