using System;
using UnityEngine;

public class AssassinateHandler : MonoBehaviour, IRoomObjectiveHandler
{
    public event Action OnObjectiveCompleted;
    public event Action OnObjectiveFailed;

    public string GetProgressText()
    {
        throw new NotImplementedException();
    }

    public void StartObjective(RoomObjectiveData data)
    {
        throw new NotImplementedException();
    }

    public void StopObjective()
    {
        throw new NotImplementedException();
    }

    public void TickObjective(float deltaTime)
    {
        throw new NotImplementedException();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
