using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MakeBoat_Lvl1.cs
///
/// 四個道具（樹幹+10 / 樹枝+5 / 石頭-5 / 全都不要+0）
/// 每輪從四個裡隨機抽三個放進三個按鈕，不重複。
/// 結算：總分>=30 → 金幣+50；破紀錄 → 額外金幣+150
/// 計時：正計時，暫停時停錶
/// Back 按鈕：顯示離開確認對話框（ResultPanel），暫停遊戲
/// </summary>
public class MakeBoat_Lvl1 : MonoBehaviour
{
    // ── 狐狸物件 ────────────────────────────────────
    [Header("Fox Objects")]
    public GameObject foxIn;
    public GameObject foxOut1;
    public GameObject foxOut2;
    public GameObject foxIn1;
    public GameObject foxIn2;

    [Header("Fox Crawl Settings")]
    public float crawlSpeed = 400f;
    public float offscreenX = 700f;
    public float centerX = 0f;
    public float frameSwitchInterval = 0.15f;

    // ── 道具池（四個，含全都不要）──────────────────
    [Header("Item Pool（四個道具：樹幹/樹枝/石頭/全都不要）")]
    public MakeBoat_ItemData[] itemPool;

    // ── 選項按鈕 ────────────────────────────────────
    [Header("Choice Buttons (Button-1, 2, 3)")]
    public Button[] buttons = new Button[3];
    public Image[] buttonImages = new Image[3];

