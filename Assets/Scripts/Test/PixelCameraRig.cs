using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// Renders gameplay at a fixed virtual resolution into a RenderTexture, then presents it
/// upscaled on a native-res screen camera that also draws the UI. The screen camera mirrors
/// the game camera's unsnapped position, so it is the one tagged MainCamera.
public class PixelCameraRig : MonoBehaviour
{
    public enum UpscaleMode { IntegerLetterbox, Fit }

    [Header("Cameras")]
    [SerializeField] private Camera gameCamera;     // has the CinemachineBrain, renders to RT
    [SerializeField] private Camera screenCamera;   // tagged MainCamera, renders UI layers
    [SerializeField] private RawImage presentImage; // on a Screen Space - Camera canvas of screenCamera

    [Header("Virtual Resolution")]
    [SerializeField] private int virtualWidth = 480;
    [SerializeField] private int virtualHeight = 270;
    [SerializeField] private int pixelsPerUnit = 16;
    [SerializeField] private UpscaleMode upscaleMode = UpscaleMode.IntegerLetterbox;
    [SerializeField] private bool subPixelSmoothing = true;

    // Extra texels around the edge so the sub-pixel shift never samples outside the image
    private const int Margin = 1;

    private RenderTexture renderTexture;
    private Vector3 smoothPos;
    private bool snapped;
    private int lastScreenW, lastScreenH;

    public RenderTexture GameTexture => renderTexture;
    public float PixelScale { get; private set; }

    private void OnEnable()
    {
        CreateRenderTexture();
        lastScreenW = lastScreenH = -1;

        Canvas.preWillRenderCanvases += OnPreRenderCanvases;
        RenderPipelineManager.endContextRendering += OnEndFrame;
    }

    private void OnDisable()
    {
        Canvas.preWillRenderCanvases -= OnPreRenderCanvases;
        RenderPipelineManager.endContextRendering -= OnEndFrame;

        if (gameCamera != null) gameCamera.targetTexture = null;
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = null;
        }
    }

    private void CreateRenderTexture()
    {
        renderTexture = new RenderTexture(
            virtualWidth + Margin * 2,
            virtualHeight + Margin * 2,
            24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default)
        {
            name = "PixelGameRT",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            useMipMap = false,
            antiAliasing = 1
        };
        renderTexture.Create();

        gameCamera.targetTexture = renderTexture;
        presentImage.texture = renderTexture;
    }

    private void UpdateLayout()
    {
        lastScreenW = Screen.width;
        lastScreenH = Screen.height;

        float scale = Mathf.Min(Screen.width / (float)virtualWidth, Screen.height / (float)virtualHeight);
        if (upscaleMode == UpscaleMode.IntegerLetterbox)
            scale = Mathf.Max(1f, Mathf.Floor(scale));
        PixelScale = scale;

        presentImage.rectTransform.sizeDelta = new Vector2(virtualWidth * scale, virtualHeight * scale);

        // One world unit covers (PPU * scale) screen pixels, matching the upscaled image,
        // so ScreenToWorldPoint and world-space UI line up with the pixel art.
        screenCamera.orthographicSize = Screen.height / (2f * scale * pixelsPerUnit);
    }

    // Runs after Cinemachine's LateUpdate and before UI meshes are rebuilt
    private void OnPreRenderCanvases()
    {
        if (Screen.width != lastScreenW || Screen.height != lastScreenH)
            UpdateLayout();

        Transform gt = gameCamera.transform;
        smoothPos = gt.position;

        // Snap the game camera to the texel grid
        float px = smoothPos.x * pixelsPerUnit;
        float py = smoothPos.y * pixelsPerUnit;
        float sx = Mathf.Round(px);
        float sy = Mathf.Round(py);

        gt.position = new Vector3(sx / pixelsPerUnit, sy / pixelsPerUnit, smoothPos.z);
        gameCamera.orthographicSize = (virtualHeight + Margin * 2) * 0.5f / pixelsPerUnit;
        snapped = true;

        // Shift the visible window by the leftover fraction of a texel
        float fx = subPixelSmoothing ? px - sx : 0f;
        float fy = subPixelSmoothing ? py - sy : 0f;
        float rw = renderTexture.width;
        float rh = renderTexture.height;
        presentImage.uvRect = new Rect(
            (Margin + fx) / rw, (Margin + fy) / rh,
            virtualWidth / rw, virtualHeight / rh);

        // Screen camera sees the smooth, unsnapped view
        screenCamera.transform.SetPositionAndRotation(smoothPos, gt.rotation);
    }

    // Put the smooth position back so gameplay code never sees the snapped one
    private void OnEndFrame(ScriptableRenderContext context, List<Camera> cameras)
    {
        if (!snapped) return;
        gameCamera.transform.position = smoothPos;
        snapped = false;
    }
}