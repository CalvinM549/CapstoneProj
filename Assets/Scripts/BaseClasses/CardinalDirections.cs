using UnityEngine;

public enum Direction
{
    North,
    East,
    South,
    West
}

public static class DirectionExtensions
{
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
            Direction.East => new Vector2Int(-1, 0),
            Direction.West => new Vector2Int(1, 0),
            _ => Vector2Int.zero
        };
    }

    public static void ConnectTo(this MapNode a, MapNode b, Direction fromA)
    {
        a.connections[fromA] = b;
        b.connections[fromA.Opposite()] = a;
    }
}