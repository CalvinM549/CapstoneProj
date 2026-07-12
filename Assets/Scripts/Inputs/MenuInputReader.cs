using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuInputReader : MonoBehaviour
{
    private readonly InputSystem_Actions actions;

    public event Action ExitPressed;
    public event Action InventoryTogglePressed;

    public MenuInputReader(InputSystem_Actions actions)
    {
        this.actions = actions;
        Subscribe();
    }

    private void Subscribe()
    {
        actions.Global.Exit.performed += OnExitPerformed;
        actions.Global.Inventory.performed += OnInventoryPerformed;
    }

    public void Unsubscribe()
    {
        actions.Global.Exit.performed -= OnExitPerformed;
        actions.Global.Inventory.performed -= OnInventoryPerformed;
    }

    private void OnExitPerformed(InputAction.CallbackContext ctx)
    {
        ExitPressed?.Invoke();
    }

    private void OnInventoryPerformed(InputAction.CallbackContext ctx)
    {
        InventoryTogglePressed?.Invoke();   
    }
}
