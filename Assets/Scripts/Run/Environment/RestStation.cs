using UnityEngine;

public class RestStation : SimpleInteractable
{
    private bool isEnabled = false;

    private void OnEnable()
    {
        GameEvents.OnRoomCompleted += HandleRoomCompleted;
    }

    private void OnDisable()
    {
        GameEvents.OnRoomCompleted -= HandleRoomCompleted;
    }

    public void HandleRoomCompleted()
    {
        isEnabled = true;
        // trigger animation

        GetComponent<SpriteRenderer>().color = Color.green; // replace with anim trigger
    }

    protected override void OnInteract()
    {
        if (!isEnabled) return;
        base.OnInteract();

        gameObject.SetActive(false);
    }
}
