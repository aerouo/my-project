using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EatingGame : MonoBehaviour
{
    [Header("Food 設定")]
    public RectTransform foodTransform;   // food 物件的 RectTransform
    public Sprite[] foodSprites;          // 三種食物圖片（拉入3張）
    public float foodStartY = -600f;      // food 起始 Y（畫面外下方）
    public float foodTargetY = 80f;       // food 到達嘴邊的 Y
    public float foodMoveSpeed = 300f;    // 移動速度（pixels/sec）

    [Header("Human 設定")]
    public Image humanImage;              // human 的 Image 組件
    public Sprite humanOpen;             // 張嘴圖
    public Sprite humanEat1;             // 吃吃吃1
    public Sprite humanEat2;             // 吃吃吃2

    [Header("設定")]
    public int pressesPerRound = 10;     // 每輪需按空白鍵次數

    [Header("完成通知")]
    public Level3Manager level3Manager;  // 拉入 Level3Manager

    private Image foodImage;
    private int currentRound = 0;
    private int pressCount = 0;
    private bool isEating = false;
    private bool isMoving = false;
    private bool gameFinished = false;

    void Start()
    {
        foodImage = foodTransform.GetComponent<Image>();
        StartCoroutine(RunGame());
    }

    void Update()
    {
        if (!isEating || gameFinished) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            pressCount++;

            // 交替換圖
            humanImage.sprite = (pressCount % 2 == 1) ? humanEat1 : humanEat2;

            if (pressCount >= pressesPerRound)
            {
                isEating = false;
            }
        }
    }

    IEnumerator RunGame()
    {
        // 打亂食物順序
        int[] order = RandomOrder(foodSprites.Length);

        for (int round = 0; round < foodSprites.Length; round++)
        {
            currentRound = round;
            pressCount = 0;

            // 設定這輪的食物圖片
            foodImage.sprite = foodSprites[order[round]];
            foodImage.enabled = true;

            // food 從畫面外開始
            Vector2 pos = foodTransform.anchoredPosition;
            pos.y = foodStartY;
            foodTransform.anchoredPosition = pos;

            // human 換成張嘴
            humanImage.sprite = humanOpen;

            // food 向上移動到嘴邊
            isMoving = true;
            while (foodTransform.anchoredPosition.y < foodTargetY)
            {
                pos = foodTransform.anchoredPosition;
                pos.y += foodMoveSpeed * Time.deltaTime;
                if (pos.y > foodTargetY) pos.y = foodTargetY;
                foodTransform.anchoredPosition = pos;
                yield return null;
            }
            isMoving = false;

            // 到達嘴邊：隱藏 food，開始按空白鍵
            foodImage.enabled = false;
            isEating = true;

            // 等待按完 10 次
            yield return new WaitUntil(() => !isEating);

            // 這輪結束，human 換回張嘴準備下一輪
            humanImage.sprite = humanOpen;
            yield return new WaitForSeconds(0.5f);
        }

        // 三輪全部完成
        gameFinished = true;
        if (level3Manager != null)
            level3Manager.OnEatingComplete();
    }

    int[] RandomOrder(int count)
    {
        int[] arr = new int[count];
        for (int i = 0; i < count; i++) arr[i] = i;
        for (int i = count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = arr[i]; arr[i] = arr[j]; arr[j] = tmp;
        }
        return arr;
    }
}