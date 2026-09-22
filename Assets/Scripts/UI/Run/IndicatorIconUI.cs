using UnityEngine;
using UnityEngine.UI;

public class IndicatorIconUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private RectTransform arrow;

    public RectTransform RectTransform => (RectTransform)transform;

    public void SetIcon(Sprite sprite) => icon.sprite = sprite;

    public void SetPointing(bool offscreen, Vector2 direction)
    {
        arrow.gameObject.SetActive(offscreen);
        if (offscreen)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }
}
