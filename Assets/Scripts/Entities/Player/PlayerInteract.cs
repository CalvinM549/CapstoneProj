using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private Player p;

    private GameplayInputReader input;

    private Dictionary<IInteractable, Transform> nearbyInteractables = new();
    private LayerMask interactionLayer; // not sure if needed

    private void Awake()
    {
        p = GetComponentInParent<Player>();
    }

    private void OnEnable()
    {
        input = InputManager.Instance.GameplayInputs;

        input.InteractPressed += HandleInteractPressed;
    }

    private void OnDisable()
    {
        input.InteractPressed -= HandleInteractPressed;
    }

    private void Update()
    {
        
    }

    private void HandleInteractPressed()
    {
        if (TimescaleManager.IsPaused || !p.CanAct) return;

        IInteractable interactable = GetClosestInteractable();
        interactable?.Interact();
    }

    private IInteractable GetClosestInteractable()
    {
        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        foreach (var kvp in nearbyInteractables)
        {
            if(kvp.Value == null)
                continue;

            float distance = Vector3.Distance(transform.position, kvp.Value.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = kvp.Key;
            }
        }

        return closest;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IInteractable>(out var interactable))
        {
            if (!interactable.CanInteract) return;

            nearbyInteractables.Add(interactable, collision.transform);
            interactable.ShowPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IInteractable>(out var interactable))
        {
            if (nearbyInteractables.ContainsKey(interactable))
            {
                nearbyInteractables.Remove(interactable);
                interactable.HidePrompt();
            }

        }
    }
}
