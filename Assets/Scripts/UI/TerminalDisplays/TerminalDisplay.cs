using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent (typeof(TextMeshProUGUI))]
public class TerminalDisplay : MonoBehaviour
{
    [SerializeField] private TerminalSequenceData sequenceData;

    [SerializeField] private float typewriterCharDelay = 0.03f;

    [SerializeField] private string cursorChar = "_";
    [SerializeField] private float cursorBlinkRate = 0.5f;

    [SerializeField] private bool playOnStart = true;

    public UnityEvent onSequenceComplete;

    // ----

    private TextMeshProUGUI label;
    private Coroutine sequenceCoroutine;
    private Coroutine cursorCoroutine;

    private string commitedText = string.Empty;
    private bool cursorVisible;


    private static readonly string[] DotFrames = { ".", "..", "..." };

    // ----

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
        label.text = string.Empty;
    }

    private void Start()
    {
        if (playOnStart && sequenceData != null)
            Play();
    }

    // ----

    public void Play()
    {
        if (sequenceData == null)
            return;

        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        StopCursor();

        commitedText = string.Empty;
        label.text = string.Empty;
        sequenceCoroutine = StartCoroutine(RunSequence());
    }

    public void Play(TerminalSequenceData data)
    {
        sequenceData = data;
        Play();
    }

    public void Stop()
    {
        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        StopCursor();
        sequenceCoroutine = null;
    }

    public void Clear()
    {
        Stop();
        commitedText = string.Empty;
        label.text = string.Empty;
    }

    #region Core Routine

    private IEnumerator RunSequence()
    {
        if (sequenceData.startDelay > 0f)
            yield return new WaitForSeconds(sequenceData.startDelay);

        foreach (TerminalLine line in sequenceData.lines)
        {
            yield return RunLine(line);

            float delay = line.delayOverride >= 0f 
                ? line.delayOverride 
                : sequenceData.defaultLineDelay;

            if (delay > 0f)
                yield return new WaitForSeconds(delay);
        }

        sequenceCoroutine = null;
        onSequenceComplete?.Invoke();
    }

    private IEnumerator RunLine(TerminalLine line)
    {
        switch (line.lineType)
        {
            case TerminalLineType.Instant:
                yield return RunInstantLine(line.text);
                break;

            case TerminalLineType.Typewriter:
                yield return RunTypewriterLine(line.text);
                break;

            case TerminalLineType.Loading:
                yield return RunLoadingLine(line);
                break;
        }
    }

    #endregion
    #region Line Types

    private IEnumerator RunInstantLine(string text)
    {
        yield return null;
        CommitNewLine(text);
    }

    private IEnumerator RunTypewriterLine(string text)
    {
        StartCursor();

        string baseText = commitedText;
        CommitNewLine(string.Empty);

        for (int i = 0; i <= text.Length; i++)
        {
            string partial = text.Substring(0, i);
            commitedText = string.IsNullOrEmpty(baseText) 
                ? partial 
                : baseText + "\n" + partial;

            RedrawWithCursor();
            yield return new WaitForSeconds(typewriterCharDelay);
        }

        StopCursor();
        label.text = commitedText;

    }

    private IEnumerator RunLoadingLine(TerminalLine line)
    {
        StartCursor();

        string baseText = commitedText;
        CommitNewLine(string.Empty);
        for (int i = 0; i <= line.text.Length; i++)
        {
            string partial = line.text.Substring(0, i);
            commitedText = string.IsNullOrEmpty(baseText)
                ? partial
                : baseText + "\n" + partial;

            RedrawWithCursor();
            yield return new WaitForSeconds(typewriterCharDelay);
        }

        float elapsed = 0f;
        int dotFrame = 0;
        float dotTimer = 0f;

        string lineBase = commitedText;

        while (elapsed < line.loadingDuration)
        {
            dotTimer += Time.deltaTime;
            elapsed += Time.deltaTime;

            if (dotTimer >= line.dotCycleSpeed)
            {
                dotTimer -= line.dotCycleSpeed;
                dotFrame = (dotFrame + 1) % DotFrames.Length;
            }

            label.text = lineBase + DotFrames[dotFrame] + (cursorVisible ? cursorChar : string.Empty);
            yield return null;
        }

        StopCursor();
        commitedText = lineBase + line.loadingResultSuffix;
        label.text = commitedText;
    }

    #endregion
    #region Text Helpers

    //private void AppendLine(string text)
    //{
    //    if (string.IsNullOrEmpty(label.text))
    //        label.text = text;
    //    else
    //        label.text += "\n" + text;
    //}

    //private void SetLine(int lineIndex, string newText)
    //{
    //    string[] lines = label.text.Split('\n');

    //    if (lineIndex < 0 || lineIndex >= lines.Length)
    //    {
    //        return; // out of range
    //    }

    //    lines[lineIndex] = newText;
    //    label.text = string.Join("\n", lines);
    //}

    //private int GetCurrentLineCount()
    //{
    //    if (string.IsNullOrEmpty(label.text)) return 0;
    //    return label.text.Split('\n').Length;
    //}

    private void CommitNewLine(string text)
    {
        commitedText = string.IsNullOrEmpty(commitedText) 
            ? text 
            : commitedText + "\n" + text;

        label.text = commitedText;
    }

    #endregion

    #region Cursor Helpers

    private void StartCursor()
    {
        StopCursor();
        cursorVisible = true;
        cursorCoroutine = StartCoroutine(BlinkCursor());
    }

    private void StopCursor()
    {
        if (cursorCoroutine != null)
        {
            StopCoroutine(cursorCoroutine);
            cursorCoroutine = null;
        }
        cursorVisible = false;
    }

    private IEnumerator BlinkCursor()
    {
        while (true)
        {
            yield return new WaitForSeconds(cursorBlinkRate);
            cursorVisible = !cursorVisible;
            RedrawWithCursor();
        }
    }

    private void RedrawWithCursor()
    {
        label.text = cursorVisible 
            ? commitedText + cursorChar 
            : commitedText;
    }

    #endregion
}
