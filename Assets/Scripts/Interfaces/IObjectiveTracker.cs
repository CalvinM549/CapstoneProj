using System;
using UnityEngine;

public interface IObjectiveTracker
{
    event Action OnEncounterCleared;
    event Action<float> OnProgressChanged;

    void Setup(RoomData room, RunState run);
    void Tick(float dt);
    void Reset();
}
