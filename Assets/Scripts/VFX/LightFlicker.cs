using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum FlickerMode
{
    Dormant,
    Reaching,
    Active
}

[Serializable]
public class FlickerProfile
{
    [Tooltip("Brightness while stable.")]
    public float baseLevel = 1f;
    [Tooltip("Brightness during a blink. 0 = fully off, above 0 = a brownout dip.")]
    public float dipLevel = 0f;

    [Tooltip("Seconds of stable light between bursts.")]
    public Vector2 calmDuration = new(1.5f, 5f);
    [Tooltip("Blinks per burst (inclusive).")]
    public Vector2Int burstCount = new(1, 4);
    [Tooltip("Length of each blink.")]
    public Vector2 offDuration = new(0.02f, 0.07f);
    [Tooltip("Length of the light between blinks in a burst.")]
    public Vector2 onDuration = new(0.02f, 0.1f);

    [Tooltip("Chance a burst ends with the light dying for a while.")]
    [Range(0f, 1f)] public float outageChance = 0f;
    public Vector2 outageDuration = new(0.5f, 1.5f);

    [Tooltip("Subtle constant wobble on top of the base level, as a fraction of it.")]
    [Range(0f, 0.5f)] public float humAmount = 0.05f;
    public float humSpeed = 8f;
}

/// <summary>
/// Faulty-light flicker: mostly stable, with clustered bursts of short blinks and occasional outages.
/// Drives any mix of a GameObject toggle, a material property on sprites, and Light2D intensity.
/// </summary>
public class LightFlicker : MonoBehaviour
{
    private enum Phase { Calm, BlinkOff, BlinkOn, Outage }

    [Header("Outputs (assign any)")]
    [Tooltip("Toggled off while the level is below the threshold. Must not be this object or a parent of it.")]
    [SerializeField] private GameObject toggleTarget;
    [SerializeField] private float toggleThreshold = 0.15f;

    [Tooltip("Sprites whose material has a float intensity property (e.g. an HDR emission multiplier).")]
    [SerializeField] private SpriteRenderer[] renderers;
    [SerializeField] private string intensityProperty = "_Intensity";

    [SerializeField] private Light2D[] lights;
    [SerializeField] private float lightIntensityScale = 1f;

    [Header("Profiles")]
    [SerializeField] private FlickerMode startMode = FlickerMode.Dormant;

    [SerializeField] private FlickerProfile dormant = new()
    {
        baseLevel = 0.35f, dipLevel = 0f,
        calmDuration = new(2f, 6f), burstCount = new(1, 3),
        offDuration = new(0.03f, 0.1f), onDuration = new(0.02f, 0.08f),
        outageChance = 0.25f, outageDuration = new(0.8f, 2.5f),
        humAmount = 0.08f, humSpeed = 6f
    };

    [SerializeField] private FlickerProfile reaching = new()
    {
        baseLevel = 1.2f, dipLevel = 0.2f,
        calmDuration = new(0.3f, 1.2f), burstCount = new(2, 6),
        offDuration = new(0.02f, 0.05f), onDuration = new(0.02f, 0.06f),
        outageChance = 0f,
        humAmount = 0.12f, humSpeed = 14f
    };

    [SerializeField] private FlickerProfile active = new()
    {
        baseLevel = 1.5f, dipLevel = 0.8f,
        calmDuration = new(4f, 10f), burstCount = new(1, 2),
        offDuration = new(0.02f, 0.04f), onDuration = new(0.03f, 0.06f),
        outageChance = 0f,
        humAmount = 0.03f, humSpeed = 10f
    };

    [Tooltip("How quickly the base brightness blends between modes.")]
    [SerializeField] private float modeBlendSpeed = 6f;

    [Header("Surge")]
    [Tooltip("Brightness used by Surge(), e.g. a flare when a menu option is chosen.")]
    [SerializeField] private float surgeLevel = 3f;

    [Header("Timing")]
    [Tooltip("Keeps flickering while the game is paused or time-slowed (menus).")]
    [SerializeField] private bool useUnscaledTime = true;

    public FlickerMode Mode { get; private set; }
    public float CurrentLevel { get; private set; }

    private Phase phase;
    private float phaseTimer;
    private int blinksRemaining;
    private float blendedBase;
    private float surgeTimer;
    private float humSeed;

    private MaterialPropertyBlock mpb;
    private int propertyId;

    private FlickerProfile Profile => Mode switch
    {
        FlickerMode.Reaching => reaching,
        FlickerMode.Active => active,
        _ => dormant
    };

    private float DeltaTime => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    private float CurrentTime => useUnscaledTime ? Time.unscaledTime : Time.time;

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();
        propertyId = Shader.PropertyToID(intensityProperty);

