using UnityEngine;

public abstract class HubScreen : MonoBehaviour
{
    public abstract HubState ScreenType { get; }
    protected HubManager manager;

    public virtual void Initialize()
    {

    }

    public virtual void Open(HubManager hub)
    {
        gameObject.SetActive(true);
        OnOpen(hub);
    }

    public virtual void Close()
    {
        OnClose();
        gameObject.SetActive(false);
    }

    public abstract void OnOpen(HubManager hub);
    public abstract void OnClose();
}
