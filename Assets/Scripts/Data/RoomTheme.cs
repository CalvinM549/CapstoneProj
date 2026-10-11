using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Map/Room Theme")]
public class RoomTheme : ScriptableObject
{
    [Header("Per-Cell-Floor Variation")]
    public Color floorColorA = Color.white;
    public Color floorColorB = Color.white;

    [Min(0.01f)] public float broadNoiseScale = 0.12f;
    [Min(0.01f)] public float fineNoiseScale = 0.7f;
    [Range(0f, 1f)] public float fineNoiseWeight = 0.2f;

    [Header("Floor edge darkening")]
    [Range(0f, 1f)] public float edgeDarkening = 0.12f;
    [Range(1, 4)] public int edgeDarkenRadius = 2;

    [Header("Floor tile swaps")]
    public TileSwap[] floorSwaps;

    [Header("walls layer tint")]
    public Color[] wallTints;

    [Header("Lighting")]
    public Color[] globalLightColours;
    public Vector2 globalLightIntensity = new Vector2(0.85f, 1f);

    public Color[] accentLightColors;
    public Vector2 accentLightIntensity = new Vector2(0.8f, 1.2f);
}

[Serializable]
public class TileSwap
{
    public TileBase from;
    public TileBase[] to; // one is chosen at random

    [Range(0f, 1f)] public float chance = 0.1f;
}