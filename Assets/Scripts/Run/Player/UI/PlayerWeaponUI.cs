using TMPro;
using UnityEngine;

public class PlayerWeaponUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoTracker;
    [SerializeField] private Player player;

    private void Start()
    {
        player.Combat.onAmmoChanged += HandleAmmoChange;
    }

    private void OnDisable()
    {
        player.Combat.onAmmoChanged -= HandleAmmoChange;
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
