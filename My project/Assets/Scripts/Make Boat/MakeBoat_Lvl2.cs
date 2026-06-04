using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 等級2：Java 程式邏輯 6 題選擇題 + 狐狸依序撿道具表現
///
/// 階層結構（每題）：
///   QXbutton
///     ├─ 正解選項物件 (例如 int / i-- / go == log)   → correctButtons[X]
///     └─ 錯誤選項物件 (例如 string / i++ / go=="log") → wrongButtons[X]
///
/// 隨機左右換：每題隨機決定正解選項排在左或右（SiblingIndex 交換）。
///
/// 道具按鈕(Button-1/2/3)：像 Lvl1 一樣洗牌隨機擺放。
///   樹幹/樹枝/石頭 三個有圖的隨機分配到三個按鈕；
///   「全都不要」不佔按鈕（狐狸撿到它時不亮任何按鈕）。
///   道具種類用 MakeBoat_ItemData.itemName 辨識。
///
/// 6 題正解（固定不變）：
///   Q1 宣告變數  : int 版本
///   Q2 迴圈撿五次: for (int i = 5; i >= 1; i--){}
///   Q3 選樹幹    : if (go == log) { score+=10; }
///   Q4 選樹枝    : else if (go == branch) { score+=5; }
///   Q5 選石頭    : else if (go == stone) { score-=5; }
///   Q6 不選      : else { score+=0; }
///
/// === 累積式播放（重點） ===
/// 先找出第一個答錯的題目 N，狐狸從樹幹開始依序撿，
/// 前面答對的題目正常加/扣分，輪到第 N 題對應的道具時動作照做但「分數不變」，
/// 做完該道具就停下跳提示。全對則完整撿 5 次後通關。
///
///   錯 Q1：不撿任何東西，直接跳提示
///   錯 Q2：不撿任何東西，直接跳提示
///   錯 Q3：選樹幹(不加分) → 停 → 提示
///   錯 Q4：樹幹(+10) → 選樹枝(不加分) → 停 → 提示
///   錯 Q5：樹幹(+10) → 樹枝(+5) → 選石頭(不扣分) → 停 → 提示
///   錯 Q6：樹幹(+10) → 樹枝(+5) → 石頭(-5) → 全都不要(不加分) → 停 → 提示
///   全對 ：樹幹(+10) → 樹枝(+5) → 石頭(-5) → 不選(+0) → 不選(+0) → 通關
/// </summary>
public class MakeBoat_Lvl2 : MonoBehaviour
{
    public enum ItemType { Log, Branch, Stone, None }  // 樹幹 / 樹枝 / 石頭 / 全都不要

    [Header("Fox（同一個 Image 物件）")]
    public Image foxImage;
    public Sprite spriteOut1;
    public Sprite spriteOut2;
    public Sprite spriteIn1;
    public Sprite spriteIn2;
    public GameObject foxIdleObject;

    [Header("Fox Crawl Settings")]
    public float crawlDuration = 1f;
    public float frameSwitchInterval = 0.15f;
    public float pickDelay = 0.6f;

    [Header("Item Pool（沿用 Lvl1：樹幹/樹枝/石頭/全都不要）")]
    public MakeBoat_ItemData[] itemPool;

    [Header("道具名稱對應（要和 ItemData.itemName 完全一致）")]
    public string nameLog = "樹幹";
    public string nameBranch = "樹枝";
    public string nameStone = "石頭";
    public string nameNone = "全都不要";

    [Header("Choice Buttons (Button-1, 2, 3) — 畫面上的道具按鈕")]
    public Button[] buttons = new Button[3];
    public Image[] buttonImages = new Image[3];

    [Header("道具按鈕高亮顏色")]
    public Color buttonNormalColor = Color.white;
    public Color buttonGlowColor = Color.yellow;

    [Header("6 題的【正解】選項物件（順序 Q1~Q6）")]
    public Button[] correctButtons = new Button[6];

    [Header("6 題的【錯誤】選項物件（順序 Q1~Q6）")]
    public Button[] wrongButtons = new Button[6];

    [Header("選項選中外觀")]
    public Color optionNormalColor = Color.white;
    public Color optionSelectedColor = new Color(1f, 0.92f, 0.5f, 1f);

    [Header("確認按鈕（OK）")]
    public Button confirmButton;

