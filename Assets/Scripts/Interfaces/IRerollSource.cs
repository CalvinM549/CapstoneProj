using System.Collections.Generic;

// Anything that can re-roll a set of loot choices for a cost (reward stations, later shops / events).
public interface IRerollSource
{
    int RerollCost { get; }
    int RerollsUsed { get; }
    bool HasRerollsLeft { get; }
    bool CanAffordReroll { get; }

    // Spends the cost and returns the new offer. False if out of rerolls or unaffordable.
    bool TryReroll(out List<LootResult> newOffer);
}
