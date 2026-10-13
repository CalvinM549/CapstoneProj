using UnityEngine;

public static class RadialUIHelper
{
    public static RectTransform CreatePivot(
        RectTransform container, 
        GameObject iconPrefab, 
        float pivotAngle,
        float radius, 
        out GameObject iconInstance)
    {
        var pivotGO = new GameObject("IconPivot", typeof(RectTransform));
        var pivot = pivotGO.GetComponent<RectTransform>();

        pivot.SetParent(container, false);
        pivot.anchoredPosition = Vector3.zero;
        pivot.sizeDelta = Vector2.zero;
        pivot.localRotation = Quaternion.Euler(0f, 0f, pivotAngle);

        iconInstance = Object.Instantiate(iconPrefab, pivot);
        var iconRect = iconInstance.GetComponent<RectTransform>();
        iconRect.anchoredPosition = new Vector2(0f, radius);
        iconRect.localRotation = Quaternion.Euler(0f, 0f, -pivotAngle);

        return pivot;
    }

    public static float AngleForIndex(int index, int count, float centerAngle, float gapBetween)
    {
        if (count <= 1) return centerAngle;
        float startAngle = centerAngle - ((count - 1) * gapBetween * 0.5f);
        return startAngle + gapBetween * index;
    }
}
