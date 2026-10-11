using System.Collections;
using UnityEngine;

public class GlitchEffect : MonoBehaviour
{
    [SerializeField] private Material glitchMaterial;
    [SerializeField] private AnimationCurve defaultCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private SpriteRenderer sr;
    MaterialPropertyBlock block;
    Material originalMaterial;
    Coroutine glitchRoutine;

    static readonly int IntensityID = Shader.PropertyToID("_Intensity");

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        block = new MaterialPropertyBlock();
        originalMaterial = sr.sharedMaterial;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Y))
            Pulse();
    }
#endif

    public void Pulse(float duration = 0.4f, float peak = 0.8f)
    {
        if(glitchRoutine != null)
            StopCoroutine(glitchRoutine);

        glitchRoutine = StartCoroutine(PulseRoutine(duration, peak));
    }

    private IEnumerator PulseRoutine(float duration, float peak)
    {
        sr.sharedMaterial = glitchMaterial;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            SetIntensity(defaultCurve.Evaluate(t / duration) * peak);
            yield return null;
        }

        SetIntensity(0f);
        sr.sharedMaterial = originalMaterial;
        glitchRoutine = null;
    }

    private void SetIntensity(float value)
    {
        sr.GetPropertyBlock(block);
        block.SetFloat(IntensityID, value);
        sr.SetPropertyBlock(block);
    }
}
