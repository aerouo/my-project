using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MakeBoat Level 2
/// 
/// 玩家先選完所有 6 題，按確認後統一驗證。
/// 狐狸動畫在確認後逐題播放。
///
/// 錯誤懲罰：
///   Q1/Q2 錯 → 提示字，畫面不動，回去重選
///   Q3 錯    → 提示字 + 亮正確選項，回去重選
///   Q4 錯    → 提示字 + 亮正確選項，回去重選
///   Q5 錯    → 提示字 + 亮正確選項，不扣分，回去重選
///   Q6 錯    → 提示字 + 亮正確選項，回去重選
/// </summary>
public class MakeBoat_Lvl2 : MonoBehaviour
{
    // ─────────────────── Fox ───────────────────────────────
    [Header("Fox")]
    public Image foxImage;
    public Sprite spriteOut1, spriteOut2;
    public Sprite spriteIn1, spriteIn2;
    public GameObject foxIdleObject;

    [Header("Fox Crawl Settings")]
    public float crawlDuration = 1f;
    public float frameSwitchInterval = 0.15f;

    // ─────────────────── Questions ────────────────────────
    [System.Serializable]
    public class QuestionData
    {
        public string label;

        [Header("左右按鈕")]
        public Button leftButton;
        public Button rightButton;

        [Header("按鈕底圖 Image")]
        public Image leftImage;
        public Image rightImage;
        public Sprite normalSprite;
        public Sprite glowSprite;

        [Header("按鈕文字")]
        public TextMeshProUGUI leftText;
        public TextMeshProUGUI rightText;

        [Header("題目內容")]
        [TextArea(2, 6)] public string correctCode;
        [TextArea(2, 6)] public string wrongCode;
        public int scoreValue = 10;

        [Header("答錯提示文字")]
        [TextArea(2, 4)] public string wrongHintMessage;

        [Header("答錯是否播懲罰動畫（亮正確選項）")]
        public bool showPenaltyAnim = false;

        // Runtime
        [HideInInspector] public bool correctOnLeft;
        [HideInInspector] public bool? playerChoice;   // true=左, false=右, null=未選
    }

    [Header("6 題資料（依序填入）")]
    public QuestionData[] questions = new QuestionData[6];

    [Header("確認按鈕")]
    public Button confirmButton;

