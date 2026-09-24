using System;
using UnityEngine;

public class ShopObjectiveHandler : MonoBehaviour, IObjectiveTracker
{
    public event Action OnEncounterCleared;
    public event Action<float> OnProgressChanged;

    public void Reset()
    {
        //
    }

    public void Setup(RoomData room, RunState run)
    {
        throw new NotImplementedException();
    }

    public void Tick(float dt)
    {
        //
    }
}
