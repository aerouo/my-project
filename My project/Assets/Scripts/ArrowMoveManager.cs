using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; //

public class ArrowMoveManager : MonoBehaviour
{
    [Header("角色設定")]
    public GameObject player; // 這個還是建議手動拖入一次 YOU

    [Header("自動抓取的物件")]
    public Transform[] slots = new Transform[6];     // 下方灰色格子
    public Transform[] footPrints = new Transform[6]; // 場景黃色方塊
    public TextMeshProUGUI failHintText;              // 提示文字

    [Header("位置微調")]
    public float yOffset = 70f; //

    [Header("正確答案設定")]
    public string[] correctAnswers = { "箭頭 (右)", "箭頭 (右)", "箭頭 (右)", "箭頭 (上)", "箭頭 (右)", "箭頭 (右)" };

    private Vector3 playerStartPosition;
    private bool isMoving = false;

    void OnEnable()
    {
        // --- 自動抓取邏輯：省去手動拖拽的煩惱 ---
        for (int i = 0; i < 6; i++)
        {
            // 自動尋找名字叫 "虛線方塊 (1)" ~ (6) 的物件
            GameObject slotObj = GameObject.Find("虛線方塊 (" + (i + 1) + ")");
            if (slotObj != null) slots[i] = slotObj.transform;

            // 自動尋找名字叫 "腳丫子 (1)" ~ (5) 與 "終點"
            string fpName = (i < 5) ? "腳丫子 (" + (i + 1) + ")" : "終點";
            GameObject fpObj = GameObject.Find(fpName);
            if (fpObj != null) footPrints[i] = fpObj.transform;
        }

        // 自動抓取文字物件 (如果還沒手動拖入)
        if (failHintText == null)
        {
            GameObject hintObj = GameObject.Find("FailHintText");
            if (hintObj != null) failHintText = hintObj.GetComponent<TextMeshProUGUI>();
        }

        // 初始設定
        if (player != null) playerStartPosition = player.transform.position;
        if (failHintText != null) failHintText.gameObject.SetActive(false);
    }

    public void StartWalking()
    {
        if (!isMoving && player != null) StartCoroutine(FollowCommands());
    }

    IEnumerator FollowCommands()
    {
        isMoving = true;
        int stepIndex = 0;

        foreach (Transform slot in slots)
        {
            if (slot != null && slot.childCount > 0 && stepIndex < footPrints.Length)
            {
                GameObject arrow = slot.GetChild(0).gameObject;
                Image arrowImage = arrow.GetComponent<Image>();
                Color originalColor = arrowImage.color;

                // 亮紫色表示正在檢查
                arrowImage.color = new Color(0.4f, 0.2f, 0.6f);

                if (arrow.name.Contains(correctAnswers[stepIndex]))
                {
                    // 答對移動
                    Vector3 targetPos = footPrints[stepIndex].position;
                    targetPos.y += yOffset;

                    float elapsed = 0;
                    Vector3 startPos = player.transform.position;
                    while (elapsed < 0.4f)
                    {
                        player.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / 0.4f);
                        elapsed += Time.deltaTime;
                        yield return null;
                    }
                    player.transform.position = targetPos;
                    arrowImage.color = originalColor;
                    stepIndex++;
                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    // 失敗邏輯
                    arrowImage.color = Color.white;
                    ShowMessage("再試一次吧！", Color.white);
                    yield return new WaitForSeconds(1.5f);
                    failHintText.gameObject.SetActive(false);
                    arrowImage.color = originalColor;
                    ResetPlayer();
                    isMoving = false;
                    yield break;
                }
            }
            else break;
        }

        // 檢查是否全部完成
        if (stepIndex == footPrints.Length)
        {
            ShowMessage("恭喜通關！", Color.white);
            Debug.Log("尋找線索完成！");
        }
        else
        {
            ResetPlayer();
        }
        isMoving = false;
    }

    void ShowMessage(string msg, Color col)
    {
        if (failHintText != null)
        {
            failHintText.text = msg;
            failHintText.color = col;
            failHintText.gameObject.SetActive(true);
        }
    }

    public void ResetPlayer()
    {
        if (player != null) player.transform.position = playerStartPosition;
    }
}