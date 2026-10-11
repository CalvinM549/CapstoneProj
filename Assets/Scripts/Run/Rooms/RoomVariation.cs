using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

[Serializable]
public struct AmbientToggle
{
    public GameObject target;
    [Range(0f, 1f)] public float chance;
}

public class RoomVariation : MonoBehaviour
{
    [SerializeField] private RoomTheme theme;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap floor;
    [SerializeField] private Tilemap walls;

    [SerializeField] private Tilemap edgeSource;

    [Header("Lights")]
    [SerializeField] private Light2D[] accentLights;

    [Header("Props")]
    [SerializeField] private Doorway[] doors;
    [SerializeField] private AmbientToggle ambient;

    private const int SaltFloor = 1;
    private const int SaltSwap = 2;
    private const int SaltWalls = 3;
    private const int SaltLights = 4;
    private const int SaltSlots = 5;
    private const int SaltAmbient = 6;

    public void Apply(int seed, bool allowTileSwaps = true)
    {
        if (theme == null) return;

        if (floor != null) 
            ProcessFloor(seed, allowTileSwaps);

        if (walls != null)
            TintWalls(seed);

        ApplyLights(seed);
    }

    #region Floor

    private void ProcessFloor(int seed, bool allowTileSwaps)
    {
        var noiseRNG = RNGManager.SaltRng(seed, SaltFloor);
        var swapRNG = RNGManager.SaltRng(seed, SaltSwap);

        float offX = (float)noiseRNG.NextDouble() * 1000f;
        float offY = (float)noiseRNG.NextDouble() * 1000f;

        floor.CompressBounds();

        var changes = new List<TileChangeData>();

        foreach (var cell in floor.cellBounds.allPositionsWithin)
        {
            TileBase tile = floor.GetTile(cell);
            if(tile == null) continue;

            if (allowTileSwaps)
                tile = TrySwap(tile, swapRNG);

            float broad = Mathf.PerlinNoise(offX + cell.x * theme.broadNoiseScale, offY + cell.y * theme.broadNoiseScale);
            float fine = Mathf.PerlinNoise(offX + cell.x * theme.fineNoiseScale, offY + cell.y * theme.fineNoiseScale);
            float t = Mathf.Clamp01(broad * (1f - theme.fineNoiseWeight) + fine * theme.fineNoiseWeight);

            Color c = Color.Lerp(theme.floorColorA, theme.floorColorB, t);

            float edge = EdgeFactor(cell);
            if (edge > 0)
            {
                Color dark = Color.Lerp(c, Color.black, edge * theme.edgeDarkening);
                dark.a = c.a;

                c = dark;
            }

            changes.Add(new TileChangeData(cell, tile, c, floor.GetTransformMatrix(cell)));
        }

        floor.SetTiles(changes.ToArray(), true);
    }

    private TileBase TrySwap(TileBase tile, System.Random rng)
    {
        if (theme.floorSwaps == null) return tile;

        foreach (var swap in theme.floorSwaps)
        {
            if(swap.from != tile) continue;

            double roll = rng.NextDouble();
            int pick = swap.to != null && swap.to.Length > 0 ? rng.Next(swap.to.Length) : 0;

            if (swap.to != null && swap.to.Length > 0 && roll < swap.chance)
                return swap.to[pick];
            return tile;
        }
        return tile;
    }

    private float EdgeFactor(Vector3Int cell)
    {
        if (edgeSource == null || theme.edgeDarkening <= 0f) return 0f;

        int r = theme.edgeDarkenRadius;
        for (int d = 1; d <= r; d++)
        {
            for (int dx = -d; dx <= d; dx++)
            {
                for (int dy = -d; dy <= d; dy++)
                {
                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != d) continue;
                    if (edgeSource.HasTile(new Vector3Int(cell.x + dx, cell.y + dy, 0)))
                        return 1f - (d - 1) / (float)r;
                }
            }
        }


        return 0f;

    }

    #endregion

    #region Walls

    private void TintWalls(int seed)
    {
        var rng = RNGManager.SaltRng(seed, SaltWalls);

        var wallTint = Pick(theme.wallTints, rng, Color.white);
        walls.color = wallTint;

        foreach(var door in doors)
            door.SetTint(wallTint);
    }

    #endregion

    #region Lights

    private void ApplyLights(int seed)
    {
        var rng = RNGManager.SaltRng(seed, SaltLights);

        var globalColor = Pick(theme.globalLightColours, rng, Color.white);
        var globalIntensity = Mathf.Lerp(theme.globalLightIntensity.x, theme.globalLightIntensity.y, (float)rng.NextDouble());

        if (LightManager.Instance != null)
            LightManager.Instance.Set(globalColor, globalIntensity);

        if (accentLights != null)
        {
            foreach (var light in accentLights)
            {
                Color col = Pick(theme.accentLightColors, rng, Color.white);
                float intensity = Mathf.Lerp(theme.accentLightIntensity.x, theme.accentLightIntensity.y, (float)rng.NextDouble());
                if (light == null) continue;

                light.color = col;
                light.intensity = intensity;

            }
        }
    }

    #endregion

    private Color Pick(Color[] options, System.Random rng, Color fallback)
    {
        int i = options != null && options.Length > 0 ? rng.Next(options.Length) : 0;
        return options == null || options.Length == 0 ? fallback : options[i];

    }

#if UNITY_EDITOR
    [ContextMenu("Preview Variation (random seed)")]
    private void PreviewVariation()
    {
        Undo.RegisterFullObjectHierarchyUndo(gameObject, "Preview Room Variation");
        // Tile swaps are skipped so the preview never permanently changes your tile layout.
        Apply(UnityEngine.Random.Range(int.MinValue, int.MaxValue), allowTileSwaps: false);
    }

    [ContextMenu("Clear Preview")]
    private void ClearPreview()
    {
        Undo.RegisterFullObjectHierarchyUndo(gameObject, "Clear Room Variation Preview");

        if (floor != null)
        {
            foreach (var cell in floor.cellBounds.allPositionsWithin)
            {
                if (!floor.HasTile(cell)) continue;
                floor.SetTileFlags(cell, TileFlags.None);
                floor.SetColor(cell, Color.white);
            }
        }
        if (walls != null) walls.color = Color.white;
    }
#endif


}
