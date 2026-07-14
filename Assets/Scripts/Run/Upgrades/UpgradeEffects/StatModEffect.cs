using UnityEngine;

public class StatModEffect : UpgradeEffect
{
    public string stat;
    public float value;
    public StatModType modType;

    private StatModifier modifier;

    public override void Apply(Player player)
    {
        modifier = new StatModifier(value, modType);
        player.Stats.ApplyStatModifier(stat, modifier);
    }

    public override void Remove(Player player)
    {
        player.Stats.RemoveStatModifier(stat, modifier);
    }
}
