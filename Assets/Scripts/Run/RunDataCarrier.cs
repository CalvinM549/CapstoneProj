using UnityEngine;

public static class RunDataCarrier
{
    public static bool IsNewRun {  get; private set; }

    private static RunConfig preparedConfig;
    private static RunSaveData preparedSaveData;
    
    public static RunConfig ConsumeNewRunData()
    {
        RunConfig temp = preparedConfig;
        preparedConfig = null;
        return temp;
    }

    public static RunSaveData ConsumeSavedRunData()
    {
        var temp = preparedSaveData;
        preparedSaveData = null;
        return temp;
    }

    public static void BuildNewRunData(int seed, RunLoadout loadout, SectorMap map)
    {
        IsNewRun = true;
        preparedConfig = new RunConfig()
        {
            seed = seed,
            loadout = loadout,
            map = map
        };
    }

    public static void BuildSavedRun(RunSaveData saveData)
    {
        IsNewRun = false;
        preparedSaveData = saveData;
    }


}

public class RunConfig
{
    public int seed;
    public RunLoadout loadout;
    public SectorMap map;
}