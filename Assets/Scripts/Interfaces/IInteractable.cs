using UnityEngine;

public interface IInteractable
{
    string InteractPrompt { get; }
    void Interact();
}
