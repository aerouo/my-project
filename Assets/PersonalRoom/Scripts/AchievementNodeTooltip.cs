using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class AchievementNodeTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip")]
    public GameObject tooltipPanel;

    public TMP_Text txtName;
    public TMP_Text txtCondition;
    public TMP_Text txtStatus;

    [Header("Achievement Info")]
    public string achievementID = "achievement_basic_01";
    public string achievementName = "新手上路";
    public string condition = "完成第一個教學關卡";

    [Header("Node UI")]
    public Image nodeImage;

    [Header("State")]
    public bool unlocked = false;
    public bool completed = false;

    [Header("Color")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    public Color unlockedColor =
        new Color(0.7f, 0.7f, 0.7f, 1f);

    public Color completedColor =
        Color.white;

    [Header("Tooltip Offset")]
    public Vector2 offset = new Vector2(-20f, 20f);

    void Start()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);

        RefreshVisual();
    }

    void Update()
    {
        // Tooltip 跟隨滑鼠
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            tooltipPanel.transform.position =
                Input.mousePosition + new Vector3(offset.x, offset.y, 0);
        }
    }

    // ===== 更新節點外觀 =====
    public void RefreshVisual()
    {
        if (nodeImage == null)
            return;

        // 未解鎖
        if (!unlocked)
        {
            nodeImage.color = lockedColor;
        }

        // 已解鎖但未完成
        else if (!completed)
        {
            nodeImage.color = unlockedColor;
        }

        // 已完成
        else
        {
            nodeImage.color = completedColor;
        }
    }

    // ===== 設定狀態 =====
    public void SetAchievementState(bool isUnlocked, bool isCompleted)
    {
        unlocked = isUnlocked;
        completed = isCompleted;

        RefreshVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipPanel.SetActive(true);

        txtName.text = achievementName;
        txtCondition.text = condition;

        if (!unlocked)
            txtStatus.text = "尚未解鎖";
        else if (completed)
            txtStatus.text = "已完成";
        else
            txtStatus.text = "未完成";
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.SetActive(false);
    }
}