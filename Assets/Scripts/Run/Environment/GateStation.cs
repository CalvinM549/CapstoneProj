using UnityEngine;

public class GateStation : SimpleInteractable
{
    [SerializeField] private float stabilityResoreAmount;

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
