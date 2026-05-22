using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MakeBoat_Lvl1 : MonoBehaviour
{
    [Header("Fox")]
    public Image foxImage;
    public Sprite spriteOut1;
    public Sprite spriteOut2;
    public Sprite spriteIn1;
    public Sprite spriteIn2;
    public GameObject foxIdleObject;

    [Header("Fox Crawl Settings")]
    public float crawlDuration = 1f;
    public float frameSwitchInterval = 0.15f;

    [Header("Item Pool")]
    public MakeBoat_ItemData[] itemPool;

    [Header("Choice Buttons")]
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
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI coinRewardText;
    public TextMeshProUGUI finalTimeText;
    public Button btnEndConfirm;

    [Header("Firebase 紀錄設定")]
    public string advancedID = "advanced_04";
    public string difficulty = "easy";

    [Header("結算設定")]
    public int scoreThreshold = 30;
    public int coinPass = 50;
    public int coinRecord = 150;

    private const int totalRounds = 5;
    private const string KEY_BESTTIME = "FoxQuizBestTime";
    private const string KEY_PLAYED = "FoxQuizHasPlayed";

    private int totalScore = 0;
    private int currentRound = 0;
    private bool isAnswered = false;
    private bool gamePaused = false;
    private bool gameStarted = false;

    private MakeBoat_ItemData[] slotItems = new MakeBoat_ItemData[3];

    private float elapsedTime = 0f;
    private bool timerRunning = false;
    private float savedBestTime = 0f;
    private bool hasPlayedBefore = false;

    void Start()
    {
        endScreen?.SetActive(false);
        hintPanel?.SetActive(true);
        quitConfirmPanel?.SetActive(false);

        HideChoices();

        foxImage?.gameObject.SetActive(false);
        foxIdleObject?.SetActive(false);

        if (scoreText) scoreText.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(false);

        btnQuitCancel?.onClick.AddListener(OnQuitCancel);

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

    void UpdateTimerDisplay()
    {
        if (txtTimer == null) return;

        int min = (int)(elapsedTime / 60f);
        int sec = (int)(elapsedTime % 60f);
        txtTimer.text = string.Format("{0:00}:{1:00}", min, sec);
    }

    void LoadData()
    {
        savedBestTime = PlayerPrefs.GetFloat(KEY_BESTTIME, 0f);
        hasPlayedBefore = PlayerPrefs.GetInt(KEY_PLAYED, 0) == 1;
    }

    void SaveData(float newBestTime)
    {
        PlayerPrefs.SetFloat(KEY_BESTTIME, newBestTime);
        PlayerPrefs.SetInt(KEY_PLAYED, 1);
        PlayerPrefs.Save();
    }

    IEnumerator RunGame()
    {
        totalScore = 0;
        currentRound = 0;
        elapsedTime = 0f;
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

        foxIdleObject?.SetActive(false);
    }

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

    public void OnClickHint()
    {
        gamePaused = true;
        hintPanel?.SetActive(true);

        if (scoreText) scoreText.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(false);
    }

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

    public void OnClickBack()
    {
        gamePaused = true;
        quitConfirmPanel?.SetActive(true);
    }

    public void OnQuitCancel()
    {
        gamePaused = false;
        quitConfirmPanel?.SetActive(false);
    }

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

        float newBestTime = (!hasPlayedBefore || (isPass && elapsedTime < savedBestTime))
            ? elapsedTime
            : savedBestTime;

        if (isPass)
        {
            SaveData(newBestTime);

            if (FirestoreManager.Instance != null)
            {
                FirestoreManager.Instance.AddCoins(coinEarned);
                FirestoreManager.Instance.SaveAdvancedRecord(advancedID, difficulty, elapsedTime, coinEarned);
            }
        }

        endScreen?.SetActive(true);

        if (titleText) titleText.text = isPass ? "恭喜通關！" : "未通關";
        if (finalScoreText) finalScoreText.text = "總分：" + totalScore;

        if (coinRewardText)
        {
            string rewardMsg = isPass ? "金幣 +" + coinPass : "未通關";
            if (isNewRecord) rewardMsg += "\n🏆 破紀錄！+" + coinRecord;
            coinRewardText.text = rewardMsg;
        }

        if (finalTimeText)
        {
            int min = (int)(elapsedTime / 60f);
            int sec = (int)(elapsedTime % 60f);
            finalTimeText.text = string.Format("{0:00}:{1:00}", min, sec);
        }
    }

    void SetupChoices()
    {
        List<MakeBoat_ItemData> pool = new List<MakeBoat_ItemData>(itemPool);
        Shuffle(pool);

        for (int i = 0; i < 3; i++)
            slotItems[i] = i < pool.Count ? pool[i] : null;

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

    void ShowChoices()
    {
        foreach (var b in buttons)
            b.gameObject.SetActive(true);
    }

    void HideChoices()
    {
        foreach (var b in buttons)
            b.gameObject.SetActive(false);
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