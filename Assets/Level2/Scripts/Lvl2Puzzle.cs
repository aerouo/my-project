using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 掛在 PuzzleGameManger 物件上
/// 九個碎片全放對 → 儲存 Level2 50% + 時間 → 通知 Level2Manager 開始第二段劇情
/// </summary>
public class Lvl2Puzzle : MonoBehaviour
{
    [Header("Hint Panel")]
    public GameObject hintPanel;

    [Header("Quit Confirm Panel（backPanel）")]
    public GameObject quitConfirmPanel;
    public Button btnQuitConfirm;
    public Button btnQuitCancel;

    [Header("碎片設定")]
    public int totalPieces = 9;

    private bool gamePaused = false;
    private bool gameStarted = false;
    private bool gameCompleted = false;

    private int placedCount = 0;

    private float totalPlayTime = 0f;
    private float segmentStartTime = 0f;
    private bool isTiming = false;

    void Start()
    {
        quitConfirmPanel?.SetActive(false);
        hintPanel?.SetActive(true);

        btnQuitCancel?.onClick.AddListener(OnQuitCancel);
    }

    public void OnClickConfirm()
    {
        hintPanel?.SetActive(false);

        if (!gameStarted)
        {
            gameStarted = true;
            StartTimer();
        }
        else
        {
            gamePaused = false;
            StartTimer();
        }
    }

    public void OnClickHint()
    {
        gamePaused = true;
        PauseTimer();
        hintPanel?.SetActive(true);
    }

    public void OnClickBack()
    {
        gamePaused = true;
        PauseTimer();
        quitConfirmPanel?.SetActive(true);
    }

    public void OnQuitCancel()
    {
        gamePaused = false;
        quitConfirmPanel?.SetActive(false);
        StartTimer();
    }

    public void OnPiecePlaced()
    {
        if (gameCompleted) return;

        if (!gameStarted)
        {
            gameStarted = true;
            StartTimer();
            Debug.Log("【第二關拼圖】第一次放置碎片，補啟動計時");
        }

        placedCount++;

        if (placedCount >= totalPieces)
            StartCoroutine(CompleteGame());
    }

    void StartTimer()
    {
        if (gameCompleted) return;
        if (isTiming) return;

        segmentStartTime = Time.time;
        isTiming = true;

        Debug.Log("【第二關拼圖】開始計時");
    }

    void PauseTimer()
    {
        if (!isTiming) return;

        float segmentTime = Time.time - segmentStartTime;
        totalPlayTime += segmentTime;
        isTiming = false;

        Debug.Log("【第二關拼圖】暫停計時，本段：" + segmentTime + " 秒，累積：" + totalPlayTime + " 秒");
    }

    float GetFinalTime()
    {
        PauseTimer();

        if (totalPlayTime < 0f)
            totalPlayTime = 0f;

        return Mathf.Round(totalPlayTime * 10f) / 10f;
    }

    IEnumerator CompleteGame()
    {
        if (gameCompleted) yield break;
        gameCompleted = true;

        float elapsedTime = GetFinalTime();

        yield return new WaitForSeconds(0.5f);

        Level2Manager level2Manager = FindObjectOfType<Level2Manager>();

        if (level2Manager != null)
        {
            level2Manager.OnPuzzleComplete(elapsedTime);
        }
        else
        {
            Debug.LogWarning("【第二關拼圖】找不到 Level2Manager");
        }
    }
}