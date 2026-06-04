using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Puzzle_Lvl2.cs
/// 掛在 MakeBoatGameManger（或管理物件）上。
///
/// 玩法：兩題 Java 選擇題（選項左右隨機換），按 OK 判斷後，
/// 碎片「自動」從左邊原位平滑移動到右邊對應格子（不拖拉），
/// 一塊到位才動下一塊。
///
/// 階層（沿用 Lvl1）：
///   original group ── 9 塊碎片，物件名 = ID（11,12,13,21,22,23,31,32,33）
///   final group    ── 9 個格子，物件名 = ID + slotSuffix（預設 "_f"，如 11_f）
///
/// 拼圖規則：
///   只對第一題 → 上方橫排 11,12,13
///   只對第二題 → 左方直排 11,21,31
///   兩題都對   → 全部九塊
///
/// 兩題正解（固定）：
///   Q1 拼橫的：for (int i = 0; i < 3; i++){...}
///   Q2 拼直的：for (int j = 2; i >= 0; i--){...}
/// </summary>
public class Puzzle_Lvl2 : MonoBehaviour
{
    [Header("碎片來源 / 格子容器")]
    public Transform originalGroup;   // 左邊：碎片父物件
    public Transform finalGroup;      // 右邊：格子父物件
    [Tooltip("格子物件名 = 碎片ID + 此後綴，例如 11_f")]
    public string slotSuffix = "_f";

    [Header("移動設定")]
    public float moveSpeed = 800f;       // 像素/秒
    public float perPieceDelay = 0.1f;   // 每塊之間的間隔

    [Header("2 題的【正解】選項物件（順序 Q1, Q2）")]
    public Button[] correctButtons = new Button[2];

    [Header("2 題的【錯誤】選項物件（順序 Q1, Q2）")]
    public Button[] wrongButtons = new Button[2];

    [Header("選項選中外觀")]
    public Color optionNormalColor = Color.white;
    public Color optionSelectedColor = new Color(1f, 0.92f, 0.5f, 1f);

    [Header("確認按鈕（OK）")]
    public Button confirmButton;

    [Header("提示文字")]
    public TextMeshProUGUI failHintText;

    [Header("In-Game UI")]
    public TextMeshProUGUI txtTimer;

    [Header("Hint Panel（進場玩法說明）")]
    public GameObject hintPanel;

    [Header("Java Hint Panel（題目面板，javahint 控制開關）")]
    public GameObject javaHintPanel;

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

    [Header("結算設定")]
    public int coinComplete = 50;
    public int coinRecord = 150;

    // ── 常數 / 存檔 Key ─────────────────────────────
    private const int totalQuestions = 2;
    private const string KEY_COINS = "TotalCoins";
    private const string KEY_BESTTIME = "PuzzleLvl2_BestTime";
    private const string KEY_PLAYED = "PuzzleLvl2_HasPlayed";

    // 各情況要拼的碎片 ID
    private static readonly string[] ROW_TOP = { "11", "12", "13" };   // 只對 Q1：上方橫排
    private static readonly string[] COL_LEFT = { "11", "21", "31" };  // 只對 Q2：左方直排
    private static readonly string[] ALL_NINE = {
        "11", "12", "13", "21", "22", "23", "31", "32", "33"          // 兩題都對：全部
    };

    private static readonly string[] FAIL_HINTS = {
        "橫排迴圈錯誤！",   // Q1
        "直排迴圈錯誤！"   // Q2
    };

