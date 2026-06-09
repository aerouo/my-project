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
    public TMP_Text txtReward;
    public TMP_Text txtStatus;

    [Header("Achievement Info")]
    public string achievementID = "achievement_basic_01";
    public string achievementName = "菜鳥新兵，報到！";
    public string condition = "完成所有學習關卡\n（測驗＋影片）";
    public string reward = "橘子皮痛衣";

    [Header("Node UI")]
    public Image nodeImage;

    [Header("State")]
    public bool unlocked = false;
    public bool completed = false;

    [Header("Color")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color unlockedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color completedColor = Color.white;

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
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            tooltipPanel.transform.position =
                Input.mousePosition + new Vector3(offset.x, offset.y, 0);
        }
    }

    public void RefreshVisual()
    {
        if (nodeImage == null)
            return;

        if (!unlocked)
        {
            nodeImage.color = lockedColor;
        }
        else if (!completed)
        {
            nodeImage.color = unlockedColor;
        }
        else
        {
            nodeImage.color = completedColor;
        }
    }

    public void SetAchievementState(bool isUnlocked, bool isCompleted)
    {
        unlocked = isUnlocked;
        completed = isCompleted;

        RefreshVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(true);

        if (txtName != null)
            txtName.text = achievementName;

        if (txtCondition != null)
            txtCondition.text = condition;

        if (txtReward != null)
            txtReward.text = "獎勵：" + reward;

        if (txtStatus != null)
        {
            if (!unlocked)
                txtStatus.text = "尚未解鎖";
            else if (completed)
                txtStatus.text = "已完成";
            else
                txtStatus.text = "未完成";
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
}