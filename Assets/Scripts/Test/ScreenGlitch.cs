using UnityEngine;

public class ScreenGlitch : MonoBehaviour
{
    public static ScreenGlitch Instance { get; private set; }

    static readonly int IntensityID = Shader.PropertyToID("_GlitchIntensity");
    static readonly int TimeID = Shader.PropertyToID("_GlitchTime");

    [SerializeField] AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    float baseline;          // constant low-level glitch (e.g. weak signal)
    float pulseTime = -1f;
    float pulseDuration;
    float pulsePeak;

    void Awake()
    {
        Instance = this;
        Shader.SetGlobalFloat(IntensityID, 0f);
    }

    void OnDisable() => Shader.SetGlobalFloat(IntensityID, 0f);

    /// <summary>Short burst, e.g. on damage, hacking, teleport.</summary>
    public void Pulse(float duration = 0.4f, float peak = 0.8f)
    {
        pulseDuration = duration;
        pulsePeak = peak;
        pulseTime = 0f;
    }

    /// <summary>Persistent glitch level, e.g. 0.1 while signal is weak. 0 to clear.</summary>
    public void SetBaseline(float value) => baseline = Mathf.Clamp01(value);

    void Update()
    {

#if UNITY_EDITOR

        if (Input.GetKeyDown(KeyCode.U))
            Pulse();

        if (Input.GetKeyDown(KeyCode.I))
            SetBaseline(0.1f);

#endif
        float pulse = 0f;
        if (pulseTime >= 0f)
        {
            pulseTime += Time.unscaledDeltaTime;
            float t = pulseTime / pulseDuration;
            if (t >= 1f) pulseTime = -1f;
            else pulse = pulseCurve.Evaluate(t) * pulsePeak;
        }

        float userScale = SettingsManager.Instance != null ? SettingsManager.Instance.Current.glitchIntensity : 1f;

        Shader.SetGlobalFloat(IntensityID, Mathf.Max(baseline, pulse) * userScale);
        Shader.SetGlobalFloat(TimeID, Time.unscaledTime);
    }
}