    // ── 執行期狀態 ──────────────────────────────────
    private bool?[] playerChoseCorrect = new bool?[totalQuestions];

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
        if (txtTimer) txtTimer.gameObject.SetActive(false);

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
    //  隨機左右換（用 SiblingIndex 交換兩個選項物件）
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
            CacheInitialPositions();   // 記錄碎片初始位置（供答錯還原）
            if (txtTimer) txtTimer.gameObject.SetActive(true);
            javaHintPanel?.SetActive(true);
            confirmButton?.gameObject.SetActive(true);
        }
        else
        {
            gamePaused = false;
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

    IEnumerator JudgeFlow()
    {
        isRunning = true;
        SetAllOptionsInteractable(false);
        if (confirmButton) confirmButton.interactable = false;
        javaHintPanel?.SetActive(false);

        bool q1 = (playerChoseCorrect[0] == true);
        bool q2 = (playerChoseCorrect[1] == true);

        // 決定要拼哪些碎片
        string[] piecesToPlace;
        if (q1 && q2) piecesToPlace = ALL_NINE;       // 兩題都對：全部
        else if (q1) piecesToPlace = ROW_TOP;         // 只對 Q1：上方橫排
        else if (q2) piecesToPlace = COL_LEFT;        // 只對 Q2：左方直排
        else piecesToPlace = new string[0];           // 都錯：不拼

        // 依序移動碎片到對應格子
        foreach (string id in piecesToPlace)
        {
            yield return StartCoroutine(MovePieceToSlot(id));
            yield return new WaitForSeconds(perPieceDelay);
        }

        // 判斷結果
        if (q1 && q2)
        {
            // 通關
            timerRunning = false;
            ShowEndScreen();
            isRunning = false;
            yield break;
        }

        // 沒全對 → 跳提示（指出第一個答錯的題目）
        int firstWrong = !q1 ? 0 : 1;
        ShowHint(FAIL_HINTS[firstWrong], Color.red);
        yield return new WaitForSeconds(2.2f);
        failHintText?.gameObject.SetActive(false);

        // 把剛剛拼上去的碎片收回原位，讓玩家重答
        yield return StartCoroutine(ResetPlacedPieces(piecesToPlace));

        ResetAndUnlock();
        isRunning = false;
    }

    // ─────────────────────────────────────────────
    //  碎片移動
    // ─────────────────────────────────────────────

    /// <summary>把 id 對應的碎片平滑移動到 id+suffix 的格子位置。</summary>
    IEnumerator MovePieceToSlot(string id)
    {
        Transform piece = FindInGroup(originalGroup, id);
        Transform slot = FindInGroup(finalGroup, id + slotSuffix);

        if (piece == null || slot == null)
        {
            Debug.LogWarning($"[Puzzle_Lvl2] 找不到碎片或格子：piece={id}, slot={id + slotSuffix}");
            yield break;
        }

        RectTransform pieceRT = piece as RectTransform;
        RectTransform slotRT = slot as RectTransform;
        if (pieceRT == null || slotRT == null) yield break;

        // 目標是格子的世界座標位置
        Vector3 targetWorld = slotRT.position;

        while (Vector3.Distance(pieceRT.position, targetWorld) > 0.5f)
        {
            if (!gamePaused)
            {
                pieceRT.position = Vector3.MoveTowards(
                    pieceRT.position, targetWorld, moveSpeed * Time.deltaTime);
            }
            yield return null;
        }
        pieceRT.position = targetWorld;
    }

    /// <summary>沒全對時把拼上去的碎片收回原位（重設位置需記錄初始位置）。</summary>
    IEnumerator ResetPlacedPieces(string[] ids)
    {
        foreach (string id in ids)
        {
            Transform piece = FindInGroup(originalGroup, id);
            if (piece == null) continue;
            RectTransform rt = piece as RectTransform;
            if (rt == null) continue;
            if (initialPosByID.TryGetValue(id, out Vector3 pos))
                rt.position = pos;
        }
        yield return null;
    }

    /// <summary>在群組底下用物件名找子物件。</summary>
    Transform FindInGroup(Transform group, string objName)
    {
        if (group == null) return null;
        // 直接子物件優先
        Transform t = group.Find(objName);
        if (t != null) return t;
        // 深層搜尋
        foreach (Transform child in group.GetComponentsInChildren<Transform>(true))
            if (child.name == objName) return child;
        return null;
    }

    // 記錄每塊碎片初始世界座標，方便答錯時還原
    private Dictionary<string, Vector3> initialPosByID = new Dictionary<string, Vector3>();

    void CacheInitialPositions()
    {
        initialPosByID.Clear();
        foreach (string id in ALL_NINE)
        {
            Transform piece = FindInGroup(originalGroup, id);
            if (piece != null)
                initialPosByID[id] = piece.position;
        }
    }

    // ─────────────────────────────────────────────
    //  結算
    // ─────────────────────────────────────────────

    void ShowEndScreen()
    {
        bool isNewRecord = hasPlayedBefore && elapsedTime < savedBestTime;
        int coinEarned = coinComplete + (isNewRecord ? coinRecord : 0);
        float newBestTime = (!hasPlayedBefore || elapsedTime < savedBestTime)
                            ? elapsedTime : savedBestTime;

        SaveData(savedCoins + coinEarned, newBestTime);

        if (endScreen == null)
            Debug.LogWarning("[Puzzle_Lvl2] End Screen 未指派！結算畫面不會顯示。");
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

    // ─────────────────────────────────────────────
    //  UTILITY
    // ─────────────────────────────────────────────

    void ShowHint(string msg, Color col)
    {
        if (failHintText == null) return;
        failHintText.text = msg;
        failHintText.color = col;
        failHintText.gameObject.SetActive(true);
    }

    void ResetAndUnlock()
    {
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