    // ── 遊戲中 UI ───────────────────────────────────
    [Header("In-Game UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI txtTimer;
    public GameObject feedbackPanel;
    public TextMeshProUGUI gainText;

    // ── Hint 面板 ────────────────────────────────────
    [Header("Hint Panel")]
    public GameObject hintPanel;

    // ── 離開確認對話框 ───────────────────────────────
    [Header("Quit Confirm (ResultPanel)")]
    [Tooltip("ResultPanel：顯示『將不儲存遊戲進度，確定離開？』")]
    public GameObject quitConfirmPanel;  // 拖入 ResultPanel
    public Button btnQuitConfirm;    // 拖入 BtnConfirm（確認離開 → 回進入頁面）
    public Button btnQuitCancel;     // 拖入 BtnCancel（取消 → 繼續遊戲）

    // ── 結算畫面 ────────────────────────────────────
    [Header("End Screen")]
    public GameObject endScreen;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI coinRewardText;
    public TextMeshProUGUI breakRecordText;
    public TextMeshProUGUI totalCoinText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI finalTimeText;

    // ── 顏色 ────────────────────────────────────────
    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color positiveColor = new Color(0.3f, 1f, 0.4f);
    public Color negativeColor = new Color(1f, 0.35f, 0.35f);
    public Color neutralColor = new Color(0.9f, 0.9f, 0.9f);
    public Color goldColor = new Color(1f, 0.85f, 0.1f);

    // ── 常數 ────────────────────────────────────────
    private const int totalRounds = 5;
    private const int scoreThreshold = 30;
    private const int coinPass = 50;
    private const int coinRecord = 150;
    private const string KEY_COINS = "TotalCoins";
    private const string KEY_HIGHSCORE = "FoxQuizHighScore";

    // ── 私有狀態 ────────────────────────────────────
    private int totalScore = 0;
    private int currentRound = 0;
    private bool isAnswered = false;
    private bool gamePaused = false;
    private MakeBoat_ItemData[] slotItems = new MakeBoat_ItemData[3];

    private float elapsedTime = 0f;
    private bool timerRunning = false;

    private int savedCoins = 0;
    private int savedHighScore = 0;

    // ════════════════════════════════════════════════
    void Start()
    {
        feedbackPanel?.SetActive(false);
        endScreen?.SetActive(false);
        hintPanel?.SetActive(false);
        quitConfirmPanel?.SetActive(false);
        HideChoices();
        SetAllFoxActive(false);

        // 綁定離開確認按鈕
        btnQuitConfirm?.onClick.AddListener(OnQuitConfirm);
        btnQuitCancel?.onClick.AddListener(OnQuitCancel);

        LoadData();
        StartCoroutine(RunGame());
    }

    void Update()
    {
        if (timerRunning && !gamePaused)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    // ═══════════════ 計時器 ══════════════════════════

    void UpdateTimerDisplay()
    {
        if (txtTimer == null) return;
        int min = (int)(elapsedTime / 60f);
        int sec = (int)(elapsedTime % 60f);
        txtTimer.text = string.Format("{0:00}:{1:00}", min, sec);
    }

    // ═══════════════ 資料讀寫（佔位） ═══════════════

    void LoadData()
    {
        // TODO: Firebase ── 替換成從 FirestoreManager 讀取
        savedCoins = PlayerPrefs.GetInt(KEY_COINS, 0);
        savedHighScore = PlayerPrefs.GetInt(KEY_HIGHSCORE, 0);
    }

    void SaveData(int newCoins, int newHighScore)
    {
        // TODO: Firebase ── 替換成寫入 FirestoreManager
        PlayerPrefs.SetInt(KEY_COINS, newCoins);
        PlayerPrefs.SetInt(KEY_HIGHSCORE, newHighScore);
        PlayerPrefs.Save();
    }

    // ═══════════════ 主流程 ═════════════════════════

    IEnumerator RunGame()
    {
        totalScore = 0; currentRound = 0; elapsedTime = 0f;
        timerRunning = true;
        UpdateHUD();

        for (int i = 0; i < totalRounds; i++)
        {
            currentRound = i;
            UpdateHUD();
            yield return StartCoroutine(RoundFlow());
        }

        timerRunning = false;
        ShowEndScreen();
    }

    IEnumerator RoundFlow()
    {
        HideChoices();
        feedbackPanel?.SetActive(false);

        yield return StartCoroutine(FoxCrawlOut());
        yield return StartCoroutine(FoxCrawlIn());
        yield return new WaitForSeconds(0.3f);

        SetupChoices();

        isAnswered = false;
        yield return new WaitUntil(() => isAnswered);
        yield return new WaitForSeconds(1.4f);

        feedbackPanel?.SetActive(false);
    }

    // ═══════════════ Hint ════════════════════════════

    /// <summary>hint 按鈕：暫停 + 顯示提示面板</summary>
    public void OnClickHint()
    {
        gamePaused = true;
        hintPanel?.SetActive(true);
        if (scoreText) scoreText.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(false);
    }

    /// <summary>Hint 面板的確認（繼續遊戲）</summary>
    public void OnClickConfirm()
    {
        gamePaused = false;
        hintPanel?.SetActive(false);
        if (scoreText) scoreText.gameObject.SetActive(true);
        if (txtTimer) txtTimer.gameObject.SetActive(true);
    }

    // ═══════════════ Back（離開確認）════════════════

    /// <summary>back 按鈕：暫停遊戲，顯示離開確認對話框</summary>
    public void OnClickBack()
    {
        gamePaused = true;
        quitConfirmPanel?.SetActive(true);
    }

    /// <summary>ResultPanel 的「確認」：真的離開，回進入頁面</summary>
    public void OnQuitConfirm()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("TrainingRoom");
    }

    /// <summary>ResultPanel 的「取消」：關掉對話框，繼續遊戲</summary>
    public void OnQuitCancel()
    {
        gamePaused = false;
        quitConfirmPanel?.SetActive(false);
    }

    // ═══════════════ 結算 ════════════════════════════

    void ShowEndScreen()
    {
        HideChoices();
        feedbackPanel?.SetActive(false);
        SetAllFoxActive(false);
        foxIn?.SetActive(true);

        bool isPass = totalScore >= scoreThreshold;
        bool isNewRecord = totalScore > savedHighScore;

        int coinEarned = 0;
        if (isPass) coinEarned += coinPass;
        if (isNewRecord) coinEarned += coinRecord;

        int newTotalCoins = savedCoins + coinEarned;
        int newHighScore = isNewRecord ? totalScore : savedHighScore;

        SaveData(newTotalCoins, newHighScore);
        savedCoins = newTotalCoins;
        savedHighScore = newHighScore;

        endScreen?.SetActive(true);

        if (finalScoreText) finalScoreText.text = "總分：" + totalScore;
        if (highScoreText) highScoreText.text = "最高紀錄：" + newHighScore;

        if (finalTimeText)
        {
            int min = (int)(elapsedTime / 60f);
            int sec = (int)(elapsedTime % 60f);
            finalTimeText.text = string.Format("花費時間：{0:00}:{1:00}", min, sec);
        }

        if (resultText)
        {
            resultText.text = isPass ? "✓ 達標！" : "未達標";
            resultText.color = isPass ? positiveColor : negativeColor;
        }

        if (coinRewardText)
        {
            coinRewardText.gameObject.SetActive(isPass);
            if (isPass) { coinRewardText.text = "金幣 +" + coinPass; coinRewardText.color = goldColor; }
        }

        if (breakRecordText)
        {
            breakRecordText.gameObject.SetActive(isNewRecord);
            if (isNewRecord) { breakRecordText.text = "🏆 破紀錄！金幣 +" + coinRecord; breakRecordText.color = goldColor; }
        }

        if (totalCoinText)
        {
            totalCoinText.text = "目前金幣：" + newTotalCoins;
            totalCoinText.color = goldColor;
        }
    }

    // ═══════════════ 狐狸動畫 ════════════════════════

    IEnumerator FoxCrawlOut()
    {
        SetAllFoxActive(false);
        yield return StartCoroutine(AnimateCrawl(foxOut1, foxOut2, centerX, offscreenX));
    }

    IEnumerator FoxCrawlIn()
    {
        SetAllFoxActive(false);
        yield return StartCoroutine(AnimateCrawl(foxIn1, foxIn2, -offscreenX, centerX));
        SetAllFoxActive(false);
        if (foxIn != null) { foxIn.SetActive(true); SetAnchorX(foxIn, centerX); }
    }

    IEnumerator AnimateCrawl(GameObject f1, GameObject f2, float startX, float endX)
    {
        if (f1 == null || f2 == null) yield break;
        bool useF1 = true;
        float timer = 0f;
        float cur = startX;
        float dir = Mathf.Sign(endX - startX);

        f1.SetActive(true); f2.SetActive(false);
        SetAnchorX(f1, startX); SetAnchorX(f2, startX);

        while (Mathf.Abs(cur - endX) > 1f)
        {
            if (gamePaused) { yield return null; continue; }

            cur += dir * crawlSpeed * Time.deltaTime;
            cur = dir > 0 ? Mathf.Min(cur, endX) : Mathf.Max(cur, endX);
            SetAnchorX(f1, cur); SetAnchorX(f2, cur);

            timer += Time.deltaTime;
            if (timer >= frameSwitchInterval)
            {
                timer = 0f; useF1 = !useF1;
                f1.SetActive(useF1); f2.SetActive(!useF1);
            }
            yield return null;
        }
        f1.SetActive(false); f2.SetActive(false);
    }

    void SetAnchorX(GameObject obj, float x)
    {
        var rt = obj?.GetComponent<RectTransform>();
        if (rt == null) return;
        var p = rt.anchoredPosition; p.x = x; rt.anchoredPosition = p;
    }

    void SetAllFoxActive(bool v)
    {
        foxIn?.SetActive(v); foxOut1?.SetActive(v);
        foxOut2?.SetActive(v); foxIn1?.SetActive(v);
        foxIn2?.SetActive(v);
    }

    // ═══════════════ 選項邏輯 ════════════════════════

    void SetupChoices()
    {
        foreach (var img in buttonImages) if (img) img.color = normalColor;

        List<MakeBoat_ItemData> pool = new List<MakeBoat_ItemData>(itemPool);
        Shuffle(pool);
        for (int i = 0; i < 3; i++)
            slotItems[i] = (i < pool.Count) ? pool[i] : null;

        for (int i = 0; i < 3; i++)
        {
            if (slotItems[i] != null && buttonImages[i] != null)
                buttonImages[i].sprite = slotItems[i].sprite;

            buttons[i].onClick.RemoveAllListeners();
            int cap = i;
            buttons[i].onClick.AddListener(() => OnItemClick(cap));
        }

        ShowChoices();
    }

    void OnItemClick(int index)
    {
        if (isAnswered || gamePaused) return;
        isAnswered = true;

        int gain = slotItems[index]?.scoreValue ?? 0;
        totalScore += gain;
        if (buttonImages[index]) buttonImages[index].color = ScoreColor(gain);
        ShowGainFeedback(gain);
        UpdateHUD();
    }

    // ═══════════════ UI 工具 ════════════════════════

    void ShowGainFeedback(int gain)
    {
        feedbackPanel?.SetActive(true);
        if (gainText == null) return;
        gainText.text = gain > 0 ? "+" + gain : gain.ToString();
        gainText.color = ScoreColor(gain);
    }

    void UpdateHUD()
    {
        if (scoreText) scoreText.text = "分數：" + totalScore;        
    }

    void ShowChoices() { foreach (var b in buttons) b.gameObject.SetActive(true); }
    void HideChoices() { foreach (var b in buttons) b.gameObject.SetActive(false); }

    Color ScoreColor(int v) => v > 0 ? positiveColor : v < 0 ? negativeColor : neutralColor;

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        { int j = Random.Range(0, i + 1); (list[i], list[j]) = (list[j], list[i]); }
    }
}