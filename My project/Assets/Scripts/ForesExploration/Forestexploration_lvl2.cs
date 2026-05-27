using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 森林探索 Lv2 — 5 道程式碼選擇題
///
/// 題目（左右隨機）：
///   Q1 - 變數型別  ：int（正確）vs string（錯誤）
///   Q2 - for 迴圈  ：i <= 5; i++（正確）vs i < 5; i--（錯誤）
///   Q3 - 蛇路判斷  ：yourchoice == snakePath（正確）vs != snakePath（錯誤）
///   Q4 - 洞路判斷  ：yourchoice == pitPath（正確）vs != pitPath（錯誤）
///   Q5 - 安全路    ：else { player.moveForward(); }（正確）
///                    vs else { player.moveForward(123); }（錯誤）
///
/// 按 OK 後驗證順序：
///   Step 1：先驗證掉洞（Q4）
///   Step 2：再驗證被蛇咬（Q3）
///   Step 3-5：三次安全通關（Q5）
/// </summary>
public class Forestexploration_lvl2 : MonoBehaviour
{
    [Header("Lane Images（左、中、右）")]
    public Image leftImage;
    public Image midImage;
    public Image rightImage;

    [Header("GO Buttons（左、中、右）")]
    public GameObject goLeft;
    public GameObject goMid;
    public GameObject goRight;

    [Header("左邊三種 Sprite")]
    public Sprite leftRoad;
    public Sprite leftSnake;
    public Sprite leftHole;

    [Header("中間三種 Sprite")]
    public Sprite midRoad;
    public Sprite midSnake;
    public Sprite midHole;

    [Header("右邊三種 Sprite")]
    public Sprite rightRoad;
    public Sprite rightSnake;
    public Sprite rightHole;

    [System.Serializable]
    public class CodeQuestion
    {
        public string label;
        public Button leftButton;
        public Button rightButton;
        public Image leftImage;
        public Image rightImage;
        public Sprite leftNormal;
        public Sprite leftGlow;
        public Sprite rightNormal;
        public Sprite rightGlow;

        [Header("題目文字元件（左、右各一個 TMP）")]
        public TextMeshProUGUI leftText;
        public TextMeshProUGUI rightText;

        [Tooltip("正確答案的程式碼文字（在 Inspector 填入）")]
        [TextArea(2, 6)] public string correctCode;
        [Tooltip("錯誤答案的程式碼文字（在 Inspector 填入）")]
        [TextArea(2, 6)] public string wrongCode;

        [HideInInspector] public bool correctOnLeft;
        [HideInInspector] public bool? playerChoice;
    }

    [Header("5 道程式碼選擇題（Q1~Q5）")]
    public CodeQuestion[] questions = new CodeQuestion[5];

    [Header("確認按鈕")]
    public Button confirmButton;

    [Header("錯誤提示 Image")]
    public Image hintImage;
    public Sprite spriteHoleHint;
    public Sprite spriteSnakeHint;

    [Header("Hearts")]
    public GameObject heart1;
    public GameObject heart2;
    public GameObject heart3;

    [Header("In-Game UI")]
    public TextMeshProUGUI txtTimer;
    public TextMeshProUGUI failHintText;

    [Header("Hint Panel")]
    public GameObject hintPanel;

    [Header("Java Hint Panel")]
    public GameObject javaHintPanel;

    [Header("Quit Confirm Panel")]
    public GameObject quitConfirmPanel;
    public Button btnQuitConfirm;
    public Button btnQuitCancel;

    [Header("End Screen")]
    public GameObject endScreen;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI coinRewardText;
    public Button btnEndConfirm;

    [Header("結算設定")]
    public int coinPass = 50;
    public int coinRecord = 150;

    [Header("驗證進度文字（可選）")]
    public TextMeshProUGUI progressText;

    private const string KEY_COINS = "TotalCoins";
    private const string KEY_BESTTIME = "ForestLvl2_BestTime";
    private const string KEY_PLAYED = "ForestLvl2_HasPlayed";

