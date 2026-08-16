using System;
using UnityEngine;
using UnityEngine.Events;

public class SimpleInteractable : MonoBehaviour, IInteractable
{
    public UnityEvent OnActivated;
    protected bool used = false;

    [SerializeField] protected bool singleUse = true;

    private void Start()
    {
        used = false;
    }

    public void Interact()
    {
        print("Rest interacted1");
        if (singleUse && used) return;

        OnInteract();
        used = true;
    }

    protected virtual void OnInteract()
    {
        OnActivated?.Invoke();
    }
}
