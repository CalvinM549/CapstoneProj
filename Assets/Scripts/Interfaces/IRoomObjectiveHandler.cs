using System;
using UnityEngine;

public interface IRoomObjectiveHandler
{
    event Action OnObjectiveCompleted;

    event Action OnObjectiveFailed;

    void StartObjective(RoomObjectiveData data);

    void TickObjective(float deltaTime);

    void StopObjective();

    string GetProgressText();
}
