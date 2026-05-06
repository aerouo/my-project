using UnityEngine;
using UnityEngine.EventSystems;

public class DragBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;
    private Transform originalParent;

    public string blockID; // 這個方塊的ID，例如 "print", "class", "main"

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(canvas.transform); // 拖曳時移到最上層
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 如果沒有放到槽裡，回到原位
        if (transform.parent == canvas.transform)
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    public void ReturnToOrigin()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }
}