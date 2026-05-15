using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MakeBoat_Lvl1 : MonoBehaviour
{
    [Header("Fox（同一個 Image 物件）")]
    public Image foxImage;       // in&out 同一個 Image
    public Sprite spriteOut1;     // out-1
    public Sprite spriteOut2;     // out-2
    public Sprite spriteIn1;      // in-1
    public Sprite spriteIn2;      // in-2
    public GameObject foxIdleObject;  // in（站定 GameObject）

    [Header("Fox Crawl Settings")]
    public float crawlDuration = 1f;    // out/in 各跑幾秒
    public float frameSwitchInterval = 0.15f; // 換幀間隔

    [Header("Item Pool（四個道具：樹幹/樹枝/石頭/全都不要）")]
    public MakeBoat_ItemData[] itemPool;

    [Header("Choice Buttons (Button-1, 2, 3)")]
    public Button[] buttons = new Button[3];
    public Image[] buttonImages = new Image[3];

    [Header("In-Game UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI txtTimer;

    [Header("Hint Panel")]
    public GameObject hintPanel;

    [Header("Quit Confirm Panel")]
    public GameObject quitConfirmPanel;
    public Button btnQuitConfirm;
    public Button btnQuitCancel;

    [Header("End Screen")]
    public GameObject endScreen;
    public TextMeshProUGUI titleText;        // title
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI coinRewardText;
    public TextMeshProUGUI finalTimeText;
    public Button btnEndConfirm;

    private const int totalRounds = 5;
    private const int scoreThreshold = 30;
    private const int coinPass = 50;
    private const int coinRecord = 150;
    private const string KEY_COINS = "TotalCoins";
    private const string KEY_BESTTIME = "FoxQuizBestTime";
    private const string KEY_PLAYED = "FoxQuizHasPlayed";

    private int totalScore = 0;
    private int currentRound = 0;
    private bool isAnswered = false;
    private bool gamePaused = false;
    private MakeBoat_ItemData[] slotItems = new MakeBoat_ItemData[3];

    private float elapsedTime = 0f;
    private bool timerRunning = false;
    private int savedCoins = 0;
    private float savedBestTime = 0f;
    private bool hasPlayedBefore = false;

    void Start()
    {
        endScreen?.SetActive(false);
        hintPanel?.SetActive(true);   // 進場先顯示提示
        quitConfirmPanel?.SetActive(false);
        HideChoices();
        foxImage?.gameObject.SetActive(false);
        foxIdleObject?.SetActive(false);
        if (scoreText) scoreText.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(false);

        btnQuitConfirm?.onClick.AddListener(OnQuitConfirm);
        btnQuitCancel?.onClick.AddListener(OnQuitCancel);
        btnEndConfirm?.onClick.AddListener(OnQuitConfirm);

        LoadData();
        // 不自動開始，等玩家按確認
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

    void LoadData()
    {
        // TODO: Firebase
        savedCoins = PlayerPrefs.GetInt(KEY_COINS, 0);
        savedBestTime = PlayerPrefs.GetFloat(KEY_BESTTIME, 0f);
        hasPlayedBefore = PlayerPrefs.GetInt(KEY_PLAYED, 0) == 1;
    }

    void SaveData(int newCoins, float newBestTime)
    {
        // TODO: Firebase
        PlayerPrefs.SetInt(KEY_COINS, newCoins);
        PlayerPrefs.SetFloat(KEY_BESTTIME, newBestTime);
        PlayerPrefs.SetInt(KEY_PLAYED, 1);
        PlayerPrefs.Save();
    }

    IEnumerator RunGame()
    {
        totalScore = 0; currentRound = 0; elapsedTime = 0f;
        timerRunning = true;
        UpdateHUD();

        for (int i = 0; i < totalRounds; i++)
        {
            currentRound = i;
            yield return StartCoroutine(RoundFlow());
        }

        timerRunning = false;
        ShowEndScreen();
    }

    IEnumerator RoundFlow()
    {
        HideChoices();
        yield return StartCoroutine(FoxCrawlOut());
        yield return StartCoroutine(FoxCrawlIn());
        yield return new WaitForSeconds(0.3f);

        SetupChoices();

        isAnswered = false;
        yield return new WaitUntil(() => isAnswered);
        yield return new WaitForSeconds(1.0f);

        // 隱藏站定的狐狸
        foxIdleObject?.SetActive(false);
    }

    // ═══════════════ 狐狸動畫 ════════════════════════

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

    /// <summary>在固定秒數內交替換兩張 sprite</summary>
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

    // ── Hint ─────────────────────────────────────────
    public void OnClickHint()
    {
        gamePaused = true;
        hintPanel?.SetActive(true);
        if (scoreText) scoreText.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(false);
    }

    private bool gameStarted = false;

    public void OnClickConfirm()
    {
        hintPanel?.SetActive(false);
        if (!gameStarted)
        {
            gameStarted = true;
            if (scoreText) scoreText.gameObject.SetActive(true);
            if (txtTimer) txtTimer.gameObject.SetActive(true);
            StartCoroutine(RunGame());
        }
        else
        {
            gamePaused = false;
            if (scoreText) scoreText.gameObject.SetActive(true);
            if (txtTimer) txtTimer.gameObject.SetActive(true);
        }
    }

    // ── Back ─────────────────────────────────────────
    public void OnClickBack()
    {
        gamePaused = true;
        quitConfirmPanel?.SetActive(true);
    }

    public void OnQuitConfirm()
    {
        PlayerPrefs.SetString("ReturnPanel", "Panel_MakeBoat");
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
        
    }

    public void OnQuitCancel()
    {
        gamePaused = false;
        quitConfirmPanel?.SetActive(false);
    }

    // ── 結算 ─────────────────────────────────────────
    void ShowEndScreen()
    {
        HideChoices();
        foxImage?.gameObject.SetActive(false);
        foxIdleObject?.SetActive(true);

        bool isPass = totalScore >= scoreThreshold;
        bool isNewRecord = isPass && hasPlayedBefore && elapsedTime < savedBestTime;

        int coinEarned = 0;
        if (isPass) coinEarned += coinPass;
        if (isNewRecord) coinEarned += coinRecord;



        endScreen?.SetActive(true);

        if (titleText) titleText.text = isPass ? "恭喜通關！" : "未通關";
        if (finalScoreText) finalScoreText.text = "總分：" + totalScore;
        if (coinRewardText)
        {
            string rewardMsg = "";
            if (isPass)
            {
                rewardMsg += "金幣 +" + coinPass;
                if (isNewRecord) rewardMsg += "\n🏆 破紀錄！+" + coinRecord;
            }
            else
            {
                rewardMsg = "未通關";
            }
            coinRewardText.text = rewardMsg;
        }
        if (finalTimeText)
        {
            int min = (int)(elapsedTime / 60f);
            int sec = (int)(elapsedTime % 60f);
            finalTimeText.text = string.Format("{0:00}:{1:00}", min, sec);
        }
    }

    // ── 選項 ─────────────────────────────────────────
    void SetupChoices()
    {
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
        totalScore += slotItems[index]?.scoreValue ?? 0;
        UpdateHUD();
    }

    void UpdateHUD()
    {
        if (scoreText) scoreText.text = "分數：" + totalScore;
    }

    void ShowChoices() { foreach (var b in buttons) b.gameObject.SetActive(true); }
    void HideChoices() { foreach (var b in buttons) b.gameObject.SetActive(false); }

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        { int j = Random.Range(0, i + 1); (list[i], list[j]) = (list[j], list[i]); }
    }
}