        if (toggleTarget != null && transform.IsChildOf(toggleTarget.transform))
            Debug.LogWarning($"[LightFlicker] '{name}': toggle target contains this script, so disabling it would stop the flicker. Put the script on a parent or sibling.");
    }

    private void OnEnable()
    {
        Mode = startMode;
        blendedBase = Profile.baseLevel;
        humSeed = UnityEngine.Random.value * 100f;
        surgeTimer = 0f;

        // Random start offset so several lights don't flicker in sync
        phase = Phase.Calm;
        phaseTimer = RandomIn(Profile.calmDuration);

        Apply(blendedBase);
    }

    private void Update()
    {
        float dt = DeltaTime;
        FlickerProfile p = Profile;

        blendedBase = Mathf.Lerp(blendedBase, p.baseLevel, 1f - Mathf.Exp(-modeBlendSpeed * dt));

        phaseTimer -= dt;
        if (phaseTimer <= 0f)
            AdvancePhase(p);

        float level = phase switch
        {
            Phase.BlinkOff => p.dipLevel,
            Phase.Outage => 0f,
            _ => blendedBase * (1f + Hum(p))
        };

        if (surgeTimer > 0f)
        {
            surgeTimer -= dt;
            level = Mathf.Max(level, surgeLevel);
        }

        Apply(level);
    }

    #region Public Controls

    public void SetMode(FlickerMode mode, bool sputter = true)
    {
        if (mode == Mode) return;
        Mode = mode;

        // React straight away: sputter into the new state instead of waiting out the current calm or outage
        if (sputter)
        {
            phase = Phase.Calm;
            phaseTimer = 0f;
        }
        else if (phase == Phase.Outage)
        {
            phase = Phase.Calm;
            phaseTimer = RandomIn(Profile.calmDuration);
        }
    }

    // Parameterless versions for UnityEvents / EventTrigger in the inspector
    public void SetDormant() => SetMode(FlickerMode.Dormant);
    public void SetReaching() => SetMode(FlickerMode.Reaching);
    public void SetActive() => SetMode(FlickerMode.Active);

    /// <summary> Holds the light at surge brightness for a moment, e.g. when a menu option is chosen. </summary>
    public void Surge(float duration)
    {
        surgeTimer = Mathf.Max(surgeTimer, duration);
    }

    #endregion

    private void AdvancePhase(FlickerProfile p)
    {
        switch (phase)
        {
            case Phase.Calm:
                blinksRemaining = UnityEngine.Random.Range(p.burstCount.x, p.burstCount.y + 1);
                if (blinksRemaining > 0)
                {
                    phase = Phase.BlinkOff;
                    phaseTimer = RandomIn(p.offDuration);
                }
                else
                {
                    phaseTimer = RandomIn(p.calmDuration);
                }
                break;

            case Phase.BlinkOff:
                blinksRemaining--;
                if (blinksRemaining > 0)
                {
                    phase = Phase.BlinkOn;
                    phaseTimer = RandomIn(p.onDuration);
                }
                else if (UnityEngine.Random.value < p.outageChance)
                {
                    phase = Phase.Outage;
                    phaseTimer = RandomIn(p.outageDuration);
                }
                else
                {
                    phase = Phase.Calm;
                    phaseTimer = RandomIn(p.calmDuration);
                }
                break;

            case Phase.BlinkOn:
                phase = Phase.BlinkOff;
                phaseTimer = RandomIn(p.offDuration);
                break;

            case Phase.Outage:
                // Comes back with a sputter rather than snapping on
                phase = Phase.Calm;
                phaseTimer = 0f;
                break;
        }
    }

    private float Hum(FlickerProfile p)
    {
        if (p.humAmount <= 0f) return 0f;
        float noise = Mathf.PerlinNoise(humSeed, CurrentTime * p.humSpeed);
        return (noise - 0.5f) * 2f * p.humAmount;
    }

    private void Apply(float level)
    {
        CurrentLevel = level;

        if (toggleTarget != null)
        {
            bool on = level >= toggleThreshold;
            if (toggleTarget.activeSelf != on)
                toggleTarget.SetActive(on);
        }

        if (renderers != null)
        {
            foreach (var r in renderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(mpb);
                mpb.SetFloat(propertyId, level);
                r.SetPropertyBlock(mpb);
            }
        }

        if (lights != null)
        {
            foreach (var l in lights)
                if (l != null) l.intensity = level * lightIntensityScale;
        }
    }

    private static float RandomIn(Vector2 range) => UnityEngine.Random.Range(range.x, range.y);
}
