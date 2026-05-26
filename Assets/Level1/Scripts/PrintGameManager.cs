using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    }

    public void StartGame()
    {
        ResetTimer();
        StartTimer();

        hasStarted = true;
        hasFinished = false;

        Debug.Log("【第一關】小遊戲開始，計時重置並開始");
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

        // 按下完成後，開始跑檢查動畫，所以這裡先停止計時
        PauseTimer();

        if (executeCoroutine != null)
            StopCoroutine(executeCoroutine);

        executeCoroutine = StartCoroutine(ExecuteBlocks(slots));
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

    IEnumerator ExecuteBlocks(Lvl1DropSlot[] slots)
    {
        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning("【第一關】slots 沒有設定");
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

                yield return StartCoroutine(ShowWrongHint(slots));
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

        float elapsed = GetFinalTime();
        ShowResult(elapsed);
    }

    IEnumerator ShowWrongHint(Lvl1DropSlot[] slots)
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

        // 答錯後回到可操作狀態，繼續計時
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

    void ShowResult(float elapsed)
    {
        if (hasFinished) return;
        hasFinished = true;

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
        {
            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);

            resultText.text =
                $"恭喜通關\n耗時：{minutes:00}:{seconds:00}";
        }

        Debug.Log("【第一關】最終累積遊玩時間：" + elapsed + " 秒");

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveQuestRecord("Level1", elapsed, 100);
            FirestoreManager.Instance.AddCoins(200);
        }
        else
        {
            Debug.LogWarning("【第一關】FirestoreManager.Instance 不存在，無法儲存紀錄");
        }
    }
}