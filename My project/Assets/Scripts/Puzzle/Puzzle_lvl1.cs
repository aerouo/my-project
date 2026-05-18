using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Puzzle_Lvl1.cs
/// 掛在 MakeBoatGameManger 物件上
/// 九個碎片全放對 → 停止計時 → 顯示結算畫面
/// 破紀錄用最短時間（需有歷史紀錄才算）
/// </summary>
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
    public TextMeshProUGUI titleText;       // 恭喜完成！
    public TextMeshProUGUI finalTimeText;   // 花費時間
    public TextMeshProUGUI coinRewardText;  // 金幣獎勵
    public Button btnEndConfirm;   // 返回

    [Header("結算設定")]
    public int coinComplete = 50;   // 完成獎勵
    public int coinRecord = 150;  // 破紀錄額外獎勵
    public int totalPieces = 9;    // 碎片總數

    private const string KEY_COINS = "TotalCoins";
    private const string KEY_BESTTIME = "PuzzleLvl1_BestTime";
    private const string KEY_PLAYED = "PuzzleLvl1_HasPlayed";

    // ── 狀態 ─────────────────────────────────────
    private float elapsedTime = 0f;
    private bool timerRunning = false;
    private bool gamePaused = false;
    private bool gameStarted = false;
    private int placedCount = 0;

    private int savedCoins = 0;
    private float savedBestTime = 0f;
    private bool hasPlayedBefore = false;

    // ════════════════════════════════════════════════
    void Start()
    {
        endScreen?.SetActive(false);
        quitConfirmPanel?.SetActive(false);
        hintPanel?.SetActive(true);   // 進場先顯示提示
        if (txtTimer) txtTimer.gameObject.SetActive(false);

        btnQuitConfirm?.onClick.AddListener(OnQuitConfirm);
        btnQuitCancel?.onClick.AddListener(OnQuitCancel);
        btnEndConfirm?.onClick.AddListener(OnQuitConfirm);

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

    // ═══════════════ 資料讀寫 ═══════════════════════

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

    // ═══════════════ Hint ════════════════════════════

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

    // ═══════════════ Back ════════════════════════════

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

    // ═══════════════ 拼圖完成通知 ════════════════════

    /// <summary>由 PuzzleSlot 在放對時呼叫</summary>
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
                            ? elapsedTime : savedBestTime;

        SaveData(savedCoins + coinEarned, newBestTime);

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