using UnityEngine;

[CreateAssetMenu(fileName = "MajorUpgrade", menuName = "Upgrades/Major/FrameRegen")]
public class Frame_01 : MajorUpgrade
{
    [SerializeField] private int structureHealed;

    protected override void OnApply(Player player)
    {
        base.OnApply(player);

        GameEvents.OnRoomCompleted += HandleRoomCompleted;
    }

    protected override void OnRemove(Player player)
    {
        base.OnRemove(player);

        GameEvents.OnRoomCompleted -= HandleRoomCompleted;
    }

    private void HandleRoomCompleted()
    {
        player.Health.RestoreSegments(structureHealed);
    }
}
