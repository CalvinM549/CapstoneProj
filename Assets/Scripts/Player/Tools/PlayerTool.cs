using UnityEngine;

public abstract class PlayerTool : ScriptableObject
{
    public bool IsAvaliable;

    public string ToolName;
    public string ToolDescription;

    public abstract bool UseTool(Vector2 direction);

    public abstract bool TryIntercept(HitData incoming);

    public abstract void OnEquip();

    public abstract void OnUnequip();

    public abstract void UpdateTool();
}
