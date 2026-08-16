using System.Collections.Generic;
using UnityEngine;

public interface IWeightedLoot
{
    float BaseDropWeight { get; }
    IReadOnlyList<string> SynergyTags { get; }
}