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
///
/// 各題錯誤效果：
///   Q1錯：無法動，提示型別錯誤
///   Q2錯（其他全對）：跑7次（蛇→洞→安全×5），提示迴圈不完整，陷入無限迴圈
///   Q3錯：走蛇路時行為錯亂，顯示掉洞動畫（不是被蛇）
///   Q4錯：走洞路時行為錯亂，顯示被蛇動畫（不是掉洞）
///   Q5錯：安全路走一次跳出提示圖（走不出去）
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

    [Header("錯誤提示 Image（小圖：蛇咬/掉洞）")]
    public Image hintImage;
    public Sprite spriteHoleHint;
    public Sprite spriteSnakeHint;

    [Header("全螢幕提示 Image（走不出這個森林）")]
    public Image fullscreenHintImage;  // 全螢幕 Image，拉滿畫面
    public Sprite spriteLoopHint;       // Q5錯：走不出這個森林

    [Header("Hearts")]
    public GameObject heart1;
    public GameObject heart2;
    public GameObject heart3;

    [Header("In-Game UI")]
    public TextMeshProUGUI txtTimer;
    public TextMeshProUGUI failHintText;
    [Tooltip("failHintText 的灰底 Image，與文字同時顯示/隱藏")]
    public Image failHintBg;

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
    private bool isVerifying = false; // 驗證中，GO 按鈕全程隱藏

    void Start()
    {
        endScreen?.SetActive(false);
        quitConfirmPanel?.SetActive(false);
        hintImage?.gameObject.SetActive(false);
        fullscreenHintImage?.gameObject.SetActive(false);
        failHintText?.gameObject.SetActive(false);
        failHintBg?.gameObject.SetActive(false);
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
                StartCoroutine(ShowIncompleteHintCoroutine());
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
    // 全對時執行順序（5回合）：
    //   回合1：蛇路（被蛇咬、扣一顆心）
    //   回合2：洞路（掉進洞、扣一顆心）
    //   回合3-5：安全路×3（不扣心）
    //
    // 各題錯誤效果：
    //   Q1錯：無法動，提示型別錯誤
    //   Q2錯（其他全對）：跑7次（蛇→洞→安全×5），提示迴圈不完整，陷入無限迴圈
    //   Q3錯：走蛇路時行為錯亂，顯示掉洞動畫（不是被蛇）
    //   Q4錯：走洞路時行為錯亂，顯示被蛇動畫（不是掉洞）
    //   Q5錯：安全路走一次跳出提示圖（走不出去）

    IEnumerator RunVerification(bool[] isCorrect)
    {
        isRunning = true;
        isVerifying = true;

        // ── Q1 錯：型別宣告錯，程式無法執行，不動 ───────────────────
        if (!isCorrect[0])
        {
            yield return ShowHintCoroutine("變數型別宣告錯誤，程式無法執行！", Color.red, 3.5f);
            ResetAndUnlock();
            isRunning = false;
            yield break;
        }

        // ── Q2 對=5回合，Q2 錯=7回合（蛇→洞→安全×5，模擬無限迴圈）──
        int totalRounds = isCorrect[1] ? 5 : 7;

        // 回合場景定義：1=蛇, 2=洞, 0=安全（陣列夠長覆蓋7回合）
        int[] roundScene = { 1, 2, 0, 0, 0, 0, 0 };

        for (int round = 0; round < totalRounds; round++)
        {
            currentPhase = round;
            UpdateProgressText();

            int scene = roundScene[round];

            // ── Step 1：判斷中（隱藏 GO，顯示三條路的 Sprite）────────
            if (scene == 0)
                SetupLanes();        // 安全回合：隨機三條路
            else
                SetupPhaseScene(scene); // 危險回合：先用 SetupPhaseScene 設好路的 Sprite

            // 強制隱藏所有 GO（判斷中）
            goLeft?.SetActive(false);
            goMid?.SetActive(false);
            goRight?.SetActive(false);
            yield return new WaitForSeconds(0.8f); // 玩家看「判斷中」場景

            // ── Step 2：走進去（顯示 GO 在對應路上）─────────────────
            if (scene == 1)
            {
                // 蛇路：GO 顯示在蛇路
                SetupPhaseScene(scene);
                yield return new WaitForSeconds(0.6f);

                // ── 蛇路回合結果 ──────────────────────────────────────
                if (!isCorrect[2])
                {
                    LoseHeart();
                    if (hearts <= 0) { GameOver(); isRunning = false; yield break; }
                    yield return ShowHintImageCoroutine(spriteHoleHint, 2f);
                    yield return ShowHintCoroutine("蛇路判斷錯誤，角色走蛇路卻掉進洞裡了！", Color.red, 3.5f);
                    ResetAndUnlock();
                    isRunning = false;
                    yield break;
                }
                LoseHeart();
                if (hearts <= 0) { GameOver(); isRunning = false; yield break; }
                yield return ShowHintImageCoroutine(spriteSnakeHint, 2f);
                yield return ShowHintCoroutine("遇到蛇！被蛇咬了一口！", Color.yellow, 2.5f);
            }
            else if (scene == 2)
            {
                // 洞路：GO 顯示在洞路
                SetupPhaseScene(scene);
                yield return new WaitForSeconds(0.6f);

                // ── 洞路回合結果 ──────────────────────────────────────
                if (!isCorrect[3])
                {
                    LoseHeart();
                    if (hearts <= 0) { GameOver(); isRunning = false; yield break; }
                    yield return ShowHintImageCoroutine(spriteSnakeHint, 2f);
                    yield return ShowHintCoroutine("洞路判斷錯誤，角色掉進洞卻走進蛇窟！", Color.red, 3.5f);
                    ResetAndUnlock();
                    isRunning = false;
                    yield break;
                }
                LoseHeart();
                if (hearts <= 0) { GameOver(); isRunning = false; yield break; }
                yield return ShowHintImageCoroutine(spriteHoleHint, 2f);
                yield return ShowHintCoroutine("掉進洞裡了！", Color.yellow, 2.5f);
            }
            else
            {
                // 安全路：手動顯示安全路的 GO
                goLeft?.SetActive(laneTypes[0] == 0);
                goMid?.SetActive(laneTypes[1] == 0);
                goRight?.SetActive(laneTypes[2] == 0);
                yield return new WaitForSeconds(0.6f);

                // ── 安全路回合結果 ────────────────────────────────────
                if (!isCorrect[4])
                {
                    // Q5錯：不扣心，直接跳出「走不出這個森林」提示圖，然後回去重新作答
                    yield return ShowFullscreenHintCoroutine(spriteLoopHint, 2.5f);
                    ResetAndUnlock();
                    isRunning = false;
                    yield break;
                }

                // 安全通過：計算是第幾次安全（round 0=蛇, 1=洞, 2+=安全）
                int safeCount = round - 1; // round=2 → 第1次安全
                int safeTotalNeeded = totalRounds - 2; // Q2對=3次, Q2錯=5次
                yield return ShowHintCoroutine(
                    string.Format("安全通過！（{0}/{1}）", safeCount, safeTotalNeeded),
                    Color.green, 2f);
            }
        }

        // ── Q2 錯提示（跑完7次後）────────────────────────────────────
        if (!isCorrect[1])
        {
            yield return ShowHintCoroutine("迴圈條件錯誤，陷入無限迴圈，路徑走不完！", Color.red, 3.5f);
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

        // 選題階段：GO 全隱；驗證中（安全回合）：只顯示安全路 GO
        if (isVerifying)
        {
            goLeft?.SetActive(laneTypes[0] == 0);
            goMid?.SetActive(laneTypes[1] == 0);
            goRight?.SetActive(laneTypes[2] == 0);
        }
        else
        {
            goLeft?.SetActive(false);
            goMid?.SetActive(false);
            goRight?.SetActive(false);
        }
    }

    /// <summary>
    /// 設定回合場景並回傳 GO 應在哪條 lane（0=左,1=中,2=右）。
    /// 蛇/洞回合：GO 在危險路；安全回合：GO 在安全路。
    /// </summary>
    int SetupPhaseSceneGetLane(int sceneType)
    {
        if (sceneType == 0)
        {
            // 安全回合：用 SetupLanes，GO 在安全路
            SetupLanes();
            // 找安全路的 lane
            for (int i = 0; i < 3; i++)
                if (laneTypes[i] == 0) return i;
            return 0;
        }

        // 危險回合（蛇=1 或 洞=2）：GO 在危險路
        int dangerType = sceneType;
        int dangerLane;
        do { dangerLane = Random.Range(0, 3); }
        while (dangerLane == lastRoadLane);
        lastRoadLane = dangerLane;

        int[] others = new int[2];
        int idx = 0;
        for (int i = 0; i < 3; i++)
            if (i != dangerLane) others[idx++] = i;

        int otherType = (dangerType == 1) ? 2 : 1;
        bool safeFirst = (Random.Range(0, 2) == 0);
        laneTypes[dangerLane] = dangerType;
        laneTypes[others[0]] = safeFirst ? 0 : otherType;
        laneTypes[others[1]] = safeFirst ? otherType : 0;

        ApplyLaneSprites();
        return dangerLane;
    }

    // 保留舊名稱供其他地方呼叫（內部轉呼叫新函式）
    void SetupPhaseScene(int sceneType)
    {
        int lane = SetupPhaseSceneGetLane(sceneType);
        goLeft?.SetActive(lane == 0);
        goMid?.SetActive(lane == 1);
        goRight?.SetActive(lane == 2);
    }

    void ApplyLaneSprites()
    {
        SetLaneSprite(leftImage, laneTypes[0], leftRoad, leftSnake, leftHole);
        SetLaneSprite(midImage, laneTypes[1], midRoad, midSnake, midHole);
        SetLaneSprite(rightImage, laneTypes[2], rightRoad, rightSnake, rightHole);
        // GO 控制由呼叫方負責，這裡不動 GO
    }

    /// <summary>
    /// 驗證中：依 laneTypes 顯示對應 GO；
    /// 非驗證中（選題階段）：全部隱藏。
    /// </summary>
    void UpdateGoButtons()
    {
        if (!isVerifying)
        {
            goLeft?.SetActive(false);
            goMid?.SetActive(false);
            goRight?.SetActive(false);
            return;
        }
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
        isVerifying = false;
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

        // 先解除驗證模式，SetupLanes → UpdateGoButtons 才能正確顯示 GO
        isVerifying = false;
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
            "回合 1：蛇路",
            "回合 2：洞路",
            "回合 3：安全路",
            "回合 4：安全路",
            "回合 5：安全路",
            "回合 6：安全路",
            "回合 7：安全路"
        };
        if (currentPhase < names.Length)
            progressText.text = names[currentPhase];
    }

    IEnumerator ShowIncompleteHintCoroutine()
    {
        javaHintPanel?.SetActive(false);
        yield return ShowHintCoroutine("請選擇所有題目！", Color.yellow, 2.5f);
        javaHintPanel?.SetActive(true);
    }

    IEnumerator ShowHintCoroutine(string msg, Color col, float duration)
    {
        if (failHintText == null) yield break;
        failHintText.text = msg;
        failHintText.color = col;
        failHintBg?.gameObject.SetActive(true);
        failHintText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        failHintText.gameObject.SetActive(false);
        failHintBg?.gameObject.SetActive(false);
    }

    IEnumerator ShowHintImageCoroutine(Sprite sprite, float duration)
    {
        if (hintImage == null || sprite == null) yield break;
        hintImage.sprite = sprite;
        hintImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        hintImage.gameObject.SetActive(false);
    }

    IEnumerator ShowFullscreenHintCoroutine(Sprite sprite, float duration)
    {
        // 不管 sprite 或 Image 有沒有設，都確保等待 duration 秒
        if (fullscreenHintImage != null)
        {
            if (sprite != null) fullscreenHintImage.sprite = sprite;
            fullscreenHintImage.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            fullscreenHintImage.gameObject.SetActive(false);
        }
        else if (hintImage != null)
        {
            // fallback：用小圖 Image 顯示
            if (sprite != null) hintImage.sprite = sprite;
            hintImage.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            hintImage.gameObject.SetActive(false);
        }
        else
        {
            // 完全沒有 Image：至少等待
            yield return new WaitForSeconds(duration);
        }
    }
}