    // ─────────────────── HUD ──────────────────────────────
    [Header("In-Game UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI txtTimer;

    // ─────────────────── Panels ───────────────────────────
    [Header("Hint Panel（開場說明）")]
    public GameObject hintPanel;

    [Header("Java Panel")]
    public GameObject javaPanel;
    public Button btnJavaHint;

    [Header("答錯提示文字（畫面上的 TMP）")]
    public TextMeshProUGUI wrongHintText;
    public Image wrongHintBg;
    public float wrongHintDuration = 2.5f;

    [Header("Quit Confirm Panel")]
    public GameObject quitConfirmPanel;
    public Button btnQuitConfirm;
    public Button btnQuitCancel;

    [Header("End Screen")]
    public GameObject endScreen;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI coinRewardText;
    public TextMeshProUGUI finalTimeText;
    public Button btnEndConfirm;

    // ─────────────────── Constants ────────────────────────
    private const int scoreThreshold = 30;
    private const int coinPass = 50;
    private const int coinRecord = 150;
    private const string KEY_COINS = "TotalCoins";
    private const string KEY_BESTTIME = "FoxQuizLvl2BestTime";
    private const string KEY_PLAYED = "FoxQuizLvl2HasPlayed";

    // ─────────────────── Runtime ──────────────────────────
    private int totalScore = 0;
    private bool isRunning = false;
    private bool gamePaused = false;
    private bool gameStarted = false;
    private float elapsedTime = 0f;
    private bool timerRunning = false;
    private int savedCoins = 0;
    private float savedBestTime = 0f;
    private bool hasPlayedBefore = false;

    // ══════════════════════════════════════════════════════
    void Start()
    {
        endScreen?.SetActive(false);
        hintPanel?.SetActive(true);
        javaPanel?.SetActive(false);
        quitConfirmPanel?.SetActive(false);
        confirmButton?.gameObject.SetActive(false);

        wrongHintText?.gameObject.SetActive(false);
        wrongHintBg?.gameObject.SetActive(false);

        foxImage?.gameObject.SetActive(false);
        foxIdleObject?.SetActive(false);
        if (scoreText) scoreText.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(false);

        btnQuitConfirm?.onClick.AddListener(OnQuitConfirm);
        btnQuitCancel?.onClick.AddListener(OnQuitCancel);
        btnEndConfirm?.onClick.AddListener(OnQuitConfirm);
        btnJavaHint?.onClick.AddListener(OnClickJavaHint);
        confirmButton?.onClick.AddListener(OnClickConfirmAnswer);

        for (int i = 0; i < questions.Length; i++)
        {
            int idx = i;
            questions[i].leftButton?.onClick.AddListener(() => OnSelectLeft(idx));
            questions[i].rightButton?.onClick.AddListener(() => OnSelectRight(idx));
        }

        RandomizeAnswerSides();
        SetAllButtonsInteractable(false);
        LoadData();
    }

    void Update()
    {
        if (timerRunning && !gamePaused)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    // ══════════════════════════════════════════════════════
    #region Randomize

    void RandomizeAnswerSides()
    {
        foreach (var q in questions)
        {
            q.correctOnLeft = (Random.Range(0, 2) == 0);
            q.playerChoice = null;
            if (q.leftText != null) q.leftText.text = q.correctOnLeft ? q.correctCode : q.wrongCode;
            if (q.rightText != null) q.rightText.text = q.correctOnLeft ? q.wrongCode : q.correctCode;
            RefreshVisual(q);
        }
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Select

    void OnSelectLeft(int idx)
    {
        if (isRunning || !gameStarted || gamePaused) return;
        questions[idx].playerChoice = true;
        RefreshVisual(questions[idx]);
    }

    void OnSelectRight(int idx)
    {
        if (isRunning || !gameStarted || gamePaused) return;
        questions[idx].playerChoice = false;
        RefreshVisual(questions[idx]);
    }

    void RefreshVisual(QuestionData q)
    {
        if (q.leftImage != null) q.leftImage.sprite = (q.playerChoice == true) ? q.glowSprite : q.normalSprite;
        if (q.rightImage != null) q.rightImage.sprite = (q.playerChoice == false) ? q.glowSprite : q.normalSprite;
    }

    void SetAllButtonsInteractable(bool v)
    {
        foreach (var q in questions)
        {
            if (q.leftButton) q.leftButton.interactable = v;
            if (q.rightButton) q.rightButton.interactable = v;
        }
        if (confirmButton) confirmButton.interactable = v;
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Confirm & Verify

    public void OnClickConfirmAnswer()
    {
        if (isRunning || !gameStarted) return;

        // 檢查有沒有漏選
        foreach (var q in questions)
        {
            if (q.playerChoice == null)
            {
                StartCoroutine(ShowHint("請選擇所有題目！", 2f));
                return;
            }
        }

        // 鎖住按鈕，開始驗證
        SetAllButtonsInteractable(false);
        javaPanel?.SetActive(false);
        StartCoroutine(RunVerification());
    }

    IEnumerator RunVerification()
    {
        isRunning = true;
        totalScore = 0;

        for (int i = 0; i < questions.Length; i++)
        {
            var q = questions[i];
            bool isCorrect = (q.playerChoice == true) == q.correctOnLeft;

            // 狐狸動畫
            yield return StartCoroutine(FoxCrawlOut());
            yield return StartCoroutine(FoxCrawlIn());
            yield return new WaitForSeconds(0.3f);

            if (isCorrect)
            {
                totalScore += q.scoreValue;
                UpdateHUD();
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                // 顯示提示
                yield return StartCoroutine(ShowHint(q.wrongHintMessage, wrongHintDuration));

                // 懲罰動畫
                if (q.showPenaltyAnim)
                    yield return StartCoroutine(PenaltyAnim(q));

                // 答錯：停止驗證，讓玩家重新作答
                foxIdleObject?.SetActive(false);
                isRunning = false;
                ResetAndUnlock();
                yield break;
            }

            foxIdleObject?.SetActive(false);
        }

        // 全部通過
        isRunning = false;
        timerRunning = false;
        ShowEndScreen();
    }

    void ResetAndUnlock()
    {
        // 保留玩家的選擇，刷新視覺
        foreach (var q in questions) RefreshVisual(q);

        javaPanel?.SetActive(true);
        SetAllButtonsInteractable(true);
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Wrong Hint / Penalty

    IEnumerator ShowHint(string msg, float duration)
    {
        if (wrongHintText == null) yield break;
        wrongHintText.text = msg;
        wrongHintBg?.gameObject.SetActive(true);
        wrongHintText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        wrongHintText.gameObject.SetActive(false);
        wrongHintBg?.gameObject.SetActive(false);
    }

    IEnumerator PenaltyAnim(QuestionData q)
    {
        Image target = q.correctOnLeft ? q.leftImage : q.rightImage;
        if (target == null || q.glowSprite == null) yield break;

        target.sprite = q.glowSprite;
        yield return new WaitForSeconds(0.6f);
        target.sprite = q.normalSprite;
        yield return new WaitForSeconds(0.2f);
        target.sprite = q.glowSprite;
        yield return new WaitForSeconds(0.5f);
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Fox Animation

    IEnumerator FoxCrawlOut()
    {
        foxIdleObject?.SetActive(false);
        foxImage?.gameObject.SetActive(true);
        yield return StartCoroutine(AnimateFlip(foxImage, spriteOut1, spriteOut2, crawlDuration));
    }

    IEnumerator FoxCrawlIn()
    {
        foxImage?.gameObject.SetActive(true);
        yield return StartCoroutine(AnimateFlip(foxImage, spriteIn1, spriteIn2, crawlDuration));
        foxImage?.gameObject.SetActive(false);
        foxIdleObject?.SetActive(true);
    }

    IEnumerator AnimateFlip(Image img, Sprite s1, Sprite s2, float duration)
    {
        if (img == null) yield break;
        float elapsed = 0f, frameTimer = 0f; bool useS1 = true;
        img.sprite = s1;
        while (elapsed < duration)
        {
            if (!gamePaused)
            {
                elapsed += Time.deltaTime; frameTimer += Time.deltaTime;
                if (frameTimer >= frameSwitchInterval)
                { frameTimer = 0f; useS1 = !useS1; img.sprite = useS1 ? s1 : s2; }
            }
            yield return null;
        }
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region HUD / Timer

    void UpdateHUD() { if (scoreText) scoreText.text = "分數：" + totalScore; }

    void UpdateTimerDisplay()
    {
        if (txtTimer == null) return;
        int min = (int)(elapsedTime / 60f), sec = (int)(elapsedTime % 60f);
        txtTimer.text = string.Format("{0:00}:{1:00}", min, sec);
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region UI Buttons

    public void OnClickHint()
    {
        gamePaused = true;
        hintPanel?.SetActive(true);
        javaPanel?.SetActive(false);
        confirmButton?.gameObject.SetActive(false);
        if (scoreText) scoreText.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(false);
    }

    public void OnClickConfirm()
    {
        hintPanel?.SetActive(false);
        if (!gameStarted)
        {
            gameStarted = true;
            timerRunning = true;
            if (scoreText) scoreText.gameObject.SetActive(true);
            if (txtTimer) txtTimer.gameObject.SetActive(true);
            javaPanel?.SetActive(true);
            confirmButton?.gameObject.SetActive(true);
            SetAllButtonsInteractable(true);
        }
        else
        {
            gamePaused = false;
            if (scoreText) scoreText.gameObject.SetActive(true);
            if (txtTimer) txtTimer.gameObject.SetActive(true);
            javaPanel?.SetActive(true);
            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(true);
                confirmButton.interactable = !isRunning;
            }
        }
    }

    public void OnClickJavaHint()
    {
        if (javaPanel == null || gamePaused) return;
        javaPanel.SetActive(!javaPanel.activeSelf);
    }

    public void OnClickBack()
    {
        gamePaused = true;
        quitConfirmPanel?.SetActive(true);
    }

    public void OnQuitConfirm() { UnityEngine.SceneManagement.SceneManager.LoadScene("TrainingRoom"); }

    public void OnQuitCancel()
    {
        gamePaused = false;
        quitConfirmPanel?.SetActive(false);
        if (gameStarted && !isRunning) javaPanel?.SetActive(true);
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region End Screen

    void ShowEndScreen()
    {
        foxImage?.gameObject.SetActive(false);
        foxIdleObject?.SetActive(true);
        javaPanel?.SetActive(false);
        confirmButton?.gameObject.SetActive(false);

        bool isPass = totalScore >= scoreThreshold;
        bool isNewRecord = isPass && hasPlayedBefore && elapsedTime < savedBestTime;
        int coinEarned = (isPass ? coinPass : 0) + (isNewRecord ? coinRecord : 0);
        if (isPass) SaveData(savedCoins + coinEarned,
            (!hasPlayedBefore || elapsedTime < savedBestTime) ? elapsedTime : savedBestTime);

        endScreen?.SetActive(true);
        if (titleText) titleText.text = isPass ? "恭喜通關！" : "未通關";
        if (finalScoreText) finalScoreText.text = "總分：" + totalScore;
        if (coinRewardText) coinRewardText.text = isPass
            ? "金幣 +" + coinPass + (isNewRecord ? "\n🏆 破紀錄！+" + coinRecord : "")
            : "未通關";
        if (finalTimeText)
        {
            int m = (int)(elapsedTime / 60f), s = (int)(elapsedTime % 60f);
            finalTimeText.text = string.Format("{0:00}:{1:00}", m, s);
        }
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Data

    void LoadData()
    {
        savedCoins = PlayerPrefs.GetInt(KEY_COINS, 0);
        savedBestTime = PlayerPrefs.GetFloat(KEY_BESTTIME, 0f);
        hasPlayedBefore = PlayerPrefs.GetInt(KEY_PLAYED, 0) == 1;
    }

    void SaveData(int coins, float best)
    {
        PlayerPrefs.SetInt(KEY_COINS, coins);
        PlayerPrefs.SetFloat(KEY_BESTTIME, best);
        PlayerPrefs.SetInt(KEY_PLAYED, 1);
        PlayerPrefs.Save();
    }

    #endregion
}