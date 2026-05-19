using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum RoomState
{
    Idle,
    Active,
    Completed,
    Failed
}

public class RoomManager : MonoBehaviour // Attach to each level alongside projectile manager?
{
    public event Action<RoomManager> OnLevelCompleted;
    public event Action<RoomManager> OnLevelFailed;

    [SerializeField] private List<GameObject> doors = new();

    public RoomState State { get; private set; } = RoomState.Idle;
    public RoomConfig Config { get; private set; }

    private IRoomObjectiveHandler handler;

    public void Initialize(RoomConfig config)
    {
        Config = config;

        handler = ResolveHandler(config.objective.objectiveType);
    }

    public void ActivateLevel()
    {
        if (State != RoomState.Idle) return;

        State = RoomState.Active;

    }

    private void HandleObjectiveCompleted()
    {

    }

    private void HandleObjectiveFailed()
    {

    }

    private void LockDoors()
    {

    }

    private void UnlockDoors()
    {

    }

    private IRoomObjectiveHandler ResolveHandler(RoomObjectiveType type)
    {
        return type switch
        {
            RoomObjectiveType.Clear => GetOrAdd<ClearHandler>(),
            RoomObjectiveType.Gauntlet => GetOrAdd<GauntletHandler>(),
            RoomObjectiveType.Assassinate => GetOrAdd<AssassinateHandler>(),
            RoomObjectiveType.Infiltrate => GetOrAdd<InfiltrateHandler>()
        };
    }

    private T GetOrAdd<T>() where T : MonoBehaviour, IRoomObjectiveHandler
    {
        return TryGetComponent<T>(out var existing) ? existing : gameObject.AddComponent<T>();
    }



}
