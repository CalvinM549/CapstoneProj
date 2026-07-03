using UnityEngine;

[RequireComponent(typeof(PixelCable))]
public class PixelCableRenderer : MonoBehaviour
{
    public Sprite pixelSprite; // Use a tiny 1x1 or 2x2 crisp pixel sprite
    public Color cableColour = Color.white;
    public float cableThickness = 1f;
    public int pixelsPerUnit = 16; // Match your game's PPU scale

    private PixelCable cableSystem;
    private GameObject[] visualPixels;

    void Start()
    {
        cableSystem = GetComponent<PixelCable>();

        // Pool sprite renderers to draw the dense pixel trail
        int maxPixels = Mathf.CeilToInt(cableSystem.cableLength * pixelsPerUnit * 1.5f);
        visualPixels = new GameObject[maxPixels];

        for (int i = 0; i < maxPixels; i++)
        {
            GameObject obj = new GameObject("CablePixel_" + i);
            obj.transform.parent = this.transform;
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = pixelSprite;
            sr.color = cableColour;
            obj.transform.localScale = Vector3.one * cableThickness;
            obj.SetActive(false);
            visualPixels[i] = obj;
        }
    }

    void LateUpdate()
    {
        var nodes = cableSystem.GetNodes();
        if (nodes.Count == 0) return;

        int pixelIdx = 0;

        for (int i = 0; i < nodes.Count - 1; i++)
        {
            Vector2 start = nodes[i].currentPos;
            Vector2 end = nodes[i + 1].currentPos;

            float distance = Vector2.Distance(start, end);
            int pixelCount = Mathf.Max(1, Mathf.RoundToInt(distance * pixelsPerUnit));

            for (int j = 0; j < pixelCount; j++)
            {
                if (pixelIdx >= visualPixels.Length) break;

                float t = (float)j / pixelCount;
                Vector2 rawPos = Vector2.Lerp(start, end, t);

                // KEY STEP: Snap to the pixel grid manually
                float snapX = Mathf.Round(rawPos.x * pixelsPerUnit) / pixelsPerUnit;
                float snapY = Mathf.Round(rawPos.y * pixelsPerUnit) / pixelsPerUnit;

                visualPixels[pixelIdx].transform.position = new Vector3(snapX, snapY, 0);
                visualPixels[pixelIdx].SetActive(true);
                pixelIdx++;
            }
        }

        // Hide unused pooled pixels
        for (int k = pixelIdx; k < visualPixels.Length; k++)
        {
            visualPixels[k].SetActive(false);
        }
    }
}
