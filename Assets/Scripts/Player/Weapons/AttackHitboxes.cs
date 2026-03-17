using System;
using UnityEngine;

public class AttackHitboxes : MonoBehaviour
{
    [Header("Light Attacks")]
    [SerializeField] private Hitbox[] lightHitboxes;

    [SerializeField] private Hitbox heavyHitbox;

    [SerializeField] private Hitbox dashHitbox;

    [SerializeField] private Transform facingPivot;

    public event Action<Collider2D, AttackInfo> OnPlayerHitboxContact;
    private AttackInfo currentAttack;


    private void Awake()
    {
        foreach (var hitbox in lightHitboxes)
            SubscribeHitbox(hitbox);

        SubscribeHitbox(heavyHitbox);
        SubscribeHitbox(dashHitbox);    
    }

    private void RotateToMouse()
    {
        Vector3 mousePos = InputManager.Instance.inputActions.Player.PointerPosition.ReadValue<Vector2>();

        Vector3 mousePosWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
        mousePosWorld.z = 0f;

        Vector3 direction = mousePosWorld - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        facingPivot.rotation = Quaternion.Euler(new Vector3(0,0, angle));

    }

    private void SubscribeHitbox(Hitbox hitbox)
    {
        if (hitbox == null) return;

        hitbox.OnHitDetected += col => OnPlayerHitboxContact?.Invoke(col, currentAttack);
    }

    public void EnableHitBox(AttackInfo attack, int comboCount = 1)
    {
        ResetHitboxes();
        currentAttack = attack;

        RotateToMouse();

        switch (attack.type)
        {
            case AttackType.Light:
                int index = Mathf.Clamp(comboCount - 1, 0, lightHitboxes.Length - 1);
                lightHitboxes[index]?.Activate();
                break;

            case AttackType.Heavy:
                heavyHitbox?.Activate();
                break;

            case AttackType.DashAttack:
                dashHitbox?.Activate();
                break;
        }
    }

    public void ResetHitboxes()
    {
        foreach (var hitbox in lightHitboxes)
            hitbox?.Deactivate();

        heavyHitbox?.Deactivate();
        dashHitbox?.Deactivate();

        currentAttack = null;
    }


}
