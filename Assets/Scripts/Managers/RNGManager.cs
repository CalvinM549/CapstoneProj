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
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void InitRNG(int seed)
    {
        rng = new(seed);
    }


}
