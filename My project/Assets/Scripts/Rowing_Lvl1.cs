using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Rowing_Lvl1 : MonoBehaviour
{
    [Header("物件連結")]
    public SpriteRenderer leftHandRenderer;
    public Sprite leftHandNormal;
    public Sprite leftHandRow;
    public SpriteRenderer rightHandRenderer;
    public Sprite rightHandNormal;
    public Sprite rightHandRow;
    public TMP_Text missText;

    [Header("UI")]
    public TMP_Text txtScore;
    public TMP_Text txtTimer;
    public GameObject resultPanel;
    public TMP_Text txtResult;
    public GameObject backButton;

    [Header("提示畫面")]
    public GameObject hintPanel;
    public TMP_Text txtCountdown;

    [Header("離開確認（backPanel）")]
    public GameObject backPanel;      // 拖入 backPanel
    public UnityEngine.UI.Button btnBackConfirm;  // 確認按鈕
    public UnityEngine.UI.Button btnBackCancel;   // 取消按鈕

    [Header("判定設定")]
    public float hitRange = 5f;
    public float missX = -18.5f;
    public float gameDuration = 180f;

    public int score = 0;
    public bool gameOver = false;
    private float timeLeft;
    public bool gamePaused = false;
    private bool isFirstStart = true;

    private List<GameObject> activeCircles = new List<GameObject>();

    void Start()
    {
        timeLeft = gameDuration;
        resultPanel.SetActive(false);
        hintPanel.SetActive(true);
        backPanel?.SetActive(false);
        gameOver = true;
        txtScore.gameObject.SetActive(false);
        txtTimer.gameObject.SetActive(false);
        UpdateScoreUI();

        btnBackConfirm?.onClick.AddListener(OnBackConfirm);
        btnBackCancel?.onClick.AddListener(OnBackCancel);
    }

    void Update()
    {
        if (gameOver || gamePaused) return;

        timeLeft -= Time.deltaTime;
        UpdateTimerUI();

        if (timeLeft <= 0)
        {
            timeLeft = 0;
            EndGame();
            return;
        }

        activeCircles.RemoveAll(c => c == null);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StartCoroutine(PlayRowAnim(leftHandRenderer, leftHandNormal, leftHandRow));
            HandleInput("Green");
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(PlayRowAnim(rightHandRenderer, rightHandNormal, rightHandRow));
            HandleInput("Pink");
        }

        for (int i = activeCircles.Count - 1; i >= 0; i--)
        {
            if (activeCircles[i] == null) continue;
            if (activeCircles[i].transform.position.x < missX)
            {
                Destroy(activeCircles[i]);
                activeCircles.RemoveAt(i);
                AddScore(-5);
                StartCoroutine(ShowMiss());
            }
        }
    }

    // ── 第一次進場確認 ────────────────────────────
    public void OnClickHintConfirm()
    {
        hintPanel.SetActive(false);
        StartCoroutine(CountdownStart());
    }

    // ── hint 按鈕（中途暫停）────────────────────
    public void OnClickHint()
    {
        gamePaused = true;
        hintPanel.SetActive(true);
        txtScore.gameObject.SetActive(false);
        txtTimer.gameObject.SetActive(false);
    }

    // ── back 按鈕：顯示離開確認，暫停遊戲 ───────
    public void OnClickBack()
    {
        if (gameOver) return;
        gamePaused = true;
        backPanel?.SetActive(true);
    }

    // ── backPanel 確認：真的離開 ─────────────────
    public void OnBackConfirm()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("TrainingRoom");
    }

    // ── backPanel 取消：繼續遊戲 ─────────────────
    public void OnBackCancel()
    {
        gamePaused = false;
        backPanel?.SetActive(false);
    }

    // ── hint 確認（中途繼續）────────────────────
    public void OnClickConfirm()
    {
        gamePaused = false;
        hintPanel.SetActive(false);
        txtScore.gameObject.SetActive(true);
        txtTimer.gameObject.SetActive(true);
    }

    IEnumerator CountdownStart()
    {
        if (txtCountdown != null)
        {
            txtCountdown.gameObject.SetActive(true);
            txtCountdown.text = "3";
            yield return new WaitForSeconds(1f);
            txtCountdown.text = "2";
            yield return new WaitForSeconds(1f);
            txtCountdown.text = "1";
            yield return new WaitForSeconds(1f);
            txtCountdown.gameObject.SetActive(false);
        }
        txtScore.gameObject.SetActive(true);
        txtTimer.gameObject.SetActive(true);
        gamePaused = false;
        gameOver = false;
    }

    void HandleInput(string correctTag)
    {
        GameObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var c in activeCircles)
        {
            if (c == null) continue;
            float dist = Mathf.Abs(c.transform.position.x - transform.position.x);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = c;
            }
        }

        if (nearest == null || minDist > hitRange) return;

        activeCircles.Remove(nearest);

        if (nearest.CompareTag(correctTag))
        {
            Destroy(nearest);
            AddScore(10);
        }
        else
        {
            Destroy(nearest);
            AddScore(-2);
            StartCoroutine(ShowMiss());
        }
    }

    void AddScore(int amount)
    {
        score += amount;
        if (score < 0) score = 0;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (txtScore != null)
            txtScore.text = "分數：" + score;
    }

    void UpdateTimerUI()
    {
        if (txtTimer != null)
        {
            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);
            txtTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void EndGame()
    {
        gameOver = true;

        var spawner = FindFirstObjectByType<CircleSpawner>();
        if (spawner != null) spawner.enabled = false;

        foreach (var c in activeCircles)
            if (c != null) Destroy(c);
        activeCircles.Clear();

        resultPanel.SetActive(true);
        if (txtResult != null)
            txtResult.text = "最終分數：" + score;

        SaveScore();
    }

    void SaveScore()
    {
        PlayerPrefs.SetInt("RowingLvl1_Score", score);
        PlayerPrefs.SetString("RowingLvl1_Date", System.DateTime.Now.ToString("yyyy/MM/dd"));
        PlayerPrefs.Save();
        Debug.Log("分數已暫存：" + score);
    }

    public void RegisterCircle(GameObject circle)
    {
        activeCircles.Add(circle);
    }

    IEnumerator PlayRowAnim(SpriteRenderer hand, Sprite normal, Sprite row)
    {
        if (hand != null)
        {
            hand.sprite = row;
            yield return new WaitForSeconds(0.3f);
            hand.sprite = normal;
        }
    }

    IEnumerator ShowMiss()
    {
        if (missText != null)
        {
            missText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            missText.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other) { }
    void OnTriggerExit2D(Collider2D other) { }
}