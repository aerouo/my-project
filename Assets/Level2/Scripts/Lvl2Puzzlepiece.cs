using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// PuzzlePiece.cs
/// 掛在 original group 底下每個碎片上（11~33）
/// 需要有 Image 元件
/// </summary>
public class Lvl2PuzzlePiece : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("這塊對應的格子 ID（例如 11、21、33）")]
    public string pieceID;  // 與 PuzzleSlot 的 slotID 對應

    [Header("拖曳時顯示在最上層的 Canvas")]
    public Canvas rootCanvas;  // 拖入場景的 Root Canvas

    private RectTransform rt;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;  // 記住起始位置
    private Transform originalParent;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalPosition = rt.anchoredPosition;
        originalParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 拖曳時移到最上層，避免被其他 UI 擋住
        transform.SetParent(rootCanvas.transform);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false; // 讓 Raycast 能穿透到格子
    }

    public void OnDrag(PointerEventData eventData)
    {
        rt.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 如果沒有放到任何格子（PuzzleSlot 會在 OnDrop 處理），就彈回原位
        ReturnToOrigin();
    }

    /// <summary>彈回原位</summary>
    public void ReturnToOrigin()
    {
        transform.SetParent(originalParent);
        rt.anchoredPosition = originalPosition;
    }

    /// <summary>放對了，碎片隱藏（格子換圖）</summary>
    public void PlaceCorrect()
    {
        gameObject.SetActive(false);
    }
}