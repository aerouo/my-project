using System.Collections;
using UnityEngine;

public class AchievementWireEffect : MonoBehaviour
{
    public enum WireDirection
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }

    [Header("線路物件")]
    public GameObject wireOff;
    public GameObject wireOn;

    [Header("電流光點")]
    public RectTransform electricPulse;

    [Header("電流方向")]
    public WireDirection direction = WireDirection.LeftToRight;

    [Header("動畫設定")]
    public float moveDuration = 0.8f;
    public float waitBeforeLoop = 0.5f;
    public bool loop = true;

    private RectTransform wireRect;
    private Coroutine pulseRoutine;
    private bool isPowered = false;

    void Awake()
    {
        if (wireOn != null)
            wireRect = wireOn.GetComponent<RectTransform>();
    }

    void Start()
    {
        SetPowered(isPowered);
    }

    public void SetPowered(bool powered)
    {
        isPowered = powered;

        if (wireOff != null)
            wireOff.SetActive(true);

        if (wireOn != null)
            wireOn.SetActive(powered);

        if (electricPulse != null)
            electricPulse.gameObject.SetActive(false);

        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }

        if (powered && electricPulse != null && wireRect != null)
            pulseRoutine = StartCoroutine(PlayPulse());
    }

    IEnumerator PlayPulse()
    {
        while (true)
        {
            electricPulse.gameObject.SetActive(true);

            float width = wireRect.rect.width;
            float height = wireRect.rect.height;

            Vector2 startPos = Vector2.zero;
            Vector2 endPos = Vector2.zero;

            switch (direction)
            {
                case WireDirection.LeftToRight:
                    startPos = new Vector2(-width / 2f, 0);
                    endPos = new Vector2(width / 2f, 0);
                    break;

                case WireDirection.RightToLeft:
                    startPos = new Vector2(width / 2f, 0);
                    endPos = new Vector2(-width / 2f, 0);
                    break;

                case WireDirection.TopToBottom:
                    startPos = new Vector2(0, height / 2f);
                    endPos = new Vector2(0, -height / 2f);
                    break;

                case WireDirection.BottomToTop:
                    startPos = new Vector2(0, -height / 2f);
                    endPos = new Vector2(0, height / 2f);
                    break;
            }

            float timer = 0f;

            while (timer < moveDuration)
            {
                timer += Time.deltaTime;
                float t = timer / moveDuration;

                electricPulse.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

                yield return null;
            }

            electricPulse.gameObject.SetActive(false);

            if (!loop)
                yield break;

            yield return new WaitForSeconds(waitBeforeLoop);
        }
    }
}