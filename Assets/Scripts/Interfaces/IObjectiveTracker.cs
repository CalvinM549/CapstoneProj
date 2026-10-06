using System;
using UnityEngine;

public interface IObjectiveTracker
{
    event Action OnEncounterCleared;
    event Action<float> OnProgressChanged;

    void Setup(MapNode node, RunState run, RunServices services);
    void Tick(float dt);
}
