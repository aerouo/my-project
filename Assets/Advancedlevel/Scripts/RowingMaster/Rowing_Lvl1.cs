using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    public TMP_Text titleText;
    public TMP_Text txtMoney;
    public TMP_Text txtTime;
    public GameObject backButton;

    [Header("提示畫面")]
    public GameObject hintPanel;
    public TMP_Text txtCountdown;

    [Header("離開確認")]
    public GameObject backPanel;
    public UnityEngine.UI.Button btnBackConfirm;
    public UnityEngine.UI.Button btnBackCancel;

    [Header("Firebase 紀錄設定")]
    public string advancedID = "advanced_05";
    public string difficulty = "easy";

    [Header("判定設定")]
    public float hitRange = 5f;
    public float missX = -18.5f;
    public float gameDuration = 60f;

    [Header("結算設定")]
    public int scoreThreshold = 60;
    public int coinPass = 50;
    public int coinRecord = 150;

    private const string KEY_HIGHSCORE = "RowingLvl1_HighScore";
    private const string KEY_PLAYED = "RowingLvl1_HasPlayed";

    public int score = 0;
    public bool gameOver = false;
    private float timeLeft;
    public bool gamePaused = false;

    private int savedHighScore = 0;
    private bool hasPlayedBefore = false;

    private List<GameObject> activeCircles = new List<GameObject>();

    void Start()
    {
        Debug.Log("【划船】目前 gameDuration = " + gameDuration);

        timeLeft = gameDuration;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (hintPanel != null)
            hintPanel.SetActive(true);

        if (backPanel != null)
            backPanel.SetActive(false);

        gameOver = true;

        if (txtScore != null)
            txtScore.gameObject.SetActive(false);

        if (txtTimer != null)
            txtTimer.gameObject.SetActive(false);

        UpdateScoreUI();

        btnBackCancel?.onClick.AddListener(OnBackCancel);

        LoadData();
    }

    void LoadData()
    {
        savedHighScore = PlayerPrefs.GetInt(KEY_HIGHSCORE, 0);
        hasPlayedBefore = PlayerPrefs.GetInt(KEY_PLAYED, 0) == 1;
    }

    void SaveData(int newHighScore)
    {
        PlayerPrefs.SetInt(KEY_HIGHSCORE, newHighScore);
        PlayerPrefs.SetInt(KEY_PLAYED, 1);
        PlayerPrefs.Save();
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

    public void OnClickHintConfirm()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);

        StartCoroutine(CountdownStart());
    }

    public void OnClickHint()
    {
        gamePaused = true;

        if (hintPanel != null)
            hintPanel.SetActive(true);

        if (txtScore != null)
            txtScore.gameObject.SetActive(false);

        if (txtTimer != null)
            txtTimer.gameObject.SetActive(false);
    }

    public void OnClickBack()
    {
        if (gameOver) return;

        gamePaused = true;

        if (backPanel != null)
            backPanel.SetActive(true);
    }

    public void OnBackCancel()
    {
        gamePaused = false;

        if (backPanel != null)
            backPanel.SetActive(false);
    }

    public void OnClickConfirm()
    {
        gamePaused = false;

        if (hintPanel != null)
            hintPanel.SetActive(false);

        if (txtScore != null)
            txtScore.gameObject.SetActive(true);

        if (txtTimer != null)
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

        if (txtScore != null)
            txtScore.gameObject.SetActive(true);

        if (txtTimer != null)
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

        if (nearest == null || minDist > hitRange)
            return;

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

        if (score < 0)
            score = 0;

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

        if (spawner != null)
            spawner.enabled = false;

        foreach (var c in activeCircles)
        {
            if (c != null)
                Destroy(c);
        }

        activeCircles.Clear();

        bool isPass = score >= scoreThreshold;
        bool isNewRecord = isPass && hasPlayedBefore && score > savedHighScore;

        int coinEarned = 0;

        if (isPass)
            coinEarned += coinPass;

        if (isNewRecord)
            coinEarned += coinRecord;

        int newHighScore = isPass && score > savedHighScore
            ? score
            : savedHighScore;

        float elapsedTime = gameDuration - timeLeft;

        if (isPass)
        {
            SaveData(newHighScore);

            if (FirestoreManager.Instance != null)
            {
                FirestoreManager.Instance.AddCoins(coinEarned);

                // 這裡存真正分數，不是金幣
                FirestoreManager.Instance.SaveAdvancedRecord(
                    advancedID,
                    difficulty,
                    elapsedTime,
                    score
                );
            }
        }

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (titleText != null)
            titleText.text = isPass ? "恭喜通關！" : "未通關";

        if (txtResult != null)
            txtResult.text = "最終分數：" + score;

        if (txtMoney != null)
        {
            string rewardMsg;

            if (isPass)
            {
                rewardMsg = "金幣 +" + coinPass;

                if (isNewRecord)
                    rewardMsg += "\n🏆 破紀錄！+" + coinRecord;
            }
            else
            {
                rewardMsg = "金幣 +0";
            }

            txtMoney.text = rewardMsg;
        }

        if (txtTime != null)
        {
            int elapsed = (int)elapsedTime;
            int min = elapsed / 60;
            int sec = elapsed % 60;
            txtTime.text = string.Format("{0:00}:{1:00}", min, sec);
        }
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