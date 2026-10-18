using DG.Tweening;
using System;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    private Camera mainCam;

    [SerializeField] private float lagValue;
    [SerializeField] private CanvasGroup cg;

    [SerializeField] private GameObject cursorPos;
    [SerializeField] private GameObject cursorPos2;
    [SerializeField] private GameObject cursorPos3;

    [HideInInspector] public Player player;
    [HideInInspector] public Transform playerPos;

    public Vector3 mouseWorldPos;

    public event Action<Player> Initialized;
    public event Action CursorUpdated;

    public bool Visible {  get; private set; }

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerTransitionTeleport += HandleTransitionTeleport;
        PixelCameraRig.CamerasPositioned += HandleCamerasPositioned;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerTransitionTeleport -= HandleTransitionTeleport;
        PixelCameraRig.CamerasPositioned -= HandleCamerasPositioned;
    }

    public void Initialize(Player p)
    {
        cg.alpha = 0f;
        Visible = false;

        player = p;
        playerPos = p.transform;

        transform.position = playerPos.position;

        Initialized?.Invoke(p);
    }

    private void Update()
    {
        UpdateMouseWorldPos();
    }

    private void LateUpdate()
    {
        UpdatePosition();
    }

    private void HandleCamerasPositioned()
    {
        if (playerPos == null) return;

        UpdateMouseWorldPos();
        UpdateCustomMouse();
    }

    private void UpdateMouseWorldPos()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        var pos = mainCam.ScreenToWorldPoint(InputManager.Instance.GetMousePosition());
        mouseWorldPos = new Vector3(pos.x, pos.y, 0f);
    }

    private void HandleTransitionTeleport(Vector3 newPos)
    {
        transform.position = newPos;
    }



    public void ToggleUI(bool show)
    {
        if (Visible == show) return;

        float target = show ? 1f : 1f;

        Visible = show;

        cg?.DOKill();
        cg.DOFade(target, 1f);
    }

    public void SetUIAlpha(float alpha)
    {
        cg?.DOKill();
        cg.DOFade(alpha, 1f);
    }

    private void UpdatePosition()
    {
        if (playerPos == null) return;

        float t = 1f - Mathf.Exp(-lagValue * Time.deltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, playerPos.position, t);
    }

    private void UpdateCustomMouse()
    {
        cursorPos.transform.position = mouseWorldPos;

        cursorPos2.transform.position = Vector3.Lerp(playerPos.position, mouseWorldPos, 0.33f);
        cursorPos3.transform.position = Vector3.Lerp(playerPos.position, mouseWorldPos, 0.66f);

        CursorUpdated?.Invoke();
    }
}
