using UnityEngine;
using UnityEngine.UI;

public class AutoResizePanel : MonoBehaviour
{
    public LayoutElement targetLayout;
    public VerticalLayoutGroup verticalLayoutGroup;

    [Header("Height Settings")]
    public float minHeight = 1300f;

    public void RefreshHeight()
    {
        if (targetLayout == null || verticalLayoutGroup == null)
            return;

        float totalHeight = 0f;
        int activeChildCount = 0;

        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeSelf)
                continue;

            LayoutElement childLayout = child.GetComponent<LayoutElement>();

            if (childLayout != null)
                totalHeight += childLayout.preferredHeight;
            else
                totalHeight += ((RectTransform)child).rect.height;

            activeChildCount++;
        }

        if (activeChildCount > 1)
            totalHeight += verticalLayoutGroup.spacing * (activeChildCount - 1);

        totalHeight += verticalLayoutGroup.padding.top;
        totalHeight += verticalLayoutGroup.padding.bottom;

        totalHeight = Mathf.Max(totalHeight, minHeight);

        targetLayout.preferredHeight = totalHeight;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}