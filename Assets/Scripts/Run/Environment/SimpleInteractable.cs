using System;
using UnityEngine;
using UnityEngine.Events;

public class SimpleInteractable : MonoBehaviour, IInteractable
{
    public UnityEvent OnActivated;
    protected bool used = false;

    [SerializeField] protected bool singleUse = true;

    public void Interact()
    {
        if (singleUse && used) return;

        OnInteract();
        used = true;
    }

    protected virtual void OnInteract()
    {
        OnActivated?.Invoke();
    }
}
