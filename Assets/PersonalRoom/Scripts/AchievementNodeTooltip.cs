using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class AchievementNodeTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip")]
    public GameObject tooltipPanel;

    public TMP_Text txtName;
    public TMP_Text txtCondition;
    public TMP_Text txtStatus;

    [Header("Achievement Info")]
    public string achievementName = "新手上路";
    public string condition = "完成所有基礎教學。";
    public string status = "未完成";

    [Header("Tooltip Offset")]
    public Vector2 offset = new Vector2(-20f, 20f);

    void Start()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipPanel.SetActive(true);

        txtName.text = achievementName;
        txtCondition.text = condition;
        txtStatus.text = status;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.SetActive(false);
    }
}