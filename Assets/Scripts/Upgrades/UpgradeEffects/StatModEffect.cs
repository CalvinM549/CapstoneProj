using UnityEngine;

public class StatModEffect : UpgradeEffect
{
    public StatType stat;
    public float value;
    public StatModType modType;

    private StatModifier modifier;

    public override void Apply(Player player)
    {
        modifier = new StatModifier(value, modType);
        player.Stats.Get(stat).AddStatModifier(modifier);
    }

    public override void Remove(Player player)
    {
        player.Stats.Get(stat).RemoveStatModifier(modifier);
    }
}
