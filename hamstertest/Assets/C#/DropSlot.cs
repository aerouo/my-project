using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public string correctID; // 在 Inspector 輸入正確答案的名稱（例如：for, println）
    [HideInInspector] public GameObject currentItem; // 當前停留在上面的物件

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            currentItem = eventData.pointerDrag;
            currentItem.transform.SetParent(transform);
            currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }

    // 當 BackToGame 重置時，呼叫這個來清空紀錄
    public void ClearSlot()
    {
        currentItem = null;
    }
}