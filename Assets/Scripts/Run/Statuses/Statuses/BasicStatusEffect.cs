using UnityEngine;

// Status with no custom behaviour: everything comes from the base data (stat modifiers, damage over time).
// Use for simple debuffs like Corrode instead of writing an empty subclass each time.
[CreateAssetMenu(menuName = "Status Effects/Basic")]
public class BasicStatusEffect : StatusEffectData { }
