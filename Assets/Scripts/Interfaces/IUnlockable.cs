// Anything the hub can unlock permanently with meta currency (shards).
// Implemented by PlayerWeapon and PlayerTool; MetaProgressionService decides where ownership is stored.
public interface IUnlockable : ILootEntry
{
    bool UnlockedByDefault { get; }
    bool ListInUnlocks { get; }
    int UnlockCost { get; }
}
