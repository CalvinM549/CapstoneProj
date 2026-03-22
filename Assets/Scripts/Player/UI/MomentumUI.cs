using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MomentumUI : MonoBehaviour
{
    [SerializeField] private Image fillBar;
    [SerializeField] private TextMeshProUGUI currentZoneLabel;
    
    
    [SerializeField] private float gainLerpSpeed;
    [SerializeField] private float drainLerpSpeed;

    [SerializeField] private Color emptyColour;
    [SerializeField] private Color lowColour;
    [SerializeField] private Color midColour;
    [SerializeField] private Color highColour;

    [SerializeField] private float colourLerpSpeed;

    private float displayValue = 0f;
    private float targetValue = 0f;
    private Color targetColour;

    private void Awake()
    {
        targetColour = emptyColour;
        if (fillBar != null) fillBar.color = targetColour;
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void Update()
    {
        UpdateFillBar();
        UpdateColour();
    }

    private void UpdateFillBar()
    {
        if (fillBar == null) return;

        float speed = targetValue > displayValue ? gainLerpSpeed : drainLerpSpeed;
        displayValue = Mathf.Lerp(displayValue, targetValue, speed * Time.deltaTime);
    }

    private void UpdateColour()
    {
        if (fillBar == null) return;

        fillBar.color = Color.Lerp(fillBar.color, targetColour, colourLerpSpeed * Time.deltaTime);
    }

    private void HandleMomentumChanged(float normalized)
    {
        targetValue = normalized;
    }

    private void HandleMomentumZoneChange(MomentumZone from, MomentumZone to)
    {
        targetColour = to switch
        {
            MomentumZone.High => highColour,
            MomentumZone.Mid => midColour,
            MomentumZone.Low => lowColour,
            MomentumZone.Empty => emptyColour,
            _ => emptyColour
        };

        if (currentZoneLabel != null)
        {
            currentZoneLabel.text = to switch
            {
                MomentumZone.High => "HIGH",
                MomentumZone.Mid => "MID",
                MomentumZone.Low => "LOW",
                MomentumZone.Empty => "NONE",
                _ => "NONE"
            };

            currentZoneLabel.color = targetColour;
        }
    }
}
