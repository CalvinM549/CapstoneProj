using System;
using UnityEngine;

public class ClearHandler : MonoBehaviour, IRoomObjectiveHandler
{
    public event Action OnObjectiveCompleted;
    public event Action OnObjectiveFailed;

    private int totalEnemies;
    private int killedEnemies;

    public void RegisterEnemies(EnemyBase[] enemies)
    {
        totalEnemies = enemies.Length;
        killedEnemies = 0;


    }

    private void HandleEnemyKilled()
    {

    }

    //

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

    public void TickObjective(float deltaTime) { }

    //
}
