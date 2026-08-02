using UnityEngine;
using UnityEngine.UI;

public class RadialMomentumUI : MonoBehaviour
{
    [SerializeField] private RingController controller;
    [SerializeField] private float arcOffset;
    [SerializeField] private float arcDegrees;

    [SerializeField] private float gainLerpSpeed;
    [SerializeField] private float drainLerpSpeed;

    [SerializeField] private float colourLerpSpeed;

    [SerializeField] private Color minColour;
    [SerializeField] private Color maxColour;

    private float lastValue;

    private void Awake()
    {
        BuildUI();
    }

    private void OnEnable()
    {
        GameEvents.OnMomentumChange += HandleMomentumChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnMomentumChange -= HandleMomentumChanged;
    }

    private void BuildUI()
    {
        controller.Initialize(arcOffset, arcDegrees, minColour, maxColour);
        controller.SetFillImmediate(0f);
        lastValue = 0f;
    }

    private void HandleMomentumChanged(float normalized)
    {
        controller.SetFill(normalized, gainLerpSpeed);

        if (Mathf.Abs(normalized - lastValue) >= 0.1f)
        {
            controller.FlashColour(Color.Lerp(Color.red, Color.green, normalized), 0.2f);
        }
        else
        {
            controller.SetColourFromNormal(normalized);
        }

        lastValue = normalized;
    }


}
