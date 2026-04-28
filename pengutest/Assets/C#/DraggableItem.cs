using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 startPosition;
    private Transform startParent;

    [Header("目標吸附設定")]
    public RectTransform targetSlot;
    public float snapDistance = 100f;

    private LevelManager levelManager;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        // 自動在場景中找到裁判
        levelManager = Object.FindFirstObjectByType<LevelManager>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        startParent = transform.parent;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (targetSlot == null) { ReturnToStart(); return; }

        float dist = Vector3.Distance(transform.position, targetSlot.position);

        if (dist < snapDistance)
        {
            // 成功吸附
            transform.SetParent(targetSlot);
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;

            // 成功後通知裁判
            if (levelManager != null) levelManager.AddScore();

            this.enabled = false;
        }
        else
        {
            ReturnToStart();
        }
    }

    private void ReturnToStart()
    {
        transform.SetParent(startParent);
        transform.position = startPosition;
    }
}