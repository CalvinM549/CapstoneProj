using UnityEngine;

public static class RunDataCarrier
{
    private static RunConfig preparedConfig;
    
    public static RunConfig ConsumeData()
    {
        RunConfig temp = preparedConfig;
        preparedConfig = null;

        return temp;
    }

    public static void BuildData(int seed, RunLoadout loadout, RunMap map)
    {
        preparedConfig = new RunConfig()
        {
            seed = seed,
            loadout = loadout,
            map = map
        };
    }


}

public class RunConfig
{
    public int seed;
    public RunLoadout loadout;
    public RunMap map;
}