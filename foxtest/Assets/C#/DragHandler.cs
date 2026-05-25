using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    private Transform startParent;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        startParent = transform.parent;

        // 關鍵這行：把物件移到同層級的最下方，這樣他就會顯示在最前面
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;// 拖動時讓滑鼠可以穿透，偵測底下的區域
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 2. 將原本的 Input.mousePosition 改成下面這行
        transform.position = Mouse.current.position.ReadValue();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 如果沒有被 DropZone 接收，就彈回原位
        if (transform.parent == startParent || transform.parent.name == "gamePanel")
        {
            transform.position = startPosition;
        }
    }
}