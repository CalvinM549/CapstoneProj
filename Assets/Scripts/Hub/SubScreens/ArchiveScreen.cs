using UnityEngine;

public class ArchiveScreen : HubScreen
{
    public override HubState ScreenType => HubState.Archive;
    private CanvasGroup cg;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    public override void OnClose()
    {
        throw new System.NotImplementedException();
    }

    public override void OnOpen(HubManager hub)
    {
        // Display first tab, load profile's archive states
        throw new System.NotImplementedException();
    }
}
