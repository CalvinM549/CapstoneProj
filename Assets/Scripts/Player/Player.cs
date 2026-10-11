using UnityEngine;

public class Player : MonoBehaviour
{
    // References
    public PlayerMovement Movement { get; private set; }
    public PlayerHealth Health { get; private set; }
    public PlayerCombat Combat { get; private set; }
    public PlayerTargeting Targeting { get; private set; }
    public PlayerTools Tools { get; private set; }
    public PlayerUpgrades Upgrades { get; private set; }
    public PlayerMomentum Momentum { get; private set; }
    public PlayerAnimator Animator { get; private set; }
    public PlayerVFX VFX { get; private set; }
    public PlayerStats Stats { get; private set; }
    public PlayerInteract Interaction { get; private set; }

    public bool CanAct { get; private set; }

    public LayerMask WallLayer;
    public LayerMask GroundLayer;
    public LayerMask EnemyLayer;

    private void Awake()
    {
        Movement    = GetComponent<PlayerMovement>();
        Health      = GetComponent<PlayerHealth>();
        Combat      = GetComponent<PlayerCombat>();
        Targeting   = GetComponent<PlayerTargeting>();
        Tools       = GetComponent<PlayerTools>();
        Upgrades    = GetComponent<PlayerUpgrades>();
        Momentum    = GetComponent<PlayerMomentum>();
        Animator    = GetComponent<PlayerAnimator>();
        VFX         = GetComponent<PlayerVFX>();
        Stats       = GetComponent<PlayerStats>();
        Interaction = GetComponentInChildren<PlayerInteract>();
    }

    private void Start()
    {
        if (CameraManager.Instance != null)
            CameraManager.Instance.SetPlayer(transform);
    }

    #region Setup

    public void SetupNew(RunLoadout loadout)
    {
        if (loadout == null)
        {
            Debug.LogWarning("[Player] No loadout provided for setup");
            return;
        }

        ResetPlayerState();

        // Initialize Each section
        Stats.Initialize();

        Movement.Initialize();
        Health.Initialize();
        Upgrades.Initialize();
        Momentum.Initialize();
        Tools.Initialize(loadout.tool);
        Combat.Initialize(loadout.weapon);
    }

    public void SetupFromSave(PlayerSaveData save)
    {
        if (save == null)
        {
            Debug.LogError("[Player] No save data provided");
            return;
        }

        var db = GameManager.Database;

        ResetPlayerState();

        // Unchanged
        Stats.Initialize();
        Health.Initialize();
        Movement.Initialize();

        Momentum.Initialize(save.currentMomentum);

        // Restore from save

        Upgrades.RestoreFromSave(save.upgradeSave, db);
        Health.RestoreFromSave(save.healthSave);
        Tools.RestoreFromSave(save.equipmentSave, db);
        Combat.RestoreFromSave(save.equipmentSave, db);
    }

    #endregion

    public PlayerSaveData PackPlayerState()
    {
        PlayerSaveData save = new()
        {
            healthSave = Health.ToSave(),
            upgradeSave = Upgrades.ToSave(),

            equipmentSave = new PlayerEquipmentSave()
            {
                equippedTool = Tools.HasToolEquipped ? Tools.EquippedTool.Id : null,
                equippedWeapon = Combat.HasWeaponEquipped ? Combat.EquippedWeapon.Id : null,
                currentAmmo = Combat.CurrentAmmo
            },

            currentMomentum = Momentum.CurrentMomentum
        };

        return save;
    }

    public void ResetPlayerState()
    {
        // Kill coroutines
        Targeting.DropLock();
    }

    public void PrepareForRoomChange()
    {
        Targeting.DropLock();
    }

    public void SetPlayerCanAct(bool value)
    {
        CanAct = value;
    }

    #region Utilities

    public Vector2 GetTargetDirection()
    {
        if (Targeting.HasTarget)
        {
            Vector3 direction = (Targeting.LockedTarget.position - transform.position).normalized;
            return direction;
        }

        return GetMouseDirection();
    }

    public Vector2 GetMouseDirection()
    {
        Vector3 direction = (GetMouseWorldPos() - transform.position).normalized;
        return direction;
    }

    public Vector2 GetMouseDirectionFrom(Vector3 pos)
    {
        Vector3 direction = (GetMouseWorldPos() - pos).normalized;
        return direction;
    }

    public Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = InputManager.Instance.GetMousePosition();
        Vector3 mousePosWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
        mousePosWorld.z = 0f;

        return mousePosWorld;
    }
    #endregion

}
