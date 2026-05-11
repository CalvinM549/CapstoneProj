using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerMovement Movement { get; private set; }
    public PlayerHealth Health { get; private set; }
    public PlayerCombat Combat { get; private set; }
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
        Tools       = GetComponent<PlayerTools>();
        Upgrades    = GetComponent<PlayerUpgrades>();
        Momentum    = GetComponent<PlayerMomentum>();
        Animator    = GetComponent<PlayerAnimator>();
        VFX         = GetComponent<PlayerVFX>();
        Stats       = GetComponent<PlayerStats>();
    }

    #region Utilities

    public Vector2 GetMouseDirection()
    {
        Vector3 mousePos = InputManager.Instance.inputActions.Player.PointerPosition.ReadValue<Vector2>();
        Vector3 mousePosWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
        mousePosWorld.z = 0f;
        Vector3 direction = mousePosWorld - transform.position;

        return direction.normalized;
    }



    #endregion

}
