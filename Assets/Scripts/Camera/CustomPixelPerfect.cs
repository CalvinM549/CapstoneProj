using UnityEngine;
using UnityEngine.UI;

public class CustomPixelPerfect : MonoBehaviour
{
    public int virtualWidth = 480;
    public int virtualHeight = 270;

    public int pixelsPerUnit = 16;

    private RenderTexture renderTexture;
    private RawImage displayImage;

    public bool createCanvas = true;

    private Camera cam;
    private Vector3 camOriginalPos;

    private float fracX, fracY;

    private int prevScreenW, prevScreenH;

    // DebugValues

    private Vector3 debugCamWorldPos;
    private float debugFracX, debugFracY;

    public bool drawDebugGizmos = true;
    public Color snappedColor = Color.red;
    public Color smoothColor = Color.green;

    public float gizmoSize = 0.2f;


    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    private void Start()
    {
        prevScreenW = Screen.width;
        prevScreenH = Screen.height;
        SetupRenderTextureAndDisplay();
    }

    void SetupRenderTextureAndDisplay()
    {
        if (renderTexture == null || renderTexture.width != virtualWidth || renderTexture.height != virtualHeight)
        {
            if (renderTexture != null)
            {
                if (renderTexture.IsCreated())
                    renderTexture.Release();
            }

            renderTexture = new RenderTexture(virtualWidth, virtualHeight, 24);
            renderTexture.filterMode = FilterMode.Point;
            renderTexture.useMipMap = false;
            renderTexture.wrapMode = TextureWrapMode.Clamp;
        }

        cam.targetTexture = renderTexture;

        cam.orthographicSize = (virtualHeight / 2f) / pixelsPerUnit;

        if (displayImage == null && createCanvas)
        {
            CreateCanvasAndRawImage();
        }

        if (displayImage != null)
        {
            displayImage.texture = renderTexture;
        }
    }

    void CreateCanvasAndRawImage()
    {
        GameObject canvasGO = new GameObject("PixelCam_Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -100;

        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject rawGO = new GameObject("PixelCam_RawImage");
        rawGO.transform.SetParent(canvasGO.transform, false);
        displayImage = rawGO.AddComponent<RawImage>();

        RectTransform rect = displayImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void OnPreCull()
    {
        camOriginalPos = cam.transform.position;

        float camPx = camOriginalPos.x * pixelsPerUnit;
        float camPy = camOriginalPos.y * pixelsPerUnit;

        float floorX = Mathf.Floor(camPx);
        float floorY = Mathf.Floor(camPy);

        fracX = camPx - floorX;
        fracY = camPy - floorY;

        cam.transform.position = new Vector3(floorX / pixelsPerUnit, floorY / pixelsPerUnit, camOriginalPos.y);

    }

    private void OnPostRender()
    {
        cam.transform.position = camOriginalPos;

        if (displayImage != null && displayImage.rectTransform != null && renderTexture != null)
        {
            float screenScaleX = Screen.width / (float)virtualWidth;
            float screenScaleY = Screen.height / (float)virtualHeight;

            displayImage.rectTransform.anchoredPosition =
                new Vector2(-fracX * screenScaleX, -fracY * screenScaleY);
        }
    }

    private void Update()
    {
        if (Screen.width != prevScreenW || Screen.height != prevScreenH)
        {
            prevScreenW = Screen.width;
            prevScreenH = Screen.height;

            SetupRenderTextureAndDisplay();
        }
    }


    // For debugging
    private void LateUpdate()
    {
        debugCamWorldPos = cam.transform.position;

        float camPx = debugCamWorldPos.x * pixelsPerUnit;
        float camPy = debugCamWorldPos.y * pixelsPerUnit;

        debugFracX = camPx - Mathf.Floor(camPx);
        debugFracY = camPy - Mathf.Floor(camPy);
    }

    private void OnDisable()
    {
        if (cam != null) cam.targetTexture = null;
        if (renderTexture != null)
        {
            if (renderTexture.IsCreated()) renderTexture.Release();
        }
    }

    void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUI.color = Color.green;
        GUI.Label(new Rect(10, 10, 500, 20),
            $"Camera World Pos: {debugCamWorldPos:F3}");

        GUI.Label(new Rect(10, 30, 500, 20),
            $"Fractional Offset: ({debugFracX:F3}, {debugFracY:F3})");

        GUI.Label(new Rect(10, 50, 500, 20),
            $"Virtual Size: {virtualWidth}x{virtualHeight}");

        if (renderTexture != null)
            GUI.Label(new Rect(10, 70, 500, 20),
                $"RenderTexture Size: {renderTexture.width}x{renderTexture.height}");
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!drawDebugGizmos || !Application.isPlaying) return;

        // Draw snapped camera position
        Vector3 snappedPos = new Vector3(
            Mathf.Floor(debugCamWorldPos.x * pixelsPerUnit) / pixelsPerUnit,
            Mathf.Floor(debugCamWorldPos.y * pixelsPerUnit) / pixelsPerUnit,
            debugCamWorldPos.z
        );

        Gizmos.color = snappedColor;
        Gizmos.DrawSphere(snappedPos, gizmoSize);

        // Draw smooth camera position
        Gizmos.color = smoothColor;
        Gizmos.DrawSphere(debugCamWorldPos, gizmoSize * 0.5f);

        // Optional: draw a line connecting them
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(snappedPos, debugCamWorldPos);
    }
#endif
}
