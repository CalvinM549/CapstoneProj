using System;
using UnityEngine;

public interface IObjectiveTracker
{
    event Action OnEncounterCleared;
    //void Setup(SpawnTable table);
    void Reset();
}
