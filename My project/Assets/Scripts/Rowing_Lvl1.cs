using UnityEngine;
using TMPro;
using System.Collections;

public class Rowing_Lvl1 : MonoBehaviour
{
    [Header("物件連結")]
    public SpriteRenderer leftHandRenderer;
    public Sprite leftHandNormal;
    public Sprite leftHandRow;
    public SpriteRenderer rightHandRenderer;
    public Sprite rightHandNormal;
    public Sprite rightHandRow;
    public TMP_Text missText;

    private GameObject circleInZone = null;

    void Update()
    {
        // 如果手邊沒球，什麼都不做
        if (circleInZone == null) return;

        // 【左鍵測試】
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (circleInZone.CompareTag("Green"))
            {
                HitSuccess(leftHandRenderer, leftHandNormal, leftHandRow);
            }
            else
            {
                StartCoroutine(ShowMiss());
            }
        }

        // 【右鍵測試】
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (circleInZone.CompareTag("Pink"))
            {
                HitSuccess(rightHandRenderer, rightHandNormal, rightHandRow);
            }
            else
            {
                StartCoroutine(ShowMiss());
            }
        }
    }

    void HitSuccess(SpriteRenderer hand, Sprite normal, Sprite row)
    {
        Debug.Log("<color=green>成功擊中！</color>");
        Destroy(circleInZone); // 只有這裡可以 Destroy！
        circleInZone = null;
        StartCoroutine(PlayRowAnim(hand, normal, row));
    }

    IEnumerator PlayRowAnim(SpriteRenderer hand, Sprite normal, Sprite row)
    {
        if (hand != null)
        {
            hand.sprite = row;
            yield return new WaitForSeconds(0.3f);
            hand.sprite = normal;
        }
    }

    IEnumerator ShowMiss()
    {
        Debug.Log("<color=red>按錯了！顯示 MISS</color>");
        if (missText != null)
        {
            missText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            missText.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 當球進來時，只記錄，絕對不刪除
        if (other.CompareTag("Green") || other.CompareTag("Pink"))
        {
            circleInZone = other.gameObject;
            Debug.Log("球進來了：" + other.tag);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == circleInZone)
        {
            Debug.Log("球飛走了，沒按到");
            circleInZone = null;
        }
    }
}