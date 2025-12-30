using UnityEngine;
using UnityEngine.EventSystems; // 這是處理拖拽必備的
using UnityEngine.UI;

public class DragArrow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = rectTransform.anchoredPosition; // 記住原本位置
        canvasGroup.alpha = 0.6f; // 拖拽時變透明一點
        canvasGroup.blocksRaycasts = false; // 讓滑鼠可以「穿透」這張圖去偵測下面的方框
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta; // 讓箭頭跟著滑鼠跑
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 檢查滑鼠放掉的地方是不是 Slot
        GameObject hoveredObject = eventData.pointerCurrentRaycast.gameObject;
        if (hoveredObject != null && hoveredObject.CompareTag("Slot"))
        {
            // 吸附到方框位置
            rectTransform.position = hoveredObject.transform.position;
        }
        else
        {
            // 沒對準就彈回原位
            rectTransform.anchoredPosition = startPosition;
        }
    }
}