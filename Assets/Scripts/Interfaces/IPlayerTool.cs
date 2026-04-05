using UnityEngine;

public interface IPlayerTool
{
    bool IsAvaliable { get; }

    bool UseTool(Vector2 direction);

    bool TryIntercept(HitData incoming);

    void UpdateTool();

    void OnEquipped();

    void OnUnequipped();
}
