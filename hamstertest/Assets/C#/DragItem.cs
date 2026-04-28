using UnityEngine;
using UnityEngine.EventSystems;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 startPosition;
    private Transform startParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        // 確保有 CanvasGroup，拖拽時才不會擋住下方的 Raycast 檢測
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position; // 記錄原始位置，沒對準時可以彈回去
        startParent = transform.parent;
        canvasGroup.blocksRaycasts = false; // 拖拽時關閉檢測，底層的 Slot 才能接收到事件
        canvasGroup.alpha = 0.6f; // 增加透明度效果
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 讓方塊跟隨滑鼠
        rectTransform.anchoredPosition += eventData.delta / transform.lossyScale.x;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;

        // 如果沒有碰到正確的 Slot，就彈回原位
        if (transform.parent == startParent)
        {
            transform.position = startPosition;
        }
    }
}