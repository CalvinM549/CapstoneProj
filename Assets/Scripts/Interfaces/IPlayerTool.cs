using UnityEngine;

public interface IPlayerTool
{
    bool IsAvaliable { get; }

    float CooldownProgress { get; }

    bool UseSecondary(Vector2 facingDirection);

    bool TryIntercept(HitData incoming);

    void OnEquipped();

    void OnUnequipped();
}
