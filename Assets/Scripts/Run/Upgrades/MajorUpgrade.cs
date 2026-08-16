using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MajorUpgrade", menuName = "Upgrades/Major Upgrade")]
public class MajorUpgrade : UpgradeBase
{
    public bool isTemplate = false;

    protected override void OnApply(Player player)
    {
        if (isTemplate) return;
    }

    protected override void OnRemove(Player player)
    {
        if (isTemplate) return;
    }
}
