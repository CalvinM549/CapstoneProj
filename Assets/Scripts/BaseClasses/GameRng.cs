using UnityEngine;

public static class GameRng
{
    public static System.Random NodeRng(int seed, Vector2Int coords, int salt = 0)
    {
        unchecked { return new System.Random(((seed * 397 ^ coords.x) * 397 ^ coords.y) * 397 ^ salt); }
    }
}
