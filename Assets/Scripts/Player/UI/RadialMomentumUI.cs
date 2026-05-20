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

    [SerializeField] private Color emptyColour;
    [SerializeField] private Color lowColour;
    [SerializeField] private Color midColour;
    [SerializeField] private Color highColour;

    private float currentFill;
    private float targetFill;
    private Color targetColour;

    private void Awake()
    {
        currentFill = 0f;
        targetColour = emptyColour;
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

    private void Update()
    {
        
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
        fillBar.fillAmount = GetAdjustedFill(currentFill); // Replace with Tweeners
    }

    private void HandleMomentumZoneChange(MomentumZone old, MomentumZone current)
    {
        targetColour = current switch
        {
            MomentumZone.Full => highColour,
            MomentumZone.High => highColour,
            MomentumZone.Mid => midColour,
            MomentumZone.Low => lowColour,
            MomentumZone.Empty => emptyColour,
            _ => emptyColour
        };

        // Use tweener
    }

}
