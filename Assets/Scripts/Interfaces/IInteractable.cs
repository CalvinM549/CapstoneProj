using UnityEngine;

public interface IInteractable
{
    string InteractPrompt { get; }
    bool CanInteract { get; }

    void Interact();

    void ShowPrompt();
    void HidePrompt();
}
