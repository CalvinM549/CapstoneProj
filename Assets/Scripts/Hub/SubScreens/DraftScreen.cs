using UnityEngine;

public class DraftScreen : HubScreen
{
    public override HubState ScreenType => HubState.Draft;
    private int tempSeed; // Generated seed (can be overriden)

    public override void OnClose()
    {
        // Any Cleanup
    }

    public override void OnOpen(HubManager hub)
    {
        manager = hub;


    }

    #region Button functions



    #endregion
}
