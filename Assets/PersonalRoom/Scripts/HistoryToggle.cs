using UnityEngine;
using UnityEngine.UI;

public class HistoryToggle : MonoBehaviour
{
    [Header("History UI")]
    public GameObject historyContainer;
    public GameObject noDataText;
    public RectTransform historyList;

    [Header("Layout")]
    public LayoutElement recordLayout;     // 自己，例如 Record_Easy / Record_Normal / Record_Hard
    public LayoutElement lessonLayout;     // 外層，例如 AdvancedLesson_01
    public LayoutElement noDataLayout;     // Txt_Nodata 的 LayoutElement

    [Header("同一課程的其他紀錄")]
    public HistoryToggle[] otherToggles;   // 同一排其他紀錄

    [Header("Panel 自動高度")]
    public AutoResizePanel panelBasicResizer; // Panel_Basic / Panel_Advanced / Panel_Challenge

    [Header("是否有資料")]
    public bool hasData = true;

    [Header("Height Settings")]
    public float closedHeight = 200f;
    public float itemHeight = 100f;
    public float itemSpacing = 10f;
    public float emptyHeight = 80f;

    private bool isOpen = false;
    private float currentHistoryHeight = 0f;

    void Start()
    {
        isOpen = false;

        if (historyContainer != null)
            historyContainer.SetActive(false);

        if (historyList != null)
            historyList.gameObject.SetActive(false);

        if (noDataText != null)
            noDataText.SetActive(false);

        RefreshLayout();
    }

    public void ToggleHistory()
    {
        isOpen = !isOpen;

        if (historyContainer != null)
            historyContainer.SetActive(isOpen);

        RefreshLayout();
    }

    void RefreshLayout()
    {
        currentHistoryHeight = CalculateHistoryHeight();

        float recordHeight = closedHeight + currentHistoryHeight;

        SetLayoutHeight(recordLayout, recordHeight);

        if (historyContainer != null)
            SetLayoutHeight(historyContainer, currentHistoryHeight);

        if (noDataLayout != null)
            SetLayoutHeight(noDataLayout, emptyHeight);

        UpdateLessonHeight();
        RefreshUnityLayout();
    }

    float CalculateHistoryHeight()
    {
        if (!isOpen)
        {
            SetHistoryVisible(false);
            SetNoDataVisible(false);
            return 0f;
        }

        if (!hasData)
        {
            SetHistoryVisible(false);
            SetNoDataVisible(true);
            return emptyHeight;
        }

        int count = GetActiveHistoryCount();

        if (count <= 0)
        {
            SetHistoryVisible(false);
            SetNoDataVisible(true);
            return emptyHeight;
        }

        SetHistoryVisible(true);
        SetNoDataVisible(false);

        return count * itemHeight + (count - 1) * itemSpacing;
    }

    int GetActiveHistoryCount()
    {
        if (historyList == null)
            return 0;

        int count = 0;

        foreach (Transform child in historyList)
        {
            if (child.gameObject.activeSelf &&
                child.name.StartsWith("HistoryItem"))
            {
                count++;
            }
        }

        return count;
    }

    void UpdateLessonHeight()
    {
        if (lessonLayout == null)
            return;

        float maxHistoryHeight = currentHistoryHeight;

        if (otherToggles != null)
        {
            foreach (HistoryToggle toggle in otherToggles)
            {
                if (toggle != null)
                {
                    maxHistoryHeight = Mathf.Max(
                        maxHistoryHeight,
                        toggle.GetCurrentHistoryHeight()
                    );
                }
            }
        }

        float lessonHeight = closedHeight + maxHistoryHeight;

        SetLayoutHeight(lessonLayout, lessonHeight);
    }

    public float GetCurrentHistoryHeight()
    {
        return currentHistoryHeight;
    }

    void SetHistoryVisible(bool visible)
    {
        if (historyList != null)
            historyList.gameObject.SetActive(visible);
    }

    void SetNoDataVisible(bool visible)
    {
        if (noDataText != null)
            noDataText.SetActive(visible);
    }

    void SetLayoutHeight(GameObject target, float height)
    {
        if (target == null)
            return;

        LayoutElement layout = target.GetComponent<LayoutElement>();

        if (layout != null)
            layout.preferredHeight = height;
    }

    void SetLayoutHeight(LayoutElement layout, float height)
    {
        if (layout != null)
            layout.preferredHeight = height;
    }

    void RefreshUnityLayout()
    {
        Canvas.ForceUpdateCanvases();

        if (recordLayout != null)
        {
            RectTransform recordRect = recordLayout.GetComponent<RectTransform>();

            if (recordRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(recordRect);
        }

        if (lessonLayout != null)
        {
            RectTransform lessonRect = lessonLayout.GetComponent<RectTransform>();

            if (lessonRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(lessonRect);
        }

        if (panelBasicResizer != null)
            panelBasicResizer.RefreshHeight();

        Canvas.ForceUpdateCanvases();
    }
}