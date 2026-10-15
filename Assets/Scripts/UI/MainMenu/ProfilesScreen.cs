using UnityEngine;

public class ProfilesScreen : UIScreen
{
    [SerializeField] private ProfileSlotUI[] slots;
    [SerializeField] private LightFlicker flicker;

    protected override void OnBeforeOpen(object payload)
    {
        flicker.SetReaching();
        Refresh();
    }
    protected override void OnFocusRegained() => Refresh();

    protected override void OnClosed()
    {
        flicker.SetDormant();
    }

    private void Refresh()
    {
        var profiles = ProfileSaveSystem.GetAllSlots();

        for (int i = 0; i < slots.Length; i++)
        {
            bool exists = i < profiles.Length;
            slots[i].gameObject.SetActive(exists);
            if (exists)
                slots[i].Setup(i, profiles[i], HandleSelect, HandleDelete);
        }
    }

    private void HandleSelect(int slot, PlayerProfile profile)
    {
        SetFocused(false);
        ProfileService.EnterProfile(profile ?? ProfileService.CreateProfile(slot));
    }

    private void HandleDelete(int slot, PlayerProfile profile)
    {
        ProfileService.DeleteProfile(slot);
        Refresh();
    }
}