    private int hearts = 3;
    private float elapsedTime = 0f;
    private bool timerRunning = false;
    private bool gamePaused = false;
    private bool gameStarted = false;
    private bool isRunning = false;
    private int savedCoins = 0;
    private float savedBestTime = 0f;
    private bool hasPlayedBefore = false;
    private int[] laneTypes = new int[3];
    private int lastRoadLane = -1;
    private int currentPhase = 0;

    void Start()
    {
        endScreen?.SetActive(false);
        quitConfirmPanel?.SetActive(false);
        hintImage?.gameObject.SetActive(false);
        failHintText?.gameObject.SetActive(false);
        hintPanel?.SetActive(true);
        javaHintPanel?.SetActive(false);
        confirmButton?.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(false);
        if (progressText) progressText.gameObject.SetActive(false);

        btnQuitConfirm?.onClick.AddListener(OnQuitConfirm);
        btnQuitCancel?.onClick.AddListener(OnQuitCancel);
        btnEndConfirm?.onClick.AddListener(OnQuitConfirm);
        confirmButton?.onClick.AddListener(OnClickConfirmAnswer);

        for (int i = 0; i < questions.Length; i++)
        {
            int idx = i;
            questions[i].leftButton?.onClick.AddListener(() => OnSelectLeft(idx));
            questions[i].rightButton?.onClick.AddListener(() => OnSelectRight(idx));
        }

        RandomizeAnswerSides();
        LoadData();

        // 確保愛心顯示與 hearts 變數同步（防止場景初始狀態不一致）
        UpdateHearts();
    }

    void Update()
    {
        if (!timerRunning || gamePaused) return;
        elapsedTime += Time.deltaTime;
        if (txtTimer != null)
        {
            int min = (int)(elapsedTime / 60f);
            int sec = (int)(elapsedTime % 60f);
            txtTimer.text = string.Format("{0:00}:{1:00}", min, sec);
        }
    }

    void RandomizeAnswerSides()
    {
        for (int i = 0; i < questions.Length; i++)
        {
            var q = questions[i];
            q.correctOnLeft = (Random.Range(0, 2) == 0);
            q.playerChoice = null;

            // 依隨機結果把正確/錯誤程式碼放到對應的文字元件
            if (q.leftText != null)
                q.leftText.text = q.correctOnLeft ? q.correctCode : q.wrongCode;
            if (q.rightText != null)
                q.rightText.text = q.correctOnLeft ? q.wrongCode : q.correctCode;

            RefreshQuestionVisual(i);
        }
    }

    void LoadData()
    {
        savedCoins = PlayerPrefs.GetInt(KEY_COINS, 0);
        savedBestTime = PlayerPrefs.GetFloat(KEY_BESTTIME, 0f);
        hasPlayedBefore = PlayerPrefs.GetInt(KEY_PLAYED, 0) == 1;
    }

    void SaveData(int newCoins, float newBestTime)
    {
        PlayerPrefs.SetInt(KEY_COINS, newCoins);
        PlayerPrefs.SetFloat(KEY_BESTTIME, newBestTime);
        PlayerPrefs.SetInt(KEY_PLAYED, 1);
        PlayerPrefs.Save();
    }

    // ── UI Handlers ────────────────────────────────────────────────

