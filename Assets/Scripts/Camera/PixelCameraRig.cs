using System;
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

    [Tooltip("Render texels per art pixel. 1 = strict pixel grid (moving sprites step a whole art pixel). " +
             "2 or 4 let moving sprites sit between art pixels, removing step jitter on followed objects, " +
             "while sprites still draw with chunky point-sampled pixels.")]
    [SerializeField, Range(1, 4)] private int positionPrecision = 1;

    // Extra texels around the edge so the sub-pixel shift never samples outside the image
    private const int Margin = 1;

    private RenderTexture renderTexture;
    private Vector3 smoothPos;
    private bool snapped;
    private int lastScreenW, lastScreenH;
    private int builtPrecision;

    /// Fired once per frame after both cameras are in their final positions for rendering.
    /// Anything that converts screen to world (custom cursors etc.) should update here.
    public static event Action CamerasPositioned;

    public RenderTexture GameTexture => renderTexture;
    public float PixelScale { get; private set; }
    public Camera GameCamera => gameCamera;
    public Camera ScreenCamera => screenCamera;

    private int TexelsPerUnit => pixelsPerUnit * builtPrecision;

    private void OnEnable()
    {
        if (gameCamera == null || screenCamera == null || presentImage == null)
        {
            Debug.LogError($"{nameof(PixelCameraRig)} on {name} is missing a camera or present image reference.", this);
            enabled = false;
            return;
        }

        CreateRenderTexture();
        lastScreenW = lastScreenH = -1;

        Canvas.preWillRenderCanvases += OnPreRenderCanvases;
        RenderPipelineManager.endContextRendering += OnEndFrame;
    }

    private void OnDisable()
    {
        Canvas.preWillRenderCanvases -= OnPreRenderCanvases;
        RenderPipelineManager.endContextRendering -= OnEndFrame;
        ReleaseRenderTexture();
    }

    private void CreateRenderTexture()
    {
        builtPrecision = Mathf.Clamp(positionPrecision, 1, 4);

        renderTexture = new RenderTexture(
            (virtualWidth + Margin * 2) * builtPrecision,
            (virtualHeight + Margin * 2) * builtPrecision,
            24, RenderTextureFormat.RGB111110Float, RenderTextureReadWrite.Default)
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

    private void ReleaseRenderTexture()
    {
        if (gameCamera != null) gameCamera.targetTexture = null;
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = null;
        }
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
        // Allow changing precision from the inspector at runtime
        if (builtPrecision != Mathf.Clamp(positionPrecision, 1, 4))
        {
            ReleaseRenderTexture();
            CreateRenderTexture();
        }

        if (Screen.width != lastScreenW || Screen.height != lastScreenH)
            UpdateLayout();

        Transform gt = gameCamera.transform;
        smoothPos = gt.position;

        // Snap the game camera to the texel grid
        float tpu = TexelsPerUnit;
        float px = smoothPos.x * tpu;
        float py = smoothPos.y * tpu;
        float sx = Mathf.Round(px);
        float sy = Mathf.Round(py);

        gt.position = new Vector3(sx / tpu, sy / tpu, smoothPos.z);
        gameCamera.orthographicSize = (virtualHeight + Margin * 2) * 0.5f / pixelsPerUnit;
        snapped = true;

        // Shift the visible window by the leftover fraction of a texel
        float fx = subPixelSmoothing ? px - sx : 0f;
        float fy = subPixelSmoothing ? py - sy : 0f;
        float rw = renderTexture.width;
        float rh = renderTexture.height;
        float margin = Margin * builtPrecision;
        presentImage.uvRect = new Rect(
            (margin + fx) / rw, (margin + fy) / rh,
            virtualWidth * builtPrecision / rw, virtualHeight * builtPrecision / rh);

        // Screen camera sees the smooth, unsnapped view
        screenCamera.transform.SetPositionAndRotation(smoothPos, gt.rotation);

        CamerasPositioned?.Invoke();
    }

    // Put the smooth position back so gameplay code never sees the snapped one
    private void OnEndFrame(ScriptableRenderContext context, List<Camera> cameras)
    {
        if (!snapped) return;
        gameCamera.transform.position = smoothPos;
        snapped = false;
    }
}
