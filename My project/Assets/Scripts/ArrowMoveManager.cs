using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ArrowMoveManager : MonoBehaviour
{
    [Header("角色設定")]
    public GameObject player;

    [Header("自動抓取的物件")]
    public Transform[] slots = new Transform[6];
    public Transform[] footPrints = new Transform[6];
    public TextMeshProUGUI failHintText;

    [Header("通關劇情演出設定")]
    public GameObject endingBackground;
    public TextMeshProUGUI endingPlotText;
    public GameObject continueButton;  // 妳新增的 ContinueButton

    [Header("下一關設定")]
    public GameObject nextGameUI;      // 拖入「遊戲-print」物件

    [Header("位置微調")]
    public float yOffset = 70f;

    [Header("正確答案設定")]
    public string[] correctAnswers = { "箭頭 (右)", "箭頭 (右)", "箭頭 (右)", "箭頭 (上)", "箭頭 (右)", "箭頭 (右)" };

    private Vector3 playerStartPosition;
    private bool isMoving = false;

    void OnEnable()
    {
        // --- 自動尋找物件邏輯 ---
        for (int i = 0; i < 6; i++)
        {
            GameObject slotObj = GameObject.Find("虛線方塊 (" + (i + 1) + ")");
            if (slotObj != null) slots[i] = slotObj.transform;

            string fpName = (i < 5) ? "腳丫子 (" + (i + 1) + ")" : "終點";
            GameObject fpObj = GameObject.Find(fpName);
            if (fpObj != null) footPrints[i] = fpObj.transform;
        }

        if (failHintText == null)
        {
            GameObject hintObj = GameObject.Find("FailHintText");
            if (hintObj != null) failHintText = hintObj.GetComponent<TextMeshProUGUI>();
        }

        // 初始隱藏所有通關後的 UI
        if (player != null) playerStartPosition = player.transform.position;
        if (failHintText != null) failHintText.gameObject.SetActive(false);
        if (endingBackground != null) endingBackground.SetActive(false);
        if (endingPlotText != null) endingPlotText.gameObject.SetActive(false);
        if (continueButton != null) continueButton.SetActive(false);
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
                arrowImage.color = new Color(0.4f, 0.2f, 0.6f);

                if (arrow.name.Contains(correctAnswers[stepIndex]))
                {
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
                    arrowImage.color = Color.red;
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

        if (stepIndex == footPrints.Length)
        {
            // 通關演出：閃爍後換幕
            for (int i = 0; i < 4; i++)
            {
                ShowMessage("恭喜通關！", Color.white);
                yield return new WaitForSeconds(0.25f);
                failHintText.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.25f);
            }
            ShowEndingPlot();
        }
        else
        {
            ResetPlayer();
        }
        isMoving = false;
    }

    void ShowEndingPlot()
    {
        if (endingBackground != null) endingBackground.SetActive(true);
        if (endingPlotText != null)
        {
            endingPlotText.text = "你晃了晃仍然發麻的雙腿，走向靠牆的一台舊型電腦，\n螢幕上的綠色指示燈閃著微弱光芒，似乎仍保存著最後一絲電力。";
            endingPlotText.gameObject.SetActive(true);
        }

        // 顯示繼續按鈕，讓玩家可以手動點擊
        if (continueButton != null) continueButton.SetActive(true);
    }

    // 💡 點擊「繼續」按鈕時會執行的函數
    public void OnClickContinue()
    {
        // 1. 打開下一個遊戲介面「遊戲-print」
        if (nextGameUI != null) nextGameUI.SetActive(true);

        // 2. 關閉目前的「小遊戲-尋找線索」整體
        this.gameObject.SetActive(false);
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