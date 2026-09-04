using System;
using UnityEngine;

public class PlayerMeleeHitboxController : MonoBehaviour
{
    [Header("Light Attacks")]
    [SerializeField] private PlayerMeleeHitbox[] lightHitboxes;

    [SerializeField] private PlayerMeleeHitbox heavyHitbox;

    [SerializeField] private Transform facingPivot;

    public event Action<Collider2D, AttackInfo> OnPlayerHitboxContact;
    private AttackInfo currentAttack;


    private void Awake()
    {
        foreach (var hitbox in lightHitboxes)
            SubscribeHitbox(hitbox);

        SubscribeHitbox(heavyHitbox);
    }

    private void RotateToDir(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        facingPivot.rotation = Quaternion.Euler(new Vector3(0,0, angle));
    }

    private void SubscribeHitbox(PlayerMeleeHitbox hitbox)
    {
        if (hitbox == null) return;

        hitbox.OnHitDetected += col => OnPlayerHitboxContact?.Invoke(col, currentAttack);
    }

    public void EnableHitBox(AttackInfo attack, Vector3 attackDirection, int comboCount = 1)
    {
        ResetHitboxes();
        currentAttack = attack;

        RotateToDir(attackDirection);

        switch (attack.type)
        {
            case AttackType.Light:
                int index = Mathf.Clamp(comboCount - 1, 0, lightHitboxes.Length - 1);
                lightHitboxes[index]?.Activate();
                break;

            case AttackType.Heavy:
                heavyHitbox?.Activate();
                break;
        }
    }

    public void ResetHitboxes()
    {
        foreach (var hitbox in lightHitboxes)
            hitbox?.Deactivate();

        heavyHitbox?.Deactivate();

        currentAttack = null;
    }
}
