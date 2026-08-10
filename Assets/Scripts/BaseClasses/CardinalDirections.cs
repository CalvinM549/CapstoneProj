using System;
using UnityEngine;

public enum Direction
{
    North,
    East,
    South,
    West
}

[Flags]
public enum DirectionMask

{
    None = 0,
    North = 1 << 0,
    East = 1 << 1,
    South = 1 << 2,
    West = 1 << 3,
    All = North | East | South | West
}



public static class DirectionExtensions
{
    public static Direction Random()
    {
        return (Direction)RNGManager.Instance.rng.Next(0, 4);
    }

    public static Direction Opposite(this Direction dir)
    {
        return dir switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            _ => 0
        };
    }

    public static Vector2Int ToGridOffset(this Direction dir)
    {
        return dir switch
        {
            Direction.North => new Vector2Int(0, 1),
            Direction.South => new Vector2Int(0, -1),
            Direction.East => new Vector2Int(1, 0),
            Direction.West => new Vector2Int(-1, 0),
            _ => Vector2Int.zero
        };
    }

    public static DirectionMask ToMask(this Direction dir)
    {
        return dir switch
        {
            Direction.North => DirectionMask.North,
            Direction.South => DirectionMask.South,
            Direction.East => DirectionMask.East,
            Direction.West => DirectionMask.West,
            _ => DirectionMask.None
        };
    }

    public static void ConnectTo(this RoomNode a, RoomNode b, Direction fromA)
    {
        a.connections[fromA] = b;
        b.connections[fromA.Opposite()] = a;
    }
}