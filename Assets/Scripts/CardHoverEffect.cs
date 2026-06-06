using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover 位移")]
    public float hoverMoveY = 10f;

    [Header("Hover 放大")]
    public float hoverScale = 1.02f;

    [Header("動畫速度")]
    public float speed = 8f;

    [Header("發光效果")]
    public bool enableGlow = false;
    public Image glowImage;
    [Range(0f, 1f)]
    public float glowAlpha = 0.18f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Vector3 originalScale;

    private Vector2 targetPosition;
    private Vector3 targetScale;
    private float targetGlowAlpha = 0f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;

        targetPosition = originalPosition;
        targetScale = originalScale;

        if (glowImage != null)
        {
            Color c = glowImage.color;
            c.a = 0f;
            glowImage.color = c;
            glowImage.raycastTarget = false;
        }
    }

    void Update()
    {
        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            targetPosition,
            Time.deltaTime * speed
        );

        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetScale,
            Time.deltaTime * speed
        );

        if (enableGlow && glowImage != null)
        {
            Color c = glowImage.color;
            c.a = Mathf.Lerp(c.a, targetGlowAlpha, Time.deltaTime * speed);
            glowImage.color = c;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetPosition = originalPosition + new Vector2(0, hoverMoveY);
        targetScale = originalScale * hoverScale;
        targetGlowAlpha = glowAlpha;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetPosition = originalPosition;
        targetScale = originalScale;
        targetGlowAlpha = 0f;
    }
}