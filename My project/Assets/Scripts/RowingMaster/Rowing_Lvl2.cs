using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 等級2：Java 程式邏輯 4 題選擇題 + 划船依答案組合表現
///
/// 階層結構（每題）：
///   QXbutton
///     ├─ 正解選項物件   → correctButtons[X]
///     └─ 錯誤選項物件   → wrongButtons[X]
///
/// 隨機左右換：每題隨機決定正解選項排在左或右（SiblingIndex 交換）。
///
/// 4 題正解（固定不變）：
///   Q1 宣告變數 : int  版本   (int green=1; int pink=2; int circle;)
///   Q2 划十次   : for (int i = 10; i >= 1; i--){}
///   Q3 綠色判斷 : if (circle == green) { ...划左邊 }
///   Q4 粉色判斷 : else { ...划右邊 }
///
/// === 判定邏輯（依需求）===
///   Q1 或 Q2 錯 → 不划船，直接跳提示
///   Q1 & Q2 對 → 依 Q3 / Q4 決定划船：
///     只有 Q3 對 → 生 5 綠圈(左手)               → 停 → 提示
///     只有 Q4 對 → 生 5 粉圈(右手)               → 停 → 提示
///     Q3 & Q4 都對 → 5 綠 + 5 粉 + 5 隨機(綠/粉)  → 通關
///
///   左手 = 綠色圈圈；右手 = 粉色圈圈
///
/// === 圈圈表現（沿用 Lvl1 連續流動）===
///   依序連續生圈（每隔 spawnInterval 生一個），每個圈自己往左飄，
///   畫面上同時會有好幾個圈依序排隊飄過來（像 Lvl1）。
///   手是「自動」的：盯住最前面的圈，當它飄到判定點(judgeX)，
///   對應的手自動做划船動作把它劃掉、加分。玩家不用按鍵，看動畫即可。
///   綠圈用左手、粉圈用右手；隨機段隨機生綠或粉，該色用該手。
///   圈圈 prefab 沿用 Lvl1 的 greenCirclePrefab / pinkCirclePrefab。
/// </summary>
public class Rowing_Lvl2 : MonoBehaviour
{
    public enum RowSide { Left, Right }  // Left=綠色(左手) / Right=粉色(右手)

    [Header("雙手（同 Lvl1 用法）")]
    public SpriteRenderer leftHandRenderer;
    public Sprite leftHandNormal;
    public Sprite leftHandRow;
    public SpriteRenderer rightHandRenderer;
    public Sprite rightHandNormal;
    public Sprite rightHandRow;

    [Header("划船動作設定")]
    public float rowAnimDuration = 0.3f;  // 一次划船手部停留時間（劃一下）

    [Header("圈圈生成（沿用 Lvl1 prefab）")]
    public GameObject greenCirclePrefab;   // 綠圈 → 左手
    public GameObject pinkCirclePrefab;    // 粉圈 → 右手
    public Transform spawnPoint;           // 圈圈生成位置（畫面右側）
    public float circleSpeed = 5f;         // 圈圈往左飄的速度（同 Lvl1）
    public float spawnInterval = 0.8f;     // 每隔多久生一個圈（同 Lvl1 的節奏）
    public float judgeX = -15f;            // 判定點 x（圈飄到這裡就被手自動劃掉，對準左邊判定點）

    [Header("4 題的【正解】選項物件（順序 Q1~Q4）")]
    public Button[] correctButtons = new Button[4];

    [Header("4 題的【錯誤】選項物件（順序 Q1~Q4）")]
    public Button[] wrongButtons = new Button[4];

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

    [Header("划船得分（每次）")]
    public int scorePerRow = 10;

    // ── 常數 / 存檔 Key ─────────────────────────────
    private const int totalQuestions = 4;
    private const string KEY_COINS = "TotalCoins";
    private const string KEY_BESTTIME = "RowingLvl2_BestTime";
    private const string KEY_PLAYED = "RowingLvl2_HasPlayed";

    private const int ROW_TIMES = 5;  // 每段划幾次

