using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Puzzle_Lvl1 : MonoBehaviour
{
    [Header("In-Game UI")]
    public TextMeshProUGUI txtTimer;

    [Header("Hint Panel")]
    public GameObject hintPanel;

    [Header("Quit Confirm Panel（backPanel）")]
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
    public string advancedID = "advanced_02";
    public string difficulty = "easy";

    [Header("結算設定")]
    public int coinComplete = 50;
    public int coinRecord = 150;
    public int totalPieces = 9;

    private const string KEY_BESTTIME = "PuzzleLvl1_BestTime";
    private const string KEY_PLAYED = "PuzzleLvl1_HasPlayed";

    private float elapsedTime = 0f;
    private bool timerRunning = false;
    private bool gamePaused = false;
    private bool gameStarted = false;
    private int placedCount = 0;

    private float savedBestTime = 0f;
    private bool hasPlayedBefore = false;

    void Start()
    {
        endScreen?.SetActive(false);
        quitConfirmPanel?.SetActive(false);
        hintPanel?.SetActive(true);

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

    public void OnClickConfirm()
    {
        hintPanel?.SetActive(false);

        if (!gameStarted)
        {
            gameStarted = true;
            timerRunning = true;

            if (txtTimer) txtTimer.gameObject.SetActive(true);
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

    public void OnPiecePlaced()
    {
        placedCount++;

        if (placedCount >= totalPieces)
            StartCoroutine(CompleteGame());
    }

    IEnumerator CompleteGame()
    {
        timerRunning = false;
        yield return new WaitForSeconds(0.5f);

        bool isNewRecord = hasPlayedBefore && elapsedTime < savedBestTime;

        int coinEarned = coinComplete;
        if (isNewRecord) coinEarned += coinRecord;

        float newBestTime = (!hasPlayedBefore || elapsedTime < savedBestTime)
            ? elapsedTime
            : savedBestTime;

        SaveData(newBestTime);

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.AddCoins(coinEarned);
            FirestoreManager.Instance.SaveAdvancedRecord(advancedID, difficulty, elapsedTime, coinEarned);
        }

        endScreen?.SetActive(true);

        if (titleText) titleText.text = "恭喜過關！";

        if (finalTimeText)
        {
            int min = (int)(elapsedTime / 60f);
            int sec = (int)(elapsedTime % 60f);
            finalTimeText.text = string.Format("{0:00}:{1:00}", min, sec);
        }

        if (coinRewardText)
        {
            string msg = "金幣 +" + coinComplete;
            if (isNewRecord) msg += "\n破紀錄！+" + coinRecord;
            coinRewardText.text = msg;
        }
    }
}