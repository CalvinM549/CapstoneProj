using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerReloadUI : MonoBehaviour
{
    private PlayerUI playerUI;

    [SerializeField] private TextMeshProUGUI ammoTracker;
    [SerializeField] private Image reloadTracker;
    private bool reloadCharged;
    
    private Player player;

    private void Awake()
    {
        playerUI = GetComponent<PlayerUI>();
    }

    private void OnEnable()
    {
        playerUI.Initialized += HandleUIReady;

        if(playerUI.player != null) HandleUIReady(playerUI.player);
    }

    private void OnDisable()
    {
        playerUI.Initialized -= HandleUIReady;

        if (player != null)
        {
            player.Combat.OnReloadChanged -= HandleReloadChanged;
        }
    }

    private void Update()
    {
        UpdateReloadPosition();
    }

    private void HandleUIReady(Player p)
    {
        player = p;

        player.Combat.OnReloadChanged += HandleReloadChanged;
    }

    private void UpdateReloadPosition()
    {

        if (reloadTracker == null || TimescaleManager.IsPaused) return;

        reloadTracker.transform.position = playerUI.mouseWorldPos;
    }

    private void HandleReloadChanged(float value)
    {
        var lastValue = reloadTracker.fillAmount;
        var fillValue = 1 - Mathf.Clamp01(value);
        
        reloadTracker.fillAmount = fillValue;
        if (fillValue >= 0.99f && !reloadCharged)
        {
            reloadTracker.DOKill();

            reloadTracker.transform.localScale = Vector3.one;
            reloadTracker.transform.DOPunchScale(Vector3.one * 0.2f, 0.35f, 6, 0.4f);
            reloadTracker.color = Color.white;
            reloadTracker.DOFade(0.25f, 0.35f);
            reloadCharged = true;
        }

        if (fillValue < lastValue && reloadCharged)
            reloadCharged = false;
    }
}
