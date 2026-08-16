using UnityEngine;

public class RestStation : SimpleInteractable
{
    private bool isEnabled = false;



    public void Enable()
    {
        isEnabled = true;

        GetComponent<SpriteRenderer>().color = Color.green;
    }

    protected override void OnInteract()
    {
        print("Rest interacted2");

        if (!isEnabled) return;
        base.OnInteract();

        gameObject.SetActive(false);
    }
}
