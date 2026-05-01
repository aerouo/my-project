using UnityEngine;
using UnityEngine.EventSystems;
// 1. 引用新的 Input System 命名空間
using UnityEngine.InputSystem;


public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    private Transform startParent;
    private Canvas canvas; // 需要引用 Canvas 來計算正確的位置
    public string buttonType; // 在 Inspector 輸入 "switch", "case" 或 "default"

    private void Awake()
    {
        // 在物件初始時找到父級的 Canvas
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        startParent = transform.parent;
        GetComponent<CanvasGroup>().blocksRaycasts = false; // 讓滑鼠可以穿透按鈕偵測到下方的格子
    }

    public void OnDrag(PointerEventData eventData)
    {
        //transform.position = Input.mousePosition; // 按鈕跟著滑鼠走//會報錯不使用
        // eventData 已經包含了滑鼠位置，我們直接拿來用
        // 將滑鼠位置轉換為 Canvas 上的局部坐標
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            eventData.position,
            eventData.pressEventCamera,
            out position
        );

        // 更新按鈕的位置
        transform.localPosition = position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        // 如果沒放到格子上，就回到原位
        if (transform.parent == startParent)
        {
            transform.position = startPosition;
        }
    }
}