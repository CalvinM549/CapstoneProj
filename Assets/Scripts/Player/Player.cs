using UnityEngine;

public class Player : MonoBehaviour
{
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
    }

    public void SetupNew(RunLoadout loadout)
    {
        if (loadout == null) return;
        ResetPlayerState();

    }

    public void ResetPlayerState()
    {

    }

    public void PrepareForRoomChange()
    {
        Targeting.DropLock();
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

    public Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = InputManager.Instance.GetMousePosition();
        Vector3 mousePosWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
        mousePosWorld.z = 0f;

        return mousePosWorld;
    }
    #endregion

}
