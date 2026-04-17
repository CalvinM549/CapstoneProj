using UnityEngine;

public abstract class PlayerTool : ScriptableObject
{
    public bool IsAvaliable;

    public string ToolName;
    public string ToolDescription;

    protected Transform playerTransform;

    public abstract bool UseTool(Vector2 direction);

    public abstract bool TryIntercept(HitData incoming);

    public abstract void OnEquip(Transform currentTransform);

    public abstract void OnUnequip();

    public abstract void UpdateTool();
}
