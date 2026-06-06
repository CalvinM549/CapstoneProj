using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTerminalSequence", menuName ="Terminal/Sequence Data")]
public class TerminalSequenceData : ScriptableObject
{
    [Header("Display Settings")]
    public float startDelay = 0.5f;

    public float defaultLineDelay = 0.4f; // Delay after each line

    [Header("Lines")]
    public TerminalLine[] lines;
}

[Serializable]
public class TerminalLine
{
    public string text = "<NEW LINE>";

    public TerminalLineType lineType = TerminalLineType.Instant;

    public float delayOverride = -1f; // Override delay after this line

    // Loading Type Settings
    [Header("Loading Type Only")]
    public float loadingDuration = 2f; // how long line runs before resolving
    public float dotCycleSpeed = 0.35f; // seconds per step of dot animation
    public string loadingResultSuffix = "DONE"; // Text displayed after load finished
}

public enum TerminalLineType
{
    Instant,
    Typewriter,
    Loading
}