using UnityEngine;

public enum HubScreen
{
    Main,
    Loadout,
    Draft
}

public class HubManager : MonoBehaviour
{
    public PlayerProfile activeProfile;
    public RunLoadout pendingLoadout;



    private RunLoadout BuildDefaultLoadout()
    {
        return null;
    }

    public void BeginRunFromHub()
    {
        // Save profile

        //SceneLoader.
    }

}
