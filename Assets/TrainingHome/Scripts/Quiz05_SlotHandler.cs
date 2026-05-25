using UnityEngine;
using UnityEngine.EventSystems;

public class Quiz05_SlotHandler : MonoBehaviour, IDropHandler
{
    public string requiredType; // 在 Inspector 輸入該格子需要的類型 (例如 "switch")
    public GameObject currentSticker; // 存放在此格子的按鈕

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped != null)
        {
            // 將按鈕吸附到格子中心
            dropped.transform.SetParent(transform);
            dropped.transform.localPosition = Vector2.zero;
            currentSticker = dropped;
        }
    }
}