    [Header("In-Game UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI txtTimer;

    [Header("提示文字（每題失敗顯示不同訊息）")]
    public TextMeshProUGUI failHintText;

    [Header("Hint Panel（進場玩法說明）")]
    public GameObject hintPanel;

    [Header("Java Hint Panel（題目面板，javahint 控制開關）")]
    public GameObject javaHintPanel;

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

    [Header("結算設定")]
    public int coinComplete = 50;
    public int coinRecord = 150;

    // ── 常數 / 存檔 Key ─────────────────────────────
    private const int totalQuestions = 6;
    private const string KEY_COINS = "TotalCoins";
    private const string KEY_BESTTIME = "MakeBoatLvl2_BestTime";
    private const string KEY_PLAYED = "MakeBoatLvl2_HasPlayed";

    // 每種道具的正常得分
    private const int SCORE_LOG = 10;     // 樹幹 +10
    private const int SCORE_BRANCH = 5;   // 樹枝 +5
    private const int SCORE_STONE = -5;   // 石頭 -5
    private const int SCORE_NONE = 0;     // 全都不要 +0

    private static readonly string[] FAIL_HINTS = {
        "變數型別錯誤！",          // Q1
        "迴圈條件錯誤！",          // Q2
        "樹幹判斷錯誤！",          // Q3
        "樹枝判斷錯誤！",          // Q4
        "石頭判斷錯誤！",          // Q5
        "結尾判斷錯誤！"           // Q6
    };

    // ── 執行期狀態 ──────────────────────────────────
    private bool?[] playerChoseCorrect = new bool?[totalQuestions];

    private int totalScore = 0;
    private bool isRunning = false;
    private bool gamePaused = false;
    private bool gameStarted = false;

    private float elapsedTime = 0f;
    private bool timerRunning = false;

    private int savedCoins = 0;
    private float savedBestTime = 0f;
    private bool hasPlayedBefore = false;

    private ItemType[] slotItemType = new ItemType[3];

    // 全都不要的 sprite（撿不選時要顯示）
    private Sprite noneSprite = null;
    // 三個按鈕的原始道具圖（撿全都不要時暫時換圖，之後可還原）
    private Sprite[] originalButtonSprites = new Sprite[3];

    // ─────────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    void Start()
    {
        endScreen?.SetActive(false);
        hintPanel?.SetActive(true);
        javaHintPanel?.SetActive(false);
        quitConfirmPanel?.SetActive(false);

        confirmButton?.gameObject.SetActive(false);
        if (failHintText) failHintText.gameObject.SetActive(false);
        if (scoreText) scoreText.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(false);

        // Lvl2：道具按鈕只是展示用 → 全程顯示但不可點
        ShowChoices();
        SetItemButtonsClickable(false);

        foxImage?.gameObject.SetActive(false);
        foxIdleObject?.SetActive(false);

        btnQuitConfirm?.onClick.AddListener(OnQuitConfirm);
        btnQuitCancel?.onClick.AddListener(OnQuitCancel);
        btnEndConfirm?.onClick.AddListener(OnQuitConfirm);
        confirmButton?.onClick.AddListener(OnClickOK);

        for (int i = 0; i < totalQuestions; i++)
        {
            int idx = i;
            if (correctButtons[i] != null)
            {
                correctButtons[i].onClick.RemoveAllListeners();
                correctButtons[i].onClick.AddListener(() => OnSelectOption(idx, true));
            }
            if (wrongButtons[i] != null)
            {
                wrongButtons[i].onClick.RemoveAllListeners();
                wrongButtons[i].onClick.AddListener(() => OnSelectOption(idx, false));
            }
        }

        LoadData();
        RandomizeOptionSides();
        SetupItemButtons();
    }

    void Update()
    {
        if (timerRunning && !gamePaused)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    void UpdateTimerDisplay()
    {
        if (txtTimer == null) return;
        int min = (int)(elapsedTime / 60f);
        int sec = (int)(elapsedTime % 60f);
        txtTimer.text = string.Format("{0:00}:{1:00}", min, sec);
    }

    // ─────────────────────────────────────────────
    //  DATA
    // ─────────────────────────────────────────────

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

    // ─────────────────────────────────────────────
    //  隨機左右換
    // ─────────────────────────────────────────────

    void RandomizeOptionSides()
    {
        for (int i = 0; i < totalQuestions; i++)
        {
            playerChoseCorrect[i] = null;

            Button correct = correctButtons[i];
            Button wrong = wrongButtons[i];
            if (correct == null || wrong == null) continue;

            ResetOptionVisual(i);

            bool correctOnRight = (Random.Range(0, 2) == 0);
            int cIdx = correct.transform.GetSiblingIndex();
            int wIdx = wrong.transform.GetSiblingIndex();
            bool currentlyCorrectOnRight = cIdx > wIdx;

            if (correctOnRight != currentlyCorrectOnRight)
            {
                correct.transform.SetSiblingIndex(wIdx);
                wrong.transform.SetSiblingIndex(cIdx);
            }
        }
    }

    // ─────────────────────────────────────────────
    //  選項點擊
    // ─────────────────────────────────────────────

    public void OnSelectOption(int questionIdx, bool isCorrectOption)
    {
        if (isRunning || !gameStarted) return;
        playerChoseCorrect[questionIdx] = isCorrectOption;
        ApplySelectedVisual(questionIdx, isCorrectOption);
    }

    void ApplySelectedVisual(int i, bool pickedCorrect)
    {
        SetButtonColor(correctButtons[i], pickedCorrect ? optionSelectedColor : optionNormalColor);
        SetButtonColor(wrongButtons[i], !pickedCorrect ? optionSelectedColor : optionNormalColor);
    }

    void ResetOptionVisual(int i)
    {
        SetButtonColor(correctButtons[i], optionNormalColor);
        SetButtonColor(wrongButtons[i], optionNormalColor);
    }

    void SetButtonColor(Button b, Color c)
    {
        if (b == null) return;
        var img = b.GetComponent<Image>();
        if (img != null) img.color = c;
    }

    // ─────────────────────────────────────────────
    //  道具按鈕設定（像 Lvl1 一樣洗牌隨機擺放）
    // ─────────────────────────────────────────────

    void SetupItemButtons()
    {
        // 取出三個「有圖、非全都不要」的道具，隨機分配到三個按鈕
        // 同時找出「全都不要」的 sprite 備用
        List<MakeBoat_ItemData> displayItems = new List<MakeBoat_ItemData>();
        noneSprite = null;
        foreach (var item in itemPool)
        {
            if (item == null) continue;
            if (ResolveType(item) != ItemType.None)
                displayItems.Add(item);
            else if (noneSprite == null)
                noneSprite = item.sprite;  // 全都不要的圖
        }
        Shuffle(displayItems);

        for (int i = 0; i < 3; i++)
        {
            if (i < displayItems.Count)
            {
                var item = displayItems[i];
                slotItemType[i] = ResolveType(item);
                if (buttonImages[i] != null)
                {
                    buttonImages[i].sprite = item.sprite;
                    buttonImages[i].color = buttonNormalColor;
                    originalButtonSprites[i] = item.sprite;  // 記住原始圖
                }
            }
            else
            {
                slotItemType[i] = ItemType.None;
                if (buttonImages[i] != null)
                    originalButtonSprites[i] = buttonImages[i].sprite;
            }
        }
    }

    /// <summary>用 itemName 辨識道具類型（穩，不依賴分數）。</summary>
    ItemType ResolveType(MakeBoat_ItemData item)
    {
        if (item == null) return ItemType.None;
        if (item.itemName == nameLog) return ItemType.Log;
        if (item.itemName == nameBranch) return ItemType.Branch;
        if (item.itemName == nameStone) return ItemType.Stone;
        return ItemType.None; // 全都不要或未對應
    }

    int FindButtonIndex(ItemType type)
    {
        for (int i = 0; i < 3; i++)
            if (slotItemType[i] == type) return i;
        return -1;
    }

    // ─────────────────────────────────────────────
    //  Hint / Java Hint / Back
    // ─────────────────────────────────────────────

    public void OnClickConfirm()
    {
        hintPanel?.SetActive(false);
        if (!gameStarted)
        {
            gameStarted = true;
            timerRunning = true;
            if (scoreText) scoreText.gameObject.SetActive(true);
            if (txtTimer) txtTimer.gameObject.SetActive(true);
            javaHintPanel?.SetActive(true);
            confirmButton?.gameObject.SetActive(true);
            UpdateHUD();
        }
        else
        {
            gamePaused = false;
            if (scoreText) scoreText.gameObject.SetActive(true);
            if (txtTimer) txtTimer.gameObject.SetActive(true);
            javaHintPanel?.SetActive(true);
            confirmButton?.gameObject.SetActive(true);
        }
    }

    public void OnClickHint()
    {
        gamePaused = true;
        hintPanel?.SetActive(true);
        javaHintPanel?.SetActive(false);
        confirmButton?.gameObject.SetActive(false);
        if (scoreText) scoreText.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(false);
    }

    public void OnClickJavaHint()
    {
        if (javaHintPanel == null) return;
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
    }

    // ─────────────────────────────────────────────
    //  按 OK：開始判斷
    // ─────────────────────────────────────────────

    public void OnClickOK()
    {
        if (isRunning || !gameStarted) return;

        for (int i = 0; i < totalQuestions; i++)
        {
            if (playerChoseCorrect[i] == null)
            {
                ShowHint("請先完成所有題目的選擇！", Color.yellow);
                return;
            }
        }

        StartCoroutine(JudgeFlow());
    }

    // ─────────────────────────────────────────────
    //  判斷流程（累積式播放）
    // ─────────────────────────────────────────────

    IEnumerator JudgeFlow()
    {
        isRunning = true;
        SetAllOptionsInteractable(false);
        if (confirmButton) confirmButton.interactable = false;
        javaHintPanel?.SetActive(false);

        ResetButtonColors();
        totalScore = 0;
        UpdateHUD();

        // 找出第一個答錯的題目（-1 表示全對）
        int firstWrong = -1;
        for (int i = 0; i < totalQuestions; i++)
        {
            if (playerChoseCorrect[i] != true) { firstWrong = i; break; }
        }

        // Q1 或 Q2 錯：不撿任何東西，直接跳提示
        if (firstWrong == 0 || firstWrong == 1)
        {
            ShowHint(FAIL_HINTS[firstWrong], Color.red);
            yield return new WaitForSeconds(2.2f);
            failHintText?.gameObject.SetActive(false);
            ResetAndUnlock();
            isRunning = false;
            yield break;
        }

        // 撿道具順序：樹幹(Q3)→樹枝(Q4)→石頭(Q5)→不選(Q6)→不選
        // 每個道具對應的題目索引（最後一個「不選」沒有對應題目）
        ItemType[] seq = { ItemType.Log, ItemType.Branch, ItemType.Stone, ItemType.None, ItemType.None };
        int[] seqQuestion = { 2, 3, 4, 5, -1 };

        bool stopped = false;

        for (int s = 0; s < seq.Length; s++)
        {
            ItemType type = seq[s];
            int qIdx = seqQuestion[s];

            // 這個道具對應的題目是否答錯（即 firstWrong）
            // 注意：firstWrong == -1 代表全對，不可視為答錯
            bool isWrongHere = (firstWrong != -1) && (qIdx == firstWrong);

            // ① 狐狸站定（顯示站定狐狸），同時亮起這次要撿的道具
            foxImage?.gameObject.SetActive(false);
            foxIdleObject?.SetActive(true);

            int btnIdx;
            if (type == ItemType.None)
            {
                // 全都不要：隨機挑一個按鈕，換成全都不要的圖再亮
                btnIdx = Random.Range(0, 3);
                if (buttonImages[btnIdx] != null)
                {
                    if (noneSprite != null) buttonImages[btnIdx].sprite = noneSprite;
                    buttonImages[btnIdx].color = buttonGlowColor;
                }
            }
            else
            {
                btnIdx = FindButtonIndex(type);
                if (btnIdx >= 0 && buttonImages[btnIdx] != null)
                    buttonImages[btnIdx].color = buttonGlowColor;
            }

            // 加分：答錯的那一題「分數不變」，其餘正常
            if (!isWrongHere)
                totalScore += ScoreOf(type);
            UpdateHUD();

            // 讓玩家看到「站定 + 道具亮」這一刻
            yield return new WaitForSeconds(pickDelay);

            // ② 道具持續亮著，播放往外爬 → 往內爬
            yield return StartCoroutine(FoxCrawlOut());
            yield return StartCoroutine(FoxCrawlIn());

            // 答錯這題 → 撿完這個道具就停下跳提示（道具仍亮著）
            if (isWrongHere)
            {
                ShowHint(FAIL_HINTS[firstWrong], Color.red);
                yield return new WaitForSeconds(2.2f);
                failHintText?.gameObject.SetActive(false);
                stopped = true;
                break;
            }

            // ③ 撿完這個道具 → 熄滅，準備下一個
            yield return new WaitForSeconds(0.2f);
            ResetButtonColors();
            RestoreButtonSprites();  // 若剛才換成全都不要的圖，還原回原本道具圖
        }

        if (stopped)
        {
            ResetAndUnlock();
            isRunning = false;
            yield break;
        }

        // 全對通關
        Debug.Log("[MakeBoat_Lvl2] 全對通關，呼叫 ShowEndScreen，總分=" + totalScore);
        timerRunning = false;
        ShowEndScreen();
        isRunning = false;
    }

    int ScoreOf(ItemType type)
    {
        switch (type)
        {
            case ItemType.Log: return SCORE_LOG;
            case ItemType.Branch: return SCORE_BRANCH;
            case ItemType.Stone: return SCORE_STONE;
            default: return SCORE_NONE;
        }
    }

    // ─────────────────────────────────────────────
    //  狐狸動畫
    // ─────────────────────────────────────────────

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
        float elapsed = 0f;
        bool useS1 = true;
        float frameTimer = 0f;
        img.sprite = s1;

        while (elapsed < duration)
        {
            if (!gamePaused)
            {
                elapsed += Time.deltaTime;
                frameTimer += Time.deltaTime;
                if (frameTimer >= frameSwitchInterval)
                {
                    frameTimer = 0f;
                    useS1 = !useS1;
                    img.sprite = useS1 ? s1 : s2;
                }
            }
            yield return null;
        }
    }

    // ─────────────────────────────────────────────
    //  結算
    // ─────────────────────────────────────────────

    void ShowEndScreen()
    {
        foxImage?.gameObject.SetActive(false);
        foxIdleObject?.SetActive(true);
        ResetButtonColors();
        RestoreButtonSprites();

        bool isNewRecord = hasPlayedBefore && elapsedTime < savedBestTime;
        int coinEarned = coinComplete + (isNewRecord ? coinRecord : 0);
        float newBestTime = (!hasPlayedBefore || elapsedTime < savedBestTime)
                            ? elapsedTime : savedBestTime;

        SaveData(savedCoins + coinEarned, newBestTime);

        if (endScreen == null)
            Debug.LogWarning("[MakeBoat_Lvl2] End Screen 欄位未指派！結算畫面不會顯示。請在 Inspector 拖入結算面板物件。");
        endScreen?.SetActive(true);

        if (titleText) titleText.text = "恭喜通關！";
        if (finalScoreText) finalScoreText.text = "總分：" + totalScore;
        if (coinRewardText)
        {
            string msg = "金幣 +" + coinComplete;
            if (isNewRecord) msg += "\n🏆 破紀錄！+" + coinRecord;
            coinRewardText.text = msg;
        }
        if (finalTimeText)
        {
            int min = (int)(elapsedTime / 60f);
            int sec = (int)(elapsedTime % 60f);
            finalTimeText.text = string.Format("{0:00}:{1:00}", min, sec);
        }
    }

    // ─────────────────────────────────────────────
    //  UTILITY
    // ─────────────────────────────────────────────

    void UpdateHUD()
    {
        if (scoreText) scoreText.text = "分數：" + totalScore;
    }

    void ShowHint(string msg, Color col)
    {
        if (failHintText == null) return;
        failHintText.text = msg;
        failHintText.color = col;
        failHintText.gameObject.SetActive(true);
    }

    void ResetButtonColors()
    {
        for (int i = 0; i < 3; i++)
            if (buttonImages[i] != null)
                buttonImages[i].color = buttonNormalColor;
    }

    /// <summary>把按鈕圖還原為原始道具圖（撿全都不要時曾暫時換圖）。</summary>
    void RestoreButtonSprites()
    {
        for (int i = 0; i < 3; i++)
            if (buttonImages[i] != null && originalButtonSprites[i] != null)
                buttonImages[i].sprite = originalButtonSprites[i];
    }

    void ResetAndUnlock()
    {
        ResetButtonColors();
        RestoreButtonSprites();
        foxImage?.gameObject.SetActive(false);
        foxIdleObject?.SetActive(false);
        if (confirmButton)
        {
            confirmButton.interactable = true;
            confirmButton.gameObject.SetActive(true);
        }
        javaHintPanel?.SetActive(true);
        SetAllOptionsInteractable(true);
    }

    void SetAllOptionsInteractable(bool v)
    {
        for (int i = 0; i < totalQuestions; i++)
        {
            if (correctButtons[i]) correctButtons[i].interactable = v;
            if (wrongButtons[i]) wrongButtons[i].interactable = v;
        }
    }

    void ShowChoices() { foreach (var b in buttons) if (b) b.gameObject.SetActive(true); }
    void HideChoices() { foreach (var b in buttons) if (b) b.gameObject.SetActive(false); }

    void SetItemButtonsClickable(bool v)
    {
        foreach (var b in buttons)
            if (b) b.interactable = v;
    }

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}