    private static readonly string[] FAIL_HINTS = {
        "變數型別錯誤！",          // Q1
        "迴圈條件錯誤！",          // Q2
        "綠色判斷錯誤！",          // Q3
        "粉色判斷錯誤！"           // Q4
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

        // 雙手歸位
        if (leftHandRenderer && leftHandNormal) leftHandRenderer.sprite = leftHandNormal;
        if (rightHandRenderer && rightHandNormal) rightHandRenderer.sprite = rightHandNormal;

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
    //  判斷流程
    // ─────────────────────────────────────────────

    IEnumerator JudgeFlow()
    {
        isRunning = true;
        SetAllOptionsInteractable(false);
        if (confirmButton) confirmButton.interactable = false;
        javaHintPanel?.SetActive(false);

        totalScore = 0;
        UpdateHUD();

        bool q1 = playerChoseCorrect[0] == true;
        bool q2 = playerChoseCorrect[1] == true;
        bool q3 = playerChoseCorrect[2] == true;
        bool q4 = playerChoseCorrect[3] == true;

        // ── Q1 或 Q2 錯：不划船，直接跳提示 ──
        if (!q1)
        {
            yield return ShowHintAndReset(FAIL_HINTS[0]);
            yield break;
        }
        if (!q2)
        {
            yield return ShowHintAndReset(FAIL_HINTS[1]);
            yield break;
        }

        // ── Q1 & Q2 都對，依 Q3 / Q4 決定划船 ──
        // 建一串要划的圈（依序連續生出、連續流動）
        List<RowSide> seq = new List<RowSide>();

        // 只有 Q3 對 → 5 綠(左手) → 停 → 提示(粉色判斷錯誤)
        if (q3 && !q4)
        {
            for (int i = 0; i < ROW_TIMES; i++) seq.Add(RowSide.Left);
            yield return RowCircleList(seq);
            yield return ShowHintAndReset(FAIL_HINTS[3]);
            yield break;
        }

        // 只有 Q4 對 → 5 粉(右手) → 停 → 提示(綠色判斷錯誤)
        if (!q3 && q4)
        {
            for (int i = 0; i < ROW_TIMES; i++) seq.Add(RowSide.Right);
            yield return RowCircleList(seq);
            yield return ShowHintAndReset(FAIL_HINTS[2]);
            yield break;
        }

        // Q3 & Q4 都錯 → 兩題都錯，優先提示綠色判斷
        if (!q3 && !q4)
        {
            yield return ShowHintAndReset(FAIL_HINTS[2]);
            yield break;
        }

        // Q3 & Q4 都對 → 5 綠 + 5 粉 + 5 隨機 → 通關
        for (int i = 0; i < ROW_TIMES; i++) seq.Add(RowSide.Left);   // 5 綠
        for (int i = 0; i < ROW_TIMES; i++) seq.Add(RowSide.Right);  // 5 粉
        for (int i = 0; i < ROW_TIMES; i++)                          // 5 隨機
            seq.Add((Random.Range(0, 2) == 0) ? RowSide.Left : RowSide.Right);
        yield return RowCircleList(seq);

        Debug.Log("[Rowing_Lvl2] 全對通關，呼叫 ShowEndScreen，總分=" + totalScore);
        timerRunning = false;
        ShowEndScreen();
        isRunning = false;
    }

    /// <summary>顯示提示 2.2 秒後解鎖，並結束流程。</summary>
    IEnumerator ShowHintAndReset(string msg)
    {
        ShowHint(msg, Color.red);
        yield return new WaitForSeconds(2.2f);
        failHintText?.gameObject.SetActive(false);
        ResetAndUnlock();
        isRunning = false;
    }

    // 飄動中的圈圈佇列（依生成順序），最前面的先被劃
    private Queue<GameObject> flyingCircles = new Queue<GameObject>();

    /// <summary>
    /// 依序連續生出整串圈（每隔 spawnInterval 一個），同時自動劃掉飄到判定點的圈。
    /// 生圈與劃圈同時進行，畫面上會有好幾個圈排隊流動（像 Lvl1）。
    /// </summary>
    IEnumerator RowCircleList(List<RowSide> seq)
    {
        flyingCircles.Clear();

        // 啟動「自動劃圈」協程（盯住最前面的圈）
        Coroutine rowing = StartCoroutine(AutoRowRoutine(seq.Count));

        // 連續生圈：每隔 spawnInterval 生一個，並讓它往左飄
        for (int i = 0; i < seq.Count; i++)
        {
            while (gamePaused) yield return null;

            RowSide side = seq[i];
            GameObject prefab = (side == RowSide.Left) ? greenCirclePrefab : pinkCirclePrefab;
            if (prefab != null && spawnPoint != null)
            {
                GameObject circle = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
                flyingCircles.Enqueue(circle);
                StartCoroutine(DriftCircle(circle));  // 每個圈自己往左飄
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        // 等自動劃圈把全部劃完
        yield return rowing;
    }

    /// <summary>單一圈往左飄（移動邏輯同 Lvl1）。被劃掉(Destroy)後自然結束。</summary>
    IEnumerator DriftCircle(GameObject circle)
    {
        while (circle != null)
        {
            while (gamePaused) yield return null;
            if (circle == null) yield break;
            circle.transform.Translate(Vector2.left * circleSpeed * Time.deltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// 自動劃圈：等最前面的圈飄到判定點，用對應的手劃掉它、加分，
    /// 直到劃滿 count 個。綠圈用左手、粉圈用右手（依 prefab 名稱判斷顏色）。
    /// </summary>
    IEnumerator AutoRowRoutine(int count)
    {
        int rowed = 0;
        while (rowed < count)
        {
            // 等到有圈、且最前面的圈飄到判定點
            while (flyingCircles.Count == 0 || flyingCircles.Peek() == null
                   || flyingCircles.Peek().transform.position.x > judgeX)
            {
                // 清掉已被銷毀的空項
                if (flyingCircles.Count > 0 && flyingCircles.Peek() == null)
                {
                    flyingCircles.Dequeue();
                    continue;
                }
                yield return null;
            }

            while (gamePaused) yield return null;

            GameObject circle = flyingCircles.Dequeue();
            if (circle == null) continue;

            // 用圈的顏色決定哪隻手（依生成順序＝顏色順序）
            RowSide side = IsGreenCircle(circle) ? RowSide.Left : RowSide.Right;

            if (side == RowSide.Left)
                yield return PlayRowAnim(leftHandRenderer, leftHandNormal, leftHandRow);
            else
                yield return PlayRowAnim(rightHandRenderer, rightHandNormal, rightHandRow);

            if (circle != null) Destroy(circle);

            totalScore += scorePerRow;
            UpdateHUD();
            rowed++;
        }
    }

    /// <summary>用 Tag 或名稱判斷是不是綠圈（沿用 Lvl1 的 Green/Pink Tag）。</summary>
    bool IsGreenCircle(GameObject circle)
    {
        if (circle == null) return true;
        if (circle.CompareTag("Green")) return true;
        if (circle.CompareTag("Pink")) return false;
        // 後備：用名稱判斷（prefab 名稱含 green）
        return circle.name.ToLower().Contains("green");
    }

    IEnumerator PlayRowAnim(SpriteRenderer hand, Sprite normal, Sprite row)
    {
        if (hand != null)
        {
            hand.sprite = row;
            yield return new WaitForSeconds(rowAnimDuration);
            hand.sprite = normal;
        }
    }

    // ─────────────────────────────────────────────
    //  結算
    // ─────────────────────────────────────────────

    void ShowEndScreen()
    {
        // 雙手歸位
        if (leftHandRenderer && leftHandNormal) leftHandRenderer.sprite = leftHandNormal;
        if (rightHandRenderer && rightHandNormal) rightHandRenderer.sprite = rightHandNormal;

        bool isNewRecord = hasPlayedBefore && elapsedTime < savedBestTime;
        int coinEarned = coinComplete + (isNewRecord ? coinRecord : 0);
        float newBestTime = (!hasPlayedBefore || elapsedTime < savedBestTime)
                            ? elapsedTime : savedBestTime;

        SaveData(savedCoins + coinEarned, newBestTime);

        if (endScreen == null)
            Debug.LogWarning("[Rowing_Lvl2] End Screen 欄位未指派！結算畫面不會顯示。請在 Inspector 拖入結算面板物件。");
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

    void ResetAndUnlock()
    {
        // 清掉殘留還在飄的圈
        while (flyingCircles.Count > 0)
        {
            var c = flyingCircles.Dequeue();
            if (c != null) Destroy(c);
        }

        if (leftHandRenderer && leftHandNormal) leftHandRenderer.sprite = leftHandNormal;
        if (rightHandRenderer && rightHandNormal) rightHandRenderer.sprite = rightHandNormal;
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
}