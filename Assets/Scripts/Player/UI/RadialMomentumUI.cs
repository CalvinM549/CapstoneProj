using UnityEngine;
using UnityEngine.UI;

public class RadialMomentumUI : MonoBehaviour
{
    [SerializeField] private Image fillBar;
    [SerializeField] private float arcOffset;
    [SerializeField] private float arcDegrees;

    [SerializeField] private float gainLerpSpeed;
    [SerializeField] private float drainLerpSpeed;

    [SerializeField] private float colourLerpSpeed;

    [SerializeField] private Color minColour;
    [SerializeField] private Color maxColour;

    private float currentFill;
    private float targetFill;
    private Color targetColour;

    private void Awake()
    {
        currentFill = 0f;
        targetColour = minColour;
        if (fillBar != null) fillBar.color = targetColour;
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
        fillBar.rectTransform.localRotation = Quaternion.Euler(0f, 0f, arcOffset);
        fillBar.fillAmount = GetAdjustedFill(currentFill);
    }

    private float GetAdjustedFill(float fillAmount)
    {
        fillAmount = Mathf.Clamp01(fillAmount);
        float arcRatio = arcDegrees / 360f;
        return fillAmount * arcRatio;
    }

    private void HandleMomentumChanged(float normalized)
    {
        currentFill = GetAdjustedFill(normalized);

        fillBar.fillAmount = currentFill; // Replace with Tweeners
        fillBar.color = Color.Lerp(minColour, maxColour, normalized);
    }


}