    public void OnClickConfirm()
    {
        hintPanel?.SetActive(false);
        if (!gameStarted)
        {
            gameStarted = true;
            timerRunning = true;
            if (txtTimer) txtTimer.gameObject.SetActive(true);
            javaHintPanel?.SetActive(true);
            confirmButton?.gameObject.SetActive(true);
            if (progressText)
            {
                progressText.gameObject.SetActive(true);
                UpdateProgressText();
            }
            SetupLanes();
        }
        else
        {
            // 從 HintPanel 返回遊戲：恢復計時、顯示遊戲中所有 UI
            gamePaused = false;
            if (txtTimer) txtTimer.gameObject.SetActive(true);

            // 恢復 javaHintPanel
            javaHintPanel?.SetActive(true);

            // confirmButton：一律顯示，但若驗證中則設為不可互動
            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(true);
                confirmButton.interactable = !isRunning;
            }
        }
    }

    public void OnClickHint()
    {
        gamePaused = true;
        hintPanel?.SetActive(true);
        // 暫存並隱藏遊戲中 UI，避免與 HintPanel 疊在一起
        javaHintPanel?.SetActive(false);
        confirmButton?.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(false);
    }

    // JavaHintPanel 是獨立的 toggle，與 HintPanel 無關
    public void OnClickJavaHint()
    {
        if (javaHintPanel == null) return;
        // 只在遊戲進行中（非暫停、非 HintPanel 開啟）才允許 toggle
        if (!gamePaused)
            javaHintPanel.SetActive(!javaHintPanel.activeSelf);
    }

    public void OnClickBack()
    {
        gamePaused = true;
        quitConfirmPanel?.SetActive(true);
    }

    public void OnQuitConfirm()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("TrainingRoom");
    }

    public void OnQuitCancel()
    {
        gamePaused = false;
        quitConfirmPanel?.SetActive(false);

        // 恢復遊戲中 UI（X 按鈕取消後）
        javaHintPanel?.SetActive(true);
        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(true);
            confirmButton.interactable = !isRunning;
        }
    }

    public void OnSelectLeft(int idx)
    {
        if (isRunning || !gameStarted) return;
        questions[idx].playerChoice = true;
        RefreshQuestionVisual(idx);
    }

    public void OnSelectRight(int idx)
    {
        if (isRunning || !gameStarted) return;
        questions[idx].playerChoice = false;
        RefreshQuestionVisual(idx);
    }

    void RefreshQuestionVisual(int idx)
    {
        var q = questions[idx];
        if (q.leftImage != null)
            q.leftImage.sprite = (q.playerChoice == true) ? q.leftGlow : q.leftNormal;
        if (q.rightImage != null)
            q.rightImage.sprite = (q.playerChoice == false) ? q.rightGlow : q.rightNormal;
    }

    // ── Confirm Answer ─────────────────────────────────────────────

    void OnClickConfirmAnswer()
    {
        if (isRunning || !gameStarted) return;

        for (int i = 0; i < questions.Length; i++)
        {
            if (questions[i].playerChoice == null)
            {
                StartCoroutine(ShowHintCoroutine("請選擇所有題目！", Color.yellow, 1.5f));
                return;
            }
        }

        bool[] isCorrect = new bool[questions.Length];
        for (int i = 0; i < questions.Length; i++)
            isCorrect[i] = (questions[i].playerChoice == true) == questions[i].correctOnLeft;

        javaHintPanel?.SetActive(false);
        confirmButton.interactable = false;
        SetAllQuestionsInteractable(false);

        StartCoroutine(RunVerification(isCorrect));
    }

    // ── Main Verification Coroutine ────────────────────────────────
    // isCorrect[0] = Q1 型別宣告
    // isCorrect[1] = Q2 for 迴圈條件
    // isCorrect[2] = Q3 蛇路判斷 (==)
    // isCorrect[3] = Q4 洞路判斷 (==)
    // isCorrect[4] = Q5 安全路 moveForward()
    //
    // 全對時執行順序：
    //   回合1：蛇路（被蛇咬、扣一顆心）
    //   回合2：洞路（掉進洞、扣一顆心）
    //   回合3-5：安全路×3（不扣心）
    //
    // 各題錯誤效果：
    //   Q1錯：無法動，提示型別錯誤
    //   Q2錯（其他全對）：只跑4次（蛇→洞→安全→安全），提示迴圈不完整
    //   Q3錯：走蛇路時行為錯亂，顯示掉洞動畫（不是被蛇）
    //   Q4錯：走洞路時行為錯亂，顯示被蛇動畫（不是掉洞）
    //   Q5錯：安全路一直重複5次停不了

    IEnumerator RunVerification(bool[] isCorrect)
    {
        isRunning = true;

        // 驗證期間隱藏所有 GO 按鈕（自動跑，玩家不需要按）
        goLeft?.SetActive(false);
        goMid?.SetActive(false);
        goRight?.SetActive(false);

        // ── Q1 錯：型別宣告錯，程式無法執行，不動 ───────────────────
        if (!isCorrect[0])
        {
            yield return ShowHintCoroutine("變數型別宣告錯誤，程式無法執行！", Color.red, 2f);
            ResetAndUnlock();
            isRunning = false;
            yield break;
        }

        // ── Q2 錯（其他全對）：只跑4次，少最後一次安全路 ─────────────
        // 決定總回合數：Q2對=5次，Q2錯=4次
        int totalRounds = isCorrect[1] ? 5 : 4;

        // 定義5回合的內容：0=蛇,1=洞,2=安全,3=安全,4=安全
        // laneType：1=蛇場景, 2=洞場景, 0=安全場景
        int[] roundScene = { 1, 2, 0, 0, 0 }; // 各回合場景（蛇/洞/安全）

        bool q2WrongHinted = false; // Q2 錯誤提示只顯示一次（在第4回合後）

        for (int round = 0; round < totalRounds; round++)
        {
            currentPhase = round;
            UpdateProgressText();

            int scene = roundScene[round];
            SetupPhaseScene(scene); // 設定該回合場景
            yield return new WaitForSeconds(0.6f);

            if (scene == 1)
            {
                // ── 蛇路回合 ──────────────────────────────────────────
                if (!isCorrect[2])
                {
                    // Q3錯：走蛇路卻掉進洞（立即扣心，顯示洞的動畫）
                    LoseHeart();
                    if (hearts <= 0) { GameOver(); isRunning = false; yield break; }
                    yield return ShowHintImageCoroutine(spriteHoleHint, 1.5f);
                    yield return ShowHintCoroutine("蛇路判斷錯誤，角色走蛇路卻掉進洞裡了！", Color.red, 2f);
                    ResetAndUnlock();
                    isRunning = false;
                    yield break;
                }
                // Q3對：被蛇咬（立即扣心，顯示蛇的動畫，繼續下一回合）
                LoseHeart();
                if (hearts <= 0) { GameOver(); isRunning = false; yield break; }
                yield return ShowHintImageCoroutine(spriteSnakeHint, 1.5f);
                yield return ShowHintCoroutine("遇到蛇！被蛇咬了一口！", Color.yellow, 1.2f);
            }
            else if (scene == 2)
            {
                // ── 洞路回合 ──────────────────────────────────────────
                if (!isCorrect[3])
                {
                    // Q4錯：走洞路卻進蛇窟（立即扣心，顯示蛇的動畫）
                    LoseHeart();
                    if (hearts <= 0) { GameOver(); isRunning = false; yield break; }
                    yield return ShowHintImageCoroutine(spriteSnakeHint, 1.5f);
                    yield return ShowHintCoroutine("洞路判斷錯誤，角色掉進洞卻走進蛇窟！", Color.red, 2f);
                    ResetAndUnlock();
                    isRunning = false;
                    yield break;
                }
                // Q4對：掉進洞（立即扣心，顯示洞的動畫，繼續下一回合）
                LoseHeart();
                if (hearts <= 0) { GameOver(); isRunning = false; yield break; }
                yield return ShowHintImageCoroutine(spriteHoleHint, 1.5f);
                yield return ShowHintCoroutine("掉進洞裡了！", Color.yellow, 1.2f);
            }
            else
            {
                // ── 安全路回合 ────────────────────────────────────────
                if (!isCorrect[4])
                {
                    // Q5錯：安全路卻一直重複5次停不了（立即扣心）
                    LoseHeart();
                    if (hearts <= 0) { GameOver(); isRunning = false; yield break; }
                    for (int loop = 0; loop < 5; loop++)
                    {
                        SetupLanes();
                        yield return new WaitForSeconds(0.5f);
                        yield return ShowHintCoroutine(
                            string.Format("第 {0}/5 次：卡在同一條路……", loop + 1),
                            Color.yellow, 0.6f);
                    }
                    yield return ShowHintCoroutine("安全路判斷錯誤，永遠無法離開這個森林！", Color.red, 2.5f);
                    ResetAndUnlock();
                    isRunning = false;
                    yield break;
                }
                // Q5對：安全通過
                yield return ShowHintCoroutine(
                    string.Format("安全通過！（{0}/3）", round - 1), Color.green, 0.8f);
            }
        }

        // ── Q2 錯提示（跑完4次後）────────────────────────────────────
        if (!isCorrect[1])
        {
            yield return ShowHintCoroutine("迴圈條件錯誤，路徑不完整！", Color.red, 2f);
            ResetAndUnlock();
            isRunning = false;
            yield break;
        }

        // ── 全部5回合通過 → 結算 ──────────────────────────────────────
        timerRunning = false;
        ShowEndScreen(true);
        isRunning = false;
    }

    // ── Lane Setup ─────────────────────────────────────────────────

    /// <summary>
    /// 安全路回合場景：一條安全路（GO顯示），一條蛇，一條洞。
    /// 安全路位置隨機但不重複上一輪。
    /// </summary>
    void SetupLanes()
    {
        int safeLane;
        do { safeLane = Random.Range(0, 3); }
        while (safeLane == lastRoadLane);
        lastRoadLane = safeLane;

        // 另外兩條：一條蛇(1)、一條洞(2)，隨機分配給剩下兩格
        int[] others = new int[2];
        int idx = 0;
        for (int i = 0; i < 3; i++)
            if (i != safeLane) others[idx++] = i;

        // 隨機決定哪格是蛇、哪格是洞
        bool snakeFirst = (Random.Range(0, 2) == 0);
        laneTypes[safeLane] = 0;
        laneTypes[others[0]] = snakeFirst ? 1 : 2;
        laneTypes[others[1]] = snakeFirst ? 2 : 1;

        ApplyLaneSprites();
    }

    void SetupPhaseScene(int dangerType)
    {
        // 安全路隨機在左(0)或右(2)，中間固定危險，另一側是另一種危險
        // 明確設定全部三格，避免殘留值
        int safeLane = (Random.Range(0, 2) == 0) ? 0 : 2;
        int otherLane = (safeLane == 0) ? 2 : 0;
        int otherType = (dangerType == 1) ? 2 : 1; // 另一種危險

        laneTypes[0] = 0; // 先全清
        laneTypes[1] = 0;
        laneTypes[2] = 0;

        laneTypes[1] = dangerType; // 中間：指定危險
        laneTypes[safeLane] = 0;          // 安全路
        laneTypes[otherLane] = otherType;  // 另一側：另一種危險
        lastRoadLane = safeLane;

        // 確認只有一條安全路
        int safeCount = 0;
        for (int i = 0; i < 3; i++) if (laneTypes[i] == 0) safeCount++;
        if (safeCount != 1)
            UnityEngine.Debug.LogError($"[SetupPhaseScene] 安全路數量錯誤：{safeCount}，應為 1");

        ApplyLaneSprites();
    }

    void ApplyLaneSprites()
    {
        SetLaneSprite(leftImage, laneTypes[0], leftRoad, leftSnake, leftHole);
        SetLaneSprite(midImage, laneTypes[1], midRoad, midSnake, midHole);
        SetLaneSprite(rightImage, laneTypes[2], rightRoad, rightSnake, rightHole);
        UpdateGoButtons();
    }

    /// <summary>只顯示安全路（type=0）的 GO 按鈕，危險路隱藏</summary>
    void UpdateGoButtons()
    {
        goLeft?.SetActive(laneTypes[0] == 0);
        goMid?.SetActive(laneTypes[1] == 0);
        goRight?.SetActive(laneTypes[2] == 0);
    }

    void SetLaneSprite(Image img, int type, Sprite road, Sprite snake, Sprite hole)
    {
        if (img == null) return;
        img.sprite = type == 0 ? road : type == 1 ? snake : hole;
    }

    // ── Hearts / Game Over ─────────────────────────────────────────

    void LoseHeart()
    {
        hearts--;
        UpdateHearts();
    }

    void UpdateHearts()
    {
        heart1?.SetActive(hearts >= 1);
        heart2?.SetActive(hearts >= 2);
        heart3?.SetActive(hearts >= 3);
    }

    void GameOver()
    {
        timerRunning = false;
        ShowEndScreen(false);
    }

    // ── End Screen ─────────────────────────────────────────────────

    void ShowEndScreen(bool isPass)
    {
        bool isNewRecord = isPass && hasPlayedBefore && elapsedTime < savedBestTime;
        int coinEarned = isPass ? coinPass : 0;
        if (isNewRecord) coinEarned += coinRecord;

        float newBestTime = (!hasPlayedBefore || (isPass && elapsedTime < savedBestTime))
                            ? elapsedTime : savedBestTime;

        SaveData(savedCoins + coinEarned, newBestTime);
        endScreen?.SetActive(true);

        if (titleText) titleText.text = isPass ? "恭喜通關！" : "遊戲結束";
        if (finalTimeText)
        {
            int min = (int)(elapsedTime / 60f);
            int sec = (int)(elapsedTime % 60f);
            finalTimeText.text = string.Format("{0:00}:{1:00}", min, sec);
        }
        if (coinRewardText)
        {
            string msg = isPass ? "金幣 +" + coinPass : "金幣 +0";
            if (isNewRecord) msg += "\n破紀錄！+" + coinRecord;
            coinRewardText.text = msg;
        }
    }

    // ── Reset & Unlock ─────────────────────────────────────────────

    /// <summary>
    /// 答錯後恢復可操作狀態：
    /// 保留原本的題目左右位置與玩家已選的選項，只重置 Phase、解鎖按鈕。
    /// 若需要重新隨機題目（例如完全重來），請改呼叫 RandomizeAnswerSides()。
    /// </summary>
    void ResetAndUnlock()
    {
        // 不重新隨機，保留原選項讓玩家修改
        // 只把視覺刷新（保持已選的高亮）
        for (int i = 0; i < questions.Length; i++)
            RefreshQuestionVisual(i);

        // 愛心補回 3 顆
        hearts = 3;
        heart1?.SetActive(true);
        heart2?.SetActive(true);
        heart3?.SetActive(true);

        currentPhase = 0;
        UpdateProgressText();
        SetupLanes();

        // 強制同步愛心顯示（防止 UI 狀態不一致）
        UpdateHearts();

        confirmButton.interactable = true;
        confirmButton?.gameObject.SetActive(true);
        javaHintPanel?.SetActive(true);
        SetAllQuestionsInteractable(true);
    }

    void SetAllQuestionsInteractable(bool v)
    {
        foreach (var q in questions)
        {
            if (q.leftButton) q.leftButton.interactable = v;
            if (q.rightButton) q.rightButton.interactable = v;
        }
    }

    // ── Helpers ────────────────────────────────────────────────────

    void UpdateProgressText()
    {
        if (progressText == null) return;
        string[] names =
        {
            "回合 1/5：蛇路",
            "回合 2/5：洞路",
            "回合 3/5：安全路",
            "回合 4/5：安全路",
            "回合 5/5：安全路"
        };
        if (currentPhase < names.Length)
            progressText.text = names[currentPhase];
    }

    IEnumerator ShowHintCoroutine(string msg, Color col, float duration)
    {
        if (failHintText == null) yield break;
        failHintText.text = msg;
        failHintText.color = col;
        failHintText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        failHintText.gameObject.SetActive(false);
    }

    IEnumerator ShowHintImageCoroutine(Sprite sprite, float duration)
    {
        if (hintImage == null || sprite == null) yield break;
        hintImage.sprite = sprite;
        hintImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        hintImage.gameObject.SetActive(false);
    }
}