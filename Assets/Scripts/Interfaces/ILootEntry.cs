using System.Collections.Generic;
using UnityEngine;

public interface ILootEntry : IDatabaseEntry
{
    string Name { get; }
    string FlavourDescription { get; }
    string EffectDescription { get; }
    Sprite Icon { get; }

    float BaseDropWeight { get; }
    IReadOnlyList<string> SynergyTags { get; }
    bool Buyable { get; }
    int ShopPrice {  get; }
}