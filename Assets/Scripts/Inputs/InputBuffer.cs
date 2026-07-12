using System;
using UnityEngine;

public class InputBuffer
{
    private readonly float bufferWindow;
    private float bufferedTimestamp = -Mathf.Infinity;

    public InputBuffer(float bufferWindow)
    {
        this.bufferWindow = bufferWindow;
    }

    public bool IsBuffered => Time.time - this.bufferedTimestamp < bufferWindow;

    public void Buffer() => bufferedTimestamp = Time.time;

    public void Clear() => bufferedTimestamp = -Mathf.Infinity;

    public void TryConsume(Func<bool> canExecute, Action execute)
    {
        if (!IsBuffered) return;
        if (!canExecute()) return;

        Clear();
        execute();
    }

}
