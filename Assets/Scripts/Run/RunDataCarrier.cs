using UnityEngine;

public static class RunDataCarrier
{
    public static RunLoadout loadout;
    public static RunMap map;
    public static CurrentRun run;

    public static void ConsumeData()
    {

    }


}

public class RunConfig
{
    public int seed;
    public RunLoadout loadout;
    public RunMap map;
}