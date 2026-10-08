using UnityEngine;

public class RNGManager : MonoBehaviour
{
    public static RNGManager Instance;

    public System.Random rng;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void InitRNG(int seed)
    {
        rng = new(seed);
    }


    public static System.Random NodeRng(int seed, Vector2Int coords, int salt = 0)
    {
        unchecked { return new System.Random(((seed * 397 ^ coords.x) * 397 ^ coords.y) * 397 ^ salt); }
    }
}
