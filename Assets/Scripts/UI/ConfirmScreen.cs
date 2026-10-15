using System;
using TMPro;
using UnityEngine;

public class ConfirmRequest
{
    public readonly string message;
    public readonly string confirmLabel;
    public readonly Action onConfirm;
    public readonly Action onCancel;

    public ConfirmRequest(string message, Action onConfirm, string confirmLabel = "CONFIRM", Action onCancel = null)
    {
        this.message = message;
        this.confirmLabel = confirmLabel;
        this.onConfirm = onConfirm;
        this.onCancel = onCancel;
    }
}

public class ConfirmScreen : UIScreen
{
    [SerializeField] private TypewriterBox messageText;
    [SerializeField] private TerminalButton confirmButton;
    [SerializeField] private TerminalButton cancelButton;

    private ConfirmRequest request;

    protected override void OnBeforeOpen(object payload)
    {
        request = (ConfirmRequest)payload;
        messageText.SetText(request?.message ?? "CONFIRM?"); 
        confirmButton.SetLabel(request?.confirmLabel ?? "CONFIRM");
        cancelButton.SetLabel("CANCEL");
    }

    public void OnConfirmClicked()
    {
        var r = request;
        request = null;
        UIManager.Instance.CloseTop();
        r?.onConfirm?.Invoke();
    }

    public override void HandleCancel()
    {
        var r = request;
        request = null;
        UIManager.Instance.CloseTop();
        r?.onCancel?.Invoke();
    }
}
