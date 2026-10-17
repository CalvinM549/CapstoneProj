// Archetype tags used on upgrades / weapons / tools (ILootEntry.SynergyTags).
// Assets store these as plain strings, so use these constants in code to avoid typos.
public static class SynergyTags
{
    public const string Kinetic = "kinetic";      // dash, momentum, speed
    public const string Arsenal = "arsenal";      // ranged, drone, ammo
    public const string Breacher = "breacher";    // heavy hits, stagger, knockback
    public const string Conductor = "conductor";  // status effects
    public const string Salvage = "salvage";      // structure, currency, sustain
    public const string Overclock = "overclock";  // risk / reward

    public static readonly string[] All = { Kinetic, Arsenal, Breacher, Conductor, Salvage, Overclock };
}
