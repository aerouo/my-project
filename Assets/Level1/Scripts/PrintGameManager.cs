using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class PrintGameManager : MonoBehaviour
{
    [Header("UI 連結")]
    public GameObject printGroupPanel;
    public GameObject resultPanel;
    public GameObject computerPanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI wrongText;

    [Header("電腦螢幕")]
    public GameObject outputLine;
    public TextMeshProUGUI outputText;
    public string outputWord = "hum-001";
    public float typingSpeed = 0.08f;

    [Header("放置槽（依順序拉入 kuang1~5）")]
    public Lvl1DropSlot[] slots;

    [Header("結果展示方塊")]
    public Image[] blockImages;
    public Color highlightColor = Color.yellow;
    public Color wrongColor = Color.red;
    public float highlightDuration = 0.5f;

    [Header("Block 1 圖片")]
    public Sprite block1_class, block1_main, block1_print, block1_string, block1_num;

    [Header("Block 2 圖片")]
    public Sprite block2_class, block2_main, block2_print, block2_string, block2_num;

    [Header("Block 3 圖片")]
    public Sprite block3_class, block3_main, block3_print, block3_string, block3_num;

    [Header("Block 4 圖片")]
    public Sprite block4_class, block4_main, block4_print, block4_string, block4_num;

    [Header("Block 5 圖片")]
    public Sprite block5_class, block5_main, block5_print, block5_string, block5_num;

    [Header("Quest Record")]
    public string levelID = "Level1";
    public int rewardCoins = 200;
    public string returnSceneName = "SampleScene";
    public string returnPanelName = "Panel_Clue";

    private float totalPlayTime = 0f;
    private float segmentStartTime = 0f;

    private bool isTiming = false;
    private bool hasStarted = false;
    private bool hasFinished = false;

    private Coroutine executeCoroutine;

    void OnEnable()
    {
        hasFinished = false;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (wrongText != null)
            wrongText.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        ResetTimer();
        StartTimer();

        hasStarted = true;
        hasFinished = false;

        Debug.Log("【第一關】小遊戲開始，正式開始計時");
    }

    public void ResetTimer()
    {
        totalPlayTime = 0f;
        segmentStartTime = 0f;
        isTiming = false;
    }

    public void StartTimer()
    {
        if (hasFinished) return;
        if (isTiming) return;

        segmentStartTime = Time.time;
        isTiming = true;

        Debug.Log("【第一關】開始計時：" + segmentStartTime);
    }

    public void PauseTimer()
    {
        if (!isTiming) return;

        float segmentTime = Time.time - segmentStartTime;
        totalPlayTime += segmentTime;
        isTiming = false;

        Debug.Log("【第一關】暫停計時，本段：" + segmentTime + " 秒，累積：" + totalPlayTime + " 秒");
    }

    public float GetCurrentTime()
    {
        float currentTime = totalPlayTime;

        if (isTiming)
            currentTime += Time.time - segmentStartTime;

        if (currentTime < 0f)
            currentTime = 0f;

        return Mathf.Round(currentTime * 10f) / 10f;
    }

    public float GetFinalTime()
    {
        PauseTimer();

        if (totalPlayTime < 0f)
            totalPlayTime = 0f;

        return Mathf.Round(totalPlayTime * 10f) / 10f;
    }

    public void CheckAnswer()
    {
        if (hasFinished) return;

        if (!hasStarted)
        {
            Debug.LogWarning("【第一關】StartGame 沒有被呼叫，改從第一次確認答案開始計時");
            StartGame();
        }

        // 按完成後先暫停，檢查動畫不算時間
        PauseTimer();

        if (executeCoroutine != null)
            StopCoroutine(executeCoroutine);

        executeCoroutine = StartCoroutine(ExecuteBlocks());
    }

    Sprite GetBlockSprite(int blockIndex, string blockID)
    {
        Sprite[] sprites;

        switch (blockIndex)
        {
            case 0: sprites = new Sprite[] { block1_class, block1_main, block1_print, block1_string, block1_num }; break;
            case 1: sprites = new Sprite[] { block2_class, block2_main, block2_print, block2_string, block2_num }; break;
            case 2: sprites = new Sprite[] { block3_class, block3_main, block3_print, block3_string, block3_num }; break;
            case 3: sprites = new Sprite[] { block4_class, block4_main, block4_print, block4_string, block4_num }; break;
            case 4: sprites = new Sprite[] { block5_class, block5_main, block5_print, block5_string, block5_num }; break;
            default: return null;
        }

        switch (blockID)
        {
            case "class": return sprites[0];
            case "main": return sprites[1];
            case "print": return sprites[2];
            case "string": return sprites[3];
            case "num": return sprites[4];
            default: return null;
        }
    }

    IEnumerator ExecuteBlocks()
    {
        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning("【第一關】slots 沒有設定");
            StartTimer();
            yield break;
        }

        if (printGroupPanel != null)
            printGroupPanel.SetActive(false);

        if (computerPanel != null)
            computerPanel.SetActive(true);

        if (outputText != null)
            outputText.text = "";

        if (outputLine != null)
            outputLine.SetActive(false);

        for (int i = 0; i < blockImages.Length && i < slots.Length; i++)
        {
            if (blockImages[i] == null || slots[i] == null) continue;

            blockImages[i].gameObject.SetActive(true);

            string placedID = slots[i].GetCurrentBlockID();
            blockImages[i].sprite = GetBlockSprite(i, placedID);
        }

        for (int i = 0; i < blockImages.Length && i < slots.Length; i++)
        {
            if (blockImages[i] == null || slots[i] == null) continue;

            Color original = blockImages[i].color;
            blockImages[i].color = highlightColor;

            yield return new WaitForSeconds(highlightDuration);

            if (!slots[i].isCorrect)
            {
                blockImages[i].color = wrongColor;
                yield return new WaitForSeconds(0.5f);
                blockImages[i].color = original;

                yield return StartCoroutine(ShowWrongHint());
                yield break;
            }

            blockImages[i].color = original;

            if (slots[i].acceptID == "num")
            {
                if (outputLine != null)
                    outputLine.SetActive(true);

                yield return StartCoroutine(TypeText(outputWord));
            }
        }

        yield return new WaitForSeconds(0.5f);

        // 全部正確，第一關完成，直接顯示 Result Panel
        float finalTime = GetFinalTime();
        ShowResult(finalTime);
    }

    IEnumerator ShowWrongHint()
    {
        foreach (Image img in blockImages)
        {
            if (img != null)
                img.gameObject.SetActive(false);
        }

        foreach (Lvl1DropSlot slot in slots)
        {
            if (slot != null)
                slot.ResetSlot();
        }

        if (computerPanel != null)
            computerPanel.SetActive(false);

        if (printGroupPanel != null)
            printGroupPanel.SetActive(true);

        if (wrongText != null)
        {
            wrongText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            wrongText.gameObject.SetActive(false);
        }

        // 答錯後繼續計時
        StartTimer();
    }

    IEnumerator TypeText(string text)
    {
        if (outputText == null) yield break;

        outputText.text = "";

        foreach (char c in text)
        {
            outputText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void CompleteLevel()
    {
        float elapsed = GetFinalTime();
        ShowResult(elapsed);
    }

    void ShowResult(float elapsed)
    {
        if (hasFinished) return;
        hasFinished = true;

        float finalTotalTime = elapsed;

        Level1Manager level1Manager = FindObjectOfType<Level1Manager>();
        if (level1Manager != null)
            finalTotalTime += level1Manager.GetFirstPartTime();

        finalTotalTime = Mathf.Round(finalTotalTime * 10f) / 10f;

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
        {
            int minutes = Mathf.FloorToInt(finalTotalTime / 60f);
            int seconds = Mathf.FloorToInt(finalTotalTime % 60f);

            resultText.text =
                $"恭喜通關\n耗時：{minutes:00}:{seconds:00}";
        }

        Debug.Log("【第一關】第二段時間：" + elapsed + " 秒");
        Debug.Log("【第一關】第一段 + 第二段總時間：" + finalTotalTime + " 秒");

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveQuestRecord(levelID, finalTotalTime, 100);
            FirestoreManager.Instance.AddCoins(rewardCoins);

            Debug.Log("【第一關】完成關卡，儲存 100% 紀錄，獎勵金幣：" + rewardCoins);
        }
    }



    public void SaveCurrentProgress()
    {
        if (hasFinished) return;

        float currentTime = GetFinalTime();

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveQuestProgress(levelID, 50, currentTime, 0);
            Debug.Log("【第一關】離開前儲存目前闖關進度：50%，時間：" + currentTime + " 秒");
        }
        else
        {
            Debug.LogWarning("【第一關】FirestoreManager.Instance 不存在，無法儲存進度");
        }
    }

    public void SaveCurrentProgressAndQuit()
    {
        SaveCurrentProgress();

        PlayerPrefs.SetString("ReturnPanel", returnPanelName);
        SceneManager.LoadScene(returnSceneName);
    }
}