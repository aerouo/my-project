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
    public GameObject continueButton;

    [Header("下一關設定")]
    public GameObject nextGameUI;

    [Header("闖關紀錄")]
    public string levelID = "Level1";
    public int firstPartProgress = 50;
    public PrintGameManager printGameManager;

    [Header("位置微調")]
    public float yOffset = 70f;

    [Header("正確答案設定")]
    public string[] correctAnswers =
    {
        "箭頭 (右)", "箭頭 (右)", "箭頭 (右)",
        "箭頭 (上)", "箭頭 (右)", "箭頭 (右)"
    };

    private Vector3 playerStartPosition;
    private bool isMoving = false;
    private bool firstPartSaved = false;

    void OnEnable()
    {
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
            if (hintObj != null)
                failHintText = hintObj.GetComponent<TextMeshProUGUI>();
        }

        if (printGameManager == null)
            printGameManager = FindObjectOfType<PrintGameManager>();

        if (player != null)
            playerStartPosition = player.transform.position;

        if (failHintText != null) failHintText.gameObject.SetActive(false);
        if (endingBackground != null) endingBackground.SetActive(false);
        if (endingPlotText != null) endingPlotText.gameObject.SetActive(false);
        if (continueButton != null) continueButton.SetActive(false);
    }

    public void StartWalking()
    {
        if (!isMoving && player != null)
            StartCoroutine(FollowCommands());
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

                    float elapsed = 0f;
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

                    if (failHintText != null)
                        failHintText.gameObject.SetActive(false);

                    arrowImage.color = originalColor;

                    ResetPlayer();

                    isMoving = false;

                    // 錯誤不存 50%，回去繼續計時
                    if (printGameManager != null)
                        printGameManager.StartTimer();

                    yield break;
                }
            }
            else
            {
                break;
            }
        }

        if (stepIndex == footPrints.Length)
        {
            SaveFirstPartProgress();

            for (int i = 0; i < 4; i++)
            {
                ShowMessage("恭喜通關！", Color.white);
                yield return new WaitForSeconds(0.25f);

                if (failHintText != null)
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

    private void SaveFirstPartProgress()
    {
        if (firstPartSaved)
            return;

        firstPartSaved = true;

        float currentTime = 0f;

        if (printGameManager != null)
            currentTime = printGameManager.GetFinalTime();

        Level1Manager level1Manager = FindObjectOfType<Level1Manager>();
        if (level1Manager != null)
            level1Manager.SetFirstPartTime(currentTime);

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveQuestProgress(levelID, firstPartProgress, currentTime, 0);
            Debug.Log("【第一關】第一段完成，停止計時並儲存 50%，時間：" + currentTime + " 秒");
        }
        else
        {
            Debug.LogWarning("【第一關】FirestoreManager.Instance 不存在，無法儲存 50%");
        }
    }
    void ShowEndingPlot()
    {
        if (endingBackground != null)
            endingBackground.SetActive(true);

        if (endingPlotText != null)
        {
            endingPlotText.text =
                "你晃了晃仍然發麻的雙腿，走向靠牆的一台舊型電腦，\n" +
                "螢幕上的綠色指示燈閃著微弱光芒，似乎仍保存著最後一絲電力。";

            endingPlotText.gameObject.SetActive(true);
        }

        if (continueButton != null)
            continueButton.SetActive(true);
    }

    public void OnClickContinue()
    {
        if (printGameManager != null)
        {
            printGameManager.StartTimer();
            Debug.Log("【第一關】第二段開始，繼續計時");
        }

        if (nextGameUI != null)
            nextGameUI.SetActive(true);

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
        if (player != null)
            player.transform.position = playerStartPosition;
    }
}