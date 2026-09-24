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

    public bool Visible {  get; private set; }

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerTransitionTeleport += HandleTransitionTeleport;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerTransitionTeleport -= HandleTransitionTeleport;
    }

    private void Update()
    {
        var pos = mainCam.ScreenToWorldPoint(InputManager.Instance.GetMousePosition());
        mouseWorldPos = new Vector3(pos.x, pos.y, 0f);
    }

    private void LateUpdate()
    {
        UpdateCustomMouse();
    }

    private void HandleTransitionTeleport(Vector3 newPos)
    {
        transform.position = newPos;
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

    public void ToggleUI(bool show)
    {
        if (Visible == show) return;

        float target = show ? 1f : 0.2f;

        Visible = show;

        cg?.DOKill();
        cg.DOFade(target, 1f);
    }

    private void FixedUpdate()
    {
        if (playerPos == null) return;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition, 
            playerPos.position, 
            lagValue * Time.deltaTime);
    }

    private void UpdateCustomMouse()
    {
        cursorPos.transform.position = mouseWorldPos;

        cursorPos2.transform.position = Vector3.Lerp(playerPos.position, mouseWorldPos, 0.33f);
        cursorPos3.transform.position = Vector3.Lerp(playerPos.position, mouseWorldPos, 0.66f);
    }
}
