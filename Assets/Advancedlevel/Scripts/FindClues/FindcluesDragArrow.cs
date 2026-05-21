using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class FindculesDragArrow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // 確保有 CanvasGroup 才能控制透明度與穿透
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1. 無限補位：如果是在選單區拖拽，就複製一個分身留守
        if (transform.parent == originalParent)
        {
            GameObject clone = Instantiate(gameObject, originalParent);
            clone.name = gameObject.name;
            clone.transform.SetSiblingIndex(transform.GetSiblingIndex());
        }

        startPosition = rectTransform.position;
        canvasGroup.alpha = 0.6f; // 拖拽時變透明一點
        canvasGroup.blocksRaycasts = false; // 關鍵：關閉射線擋住，才能讓 X 光偵測到下方的方框

        // 讓拖拽中的物件顯示在最上層
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 讓箭頭跟隨滑鼠/手指位置
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 2. X 光偵測：獲取滑鼠位置下方的所有物件
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        GameObject slotObject = null;

        // 尋找帶有 Slot 標籤的方框
        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("Slot"))
            {
                slotObject = result.gameObject;
                break;
            }
        }

        if (slotObject != null)
        {
            // 3. 自動取代：刪除方框內原本的所有舊箭頭
            foreach (Transform child in slotObject.transform)
            {
                if (child != transform)
                {
                    Destroy(child.gameObject);
                }
            }

            // 吸附到方框中心並對齊
            transform.SetParent(slotObject.transform);
            rectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            // 如果沒放進方框，直接銷毀（因為原本的位置已經有補位分身了）
            Destroy(gameObject);
        }
    }
}