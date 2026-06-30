using UnityEngine;

public class SubUpgrade : UpgradeBase
{
    public MajorUpgrade requiredMajor;

    public SubUpgrade()
    {
        slot = UpgradeSlot.None;
        maxStacks = 1;
    }

}
