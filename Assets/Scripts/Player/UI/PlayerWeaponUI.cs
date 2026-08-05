using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWeaponUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoTracker;
    [SerializeField] private Image reloadTracker;
    private bool reloadCharged;
    
    [SerializeField] private Player player;

    private void Start()
    {
        player.Combat.OnAmmoChanged += HandleAmmoChange;
        player.Combat.OnReloadChanged += HandleReloadChanged;
    }

    private void OnDisable()
    {
        player.Combat.OnAmmoChanged -= HandleAmmoChange;
        player.Combat.OnReloadChanged -= HandleReloadChanged;
    }

    private void Update()
    {
        UpdateReloadPosition();
    }

    private void UpdateReloadPosition()
    {
        if (reloadTracker != null && !TimescaleManager.IsPaused)
        {
            var pos = Camera.main.ScreenToWorldPoint(InputManager.Instance.GetMousePosition());
            reloadTracker.transform.position = new Vector3(pos.x, pos.y, 0f);
        }
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

    private void HandleAmmoChange(int ammo)
    {
        ammoTracker.text = string.Empty;
        for (int i = 0; i < ammo; i++)
        {
            ammoTracker.text += "I ";
        }
    }
}
