using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Forestexploration_lvl1 : MonoBehaviour
{
    [Header("Lane Images（左、中、右）")]
    public Image leftImage;
    public Image midImage;
    public Image rightImage;

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

    [Header("GO Buttons（左、中、右）")]
    public Button goLeft;
    public Button goMid;
    public Button goRight;

    [Header("錯誤提示")]
    public Image hintImage;
    public Sprite spriteSnakeHint;
    public Sprite spriteHoleHint;

    [Header("Hearts")]
    public GameObject heart1;
    public GameObject heart2;
    public GameObject heart3;

    [Header("In-Game UI")]
    public TextMeshProUGUI txtTimer;

    [Header("Hint Panel")]
    public GameObject hintPanel;

    [Header("Quit Confirm Panel")]
    public GameObject quitConfirmPanel;
    public Button btnQuitConfirm;
    public Button btnQuitCancel;

    [Header("End Screen（ResultPanel）")]
    public GameObject endScreen;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI coinRewardText;
    public Button btnEndConfirm;

    [Header("Firebase 紀錄設定")]
    public string advancedID = "advanced_03";
    public string difficulty = "easy";

    [Header("結算設定")]
    public int coinPass = 50;
    public int coinRecord = 150;

    private const int totalRounds = 5;
    private const string KEY_BESTTIME = "ForestLvl1_BestTime";
    private const string KEY_PLAYED = "ForestLvl1_HasPlayed";

    private int currentRound = 0;
    private int hearts = 3;
    private float elapsedTime = 0f;
    private bool timerRunning = false;
    private bool gamePaused = false;
    private bool gameStarted = false;
    private bool isAnswered = false;

    private float savedBestTime = 0f;
    private bool hasPlayedBefore = false;

    private int[] laneTypes = new int[3];
    private int lastRoadLane = -1;

    void Start()
    {
        endScreen?.SetActive(false);
        quitConfirmPanel?.SetActive(false);
        hintImage?.gameObject.SetActive(false);
        hintPanel?.SetActive(true);

        if (txtTimer) txtTimer.gameObject.SetActive(false);

        btnQuitCancel?.onClick.AddListener(OnQuitCancel);

        goLeft.onClick.AddListener(() => OnGoClick(0));
        goMid.onClick.AddListener(() => OnGoClick(1));
        goRight.onClick.AddListener(() => OnGoClick(2));

        SetGoButtonsActive(false);
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

    public void OnClickConfirm()
    {
        hintPanel?.SetActive(false);

        if (!gameStarted)
        {
            gameStarted = true;
            timerRunning = true;

            if (txtTimer) txtTimer.gameObject.SetActive(true);

            StartCoroutine(RunGame());
        }
        else
        {
            gamePaused = false;

            if (txtTimer) txtTimer.gameObject.SetActive(true);
        }
    }

    public void OnClickHint()
    {
        gamePaused = true;
        hintPanel?.SetActive(true);

        if (txtTimer) txtTimer.gameObject.SetActive(false);
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

    IEnumerator RunGame()
    {
        for (int i = 0; i < totalRounds; i++)
        {
            if (hearts <= 0) break;

            currentRound = i;
            SetupRound();

            isAnswered = false;
            yield return new WaitUntil(() => isAnswered);
        }

        timerRunning = false;
        ShowEndScreen();
    }

    void SetupRound()
    {
        int roadLane;

        do
        {
            roadLane = Random.Range(0, 3);
        }
        while (roadLane == lastRoadLane);

        lastRoadLane = roadLane;

        for (int i = 0; i < 3; i++)
            laneTypes[i] = (i == roadLane) ? 0 : Random.Range(1, 3);

        SetLaneSprite(leftImage, laneTypes[0], leftRoad, leftSnake, leftHole);
        SetLaneSprite(midImage, laneTypes[1], midRoad, midSnake, midHole);
        SetLaneSprite(rightImage, laneTypes[2], rightRoad, rightSnake, rightHole);

        SetGoButtonsActive(true);
    }

    void SetLaneSprite(Image img, int type, Sprite road, Sprite snake, Sprite hole)
    {
        if (img == null) return;

        img.sprite = type == 0 ? road : type == 1 ? snake : hole;
    }

    void OnGoClick(int laneIndex)
    {
        if (isAnswered || gamePaused) return;

        isAnswered = true;
        SetGoButtonsActive(false);
        StartCoroutine(HandleChoice(laneTypes[laneIndex]));
    }

    IEnumerator HandleChoice(int type)
    {
        if (type == 0)
        {
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            if (hintImage != null)
            {
                hintImage.sprite = type == 1 ? spriteSnakeHint : spriteHoleHint;
                hintImage.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(1f);

            hintImage?.gameObject.SetActive(false);

            hearts--;
            UpdateHearts();

            if (hearts <= 0)
            {
                timerRunning = false;
                ShowEndScreen();
                yield break;
            }
        }
    }

    void UpdateHearts()
    {
        heart1?.SetActive(hearts >= 1);
        heart2?.SetActive(hearts >= 2);
        heart3?.SetActive(hearts >= 3);
    }

    void SetGoButtonsActive(bool active)
    {
        goLeft?.gameObject.SetActive(active);
        goMid?.gameObject.SetActive(active);
        goRight?.gameObject.SetActive(active);
    }

    void ShowEndScreen()
    {
        SetGoButtonsActive(false);

        bool isPass = hearts > 0;
        bool isNewRecord = isPass && hasPlayedBefore && elapsedTime < savedBestTime;

        int coinEarned = isPass ? coinPass : 0;
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
}