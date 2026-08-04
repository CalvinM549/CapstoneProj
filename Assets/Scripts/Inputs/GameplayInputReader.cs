using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayInputReader
{
    private readonly InputSystem_Actions actions;

    // MOVEMENT
    public event Action<Vector2> MoveChanged;
    public event Action DashPressed;

    // COMBAT
    public event Action MeleePressed;
    public event Action MeleeCanceled;

    public event Action RangedPressed;
    public event Action RangedCanceled;

    public event Action TargetPressed;

    public event Action ToolPressed;

    // INTERACTION
    public event Action InteractPressed;

    public Vector2 MoveInput {  get; private set; }


    public GameplayInputReader(InputSystem_Actions actions)
    {
        this.actions = actions;
        Subscribe();
    }

    private void Subscribe()
    {
        actions.Gameplay.Move.performed += OnMovePerformed;
        actions.Gameplay.Move.canceled += OnMoveCanceled;
        actions.Gameplay.Dash.performed += OnDashPerformed;

        actions.Gameplay.LightAttack.performed += OnMeleePerformed;
        actions.Gameplay.LightAttack.canceled += OnMeleeCanceled;
        actions.Gameplay.RangedAttack.performed += OnRangedPerformed;
        actions.Gameplay.RangedAttack.canceled += OnRangedCanceled;

        actions.Gameplay.LockOn.performed += OnTargetPerformed;

    }

    public void Unsubscribe()
    {
        actions.Gameplay.Move.performed -= OnMovePerformed;
        actions.Gameplay.Move.canceled -= OnMoveCanceled;
        actions.Gameplay.Dash.performed -= OnDashPerformed;

        actions.Gameplay.LightAttack.performed -= OnMeleePerformed;
        actions.Gameplay.LightAttack.canceled -= OnMeleeCanceled;
        actions.Gameplay.RangedAttack.performed -= OnRangedPerformed;
        actions.Gameplay.RangedAttack.canceled -= OnRangedCanceled;

        actions.Gameplay.LockOn.performed -= OnTargetPerformed;
    }

    #region Movement

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        MoveInput = ctx.ReadValue<Vector2>();
        MoveChanged?.Invoke(MoveInput);
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        MoveInput = ctx.ReadValue<Vector2>();
        MoveChanged?.Invoke(MoveInput);
    }

    private void OnDashPerformed(InputAction.CallbackContext ctx)
    {
        DashPressed?.Invoke();
    }

    #endregion

    #region Combat
    private void OnMeleePerformed(InputAction.CallbackContext ctx)
    {
        MeleePressed?.Invoke();
    }

    private void OnMeleeCanceled(InputAction.CallbackContext ctx)
    {
        MeleeCanceled?.Invoke();
    }

    private void OnRangedPerformed(InputAction.CallbackContext ctx)
    {
        RangedPressed?.Invoke();
    }

    private void OnRangedCanceled(InputAction.CallbackContext ctx)
    {
        RangedCanceled?.Invoke();
    }

    private void OnTargetPerformed(InputAction.CallbackContext ctx)
    {
        TargetPressed?.Invoke();
    }

    #endregion
}
