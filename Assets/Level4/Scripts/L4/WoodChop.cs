using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class WoodChop : MonoBehaviour, IDropHandler
{
    [Header("飛散設定")]
    public Sprite[] chipSprites;        // 拉入兩張樹幹樹枝圖片
    public int chipCount = 8;           // 飛散碎片數量
    public float flySpeed = 400f;       // 飛散速度
    public float flyDuration = 0.8f;    // 飛散持續時間
    public float fadeDelay = 0.4f;      // 開始消失的延遲

    [Header("完成通知")]
    public WoodChopManager manager;     // 拉入 WoodChopManager

    private bool chopped = false;

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop 觸發！物件：" + gameObject.name);
        if (chopped) return;

        // 確認是斧頭
        AxeInteraction axe = eventData.pointerDrag?.GetComponent<AxeInteraction>();
        if (axe == null) { Debug.Log("不是斧頭！"); return; }

        chopped = true;

        // 先執行飛散，再隱藏
        StartCoroutine(ChopSequence());
    }

    IEnumerator ChopSequence()
    {
        if (manager != null) manager.OnWoodChopped();
        yield return StartCoroutine(SpawnChips());
        gameObject.SetActive(false);
    }

    IEnumerator SpawnChips()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransform selfRect = GetComponent<RectTransform>();

        for (int i = 0; i < chipCount; i++)
        {
            GameObject chip = new GameObject("chip");
            chip.transform.SetParent(canvas.transform, false);

            RectTransform rt = chip.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 80);

            // 用世界座標對齊木頭位置
            rt.position = selfRect.position;

            Image img = chip.AddComponent<Image>();
            img.sprite = chipSprites[Random.Range(0, chipSprites.Length)];

            Vector2 dir = Random.insideUnitCircle.normalized;
            float rotation = Random.Range(0f, 360f);
            float rotSpeed = Random.Range(-300f, 300f);

            StartCoroutine(FlyChip(rt, img, dir, rotation, rotSpeed));
        }

        yield return new WaitForSeconds(flyDuration);
    }

    IEnumerator FlyChip(RectTransform rt, Image img, Vector2 dir, float startRot, float rotSpeed)
    {
        float elapsed = 0f;
        Vector3 startPos = rt.position;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;

            // 移動（世界座標）
            rt.position = startPos + (Vector3)(dir * flySpeed * elapsed);

            // 旋轉
            rt.rotation = Quaternion.Euler(0, 0, startRot + rotSpeed * elapsed);

            // 淡出
            if (elapsed > fadeDelay)
            {
                float fadeT = (elapsed - fadeDelay) / (flyDuration - fadeDelay);
                Color c = img.color;
                c.a = 1f - fadeT;
                img.color = c;
            }

            yield return null;
        }

        Destroy(rt.gameObject);
    }
}