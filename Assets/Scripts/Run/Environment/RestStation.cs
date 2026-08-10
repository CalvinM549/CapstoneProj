using UnityEngine;

public class RestStation : SimpleInteractable
{
    private bool isEnabled = false;

    public void Enable()
    {
        isEnabled = true;
    }

    protected override void OnInteract()
    {
        if (!isEnabled) return;
        base.OnInteract();
    }
}
