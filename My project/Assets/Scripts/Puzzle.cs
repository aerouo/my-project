using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Puzzle.cs
/// 掛在 PuzzleGameManger 物件上
/// 九個碎片全放對 → 通知 Level2Manager 開始第二段劇情
/// 不計時、不計金幣
/// </summary>
public class Puzzle: MonoBehaviour
{
    [Header("Hint Panel")]
    public GameObject hintPanel;

    [Header("Quit Confirm Panel（backPanel）")]
    public GameObject quitConfirmPanel;
    public Button btnQuitConfirm;
    public Button btnQuitCancel;

    [Header("碎片設定")]
    public int totalPieces = 9;

    // ── 狀態 ─────────────────────────────────────
    private bool gamePaused = false;
    private bool gameStarted = false;
    private int placedCount = 0;

    // ════════════════════════════════════════════════
    void Start()
    {
        quitConfirmPanel?.SetActive(false);
        hintPanel?.SetActive(true);

        btnQuitConfirm?.onClick.AddListener(OnQuitConfirm);
        btnQuitCancel?.onClick.AddListener(OnQuitCancel);
    }

    // ═══════════════ Hint ════════════════════════════

    public void OnClickConfirm()
    {
        hintPanel?.SetActive(false);
        if (!gameStarted)
            gameStarted = true;
        else
            gamePaused = false;
    }

    public void OnClickHint()
    {
        gamePaused = true;
        hintPanel?.SetActive(true);
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
        yield return new WaitForSeconds(0.5f);

        // 通知 Level2Manager 拼圖完成，開始第二段劇情
        FindObjectOfType<Level2Manager>()?.OnPuzzleComplete();
    }
}