using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// FindClues_lvl2.cs
/// 掛在場景的 GameManager 物件上
///
/// 8題選擇題（每題左右兩個圖片選項）
/// 玩家選完按確認 → YOU 根據選擇的邏輯在格子上走
/// 選錯 → YOU 走錯路 → 失敗
/// 全對 → YOU 走到終點 → 通關
/// </summary>
public class FindClues_lvl2 : MonoBehaviour
{
    // ══════════════════════════════════════════
    // 8題選項圖片（每題左右各兩張：原始版/發光版）
    // ══════════════════════════════════════════
    [System.Serializable]
    public class QuestionOption
    {
        public Sprite normalSprite;  // 原始版
        public Sprite glowSprite;    // 發光版（選中時）
    }

    [System.Serializable]
    public class Question
    {
        public string questionTitle;         // 題目說明文字（選用）
        public QuestionOption leftOption;    // 左選項
        public QuestionOption rightOption;   // 右選項
    }

    [Header("8題題目設定")]
    public Question[] questions = new Question[8];

    [Header("題目 UI（8題的左右按鈕，順序對應）")]
    public Button[] leftButtons = new Button[8];
    public Button[] rightButtons = new Button[8];
    public Image[] leftImages = new Image[8];
    public Image[] rightImages = new Image[8];

    [Header("確認按鈕")]
    public Button confirmButton;

    [Header("角色設定（跟等級一相同）")]
    public GameObject player;
    public float yOffset = 70f;
    public float moveSpeed = 0.4f;  // 每步移動秒數

    [Header("格子視覺設定（跟等級一相同）")]
    public Sprite spriteStart;
    public Sprite spritePath;
    public Sprite spriteEnd;
    public Sprite spriteNormal;

    [Header("提示文字")]
    public TextMeshProUGUI failHintText;
    public TextMeshProUGUI resultText;   // 通關/失敗大字

    [Header("In-Game UI")]
    public TextMeshProUGUI txtTimer;

    [Header("Hint Panel")]
    public GameObject hintPanel;

    [Header("Java Hint Panel")]
    public GameObject javaHintPanel;

    [Header("Quit Confirm Panel")]
    public GameObject quitConfirmPanel;
    public Button btnQuitConfirm;
    public Button btnQuitCancel;

    [Header("End Screen")]
    public GameObject endScreen;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI coinRewardText;
    public Button btnEndConfirm;

    [Header("結算設定")]
    public int coinComplete = 50;
    public int coinRecord = 150;

    private const string KEY_COINS = "TotalCoins";
    private const string KEY_BESTTIME = "FindCluesLvl2_BestTime";
    private const string KEY_PLAYED = "FindCluesLvl2_HasPlayed";

    // ── 玩家選擇記錄（true=選左, false=選右, null=未選）──
    private bool?[] playerChoices = new bool?[8];
    // ── 每題正確答案實際在左還是右（隨機決定）──
    private bool[] correctOnLeft = new bool[8];

    // ── 格子 ──────────────────────────────────
    private Image[,] cellImages = new Image[3, 5];
    private Vector2Int startCell;
    private Vector2Int endCell;
    private List<Vector2Int> path = new List<Vector2Int>();
    private string[] correctDirs = new string[6]; // 正確方向序列

    private static readonly Vector2Int[] DIRS = { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(0, 1) };
    private static readonly string[] DIR_NAMES = { "up", "down", "left", "right" };

    // ── 狀態 ──────────────────────────────────
    private Vector3 playerStartPos;
    private bool isRunning = false;
    private bool gamePaused = false;
    private bool gameStarted = false;
    private float elapsedTime = 0f;
    private bool timerRunning = false;

    private int savedCoins = 0;
    private float savedBestTime = 0f;
    private bool hasPlayedBefore = false;

    // ════════════════════════════════════════════
    void Start()
    {
        endScreen?.SetActive(false);
        quitConfirmPanel?.SetActive(false);
        hintPanel?.SetActive(true);
        javaHintPanel?.SetActive(false);
        confirmButton?.gameObject.SetActive(false);
        if (failHintText) failHintText.gameObject.SetActive(false);
        if (resultText) resultText.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(false);

        btnQuitConfirm?.onClick.AddListener(OnQuitConfirm);
        btnQuitCancel?.onClick.AddListener(OnQuitCancel);
        btnEndConfirm?.onClick.AddListener(OnQuitConfirm);
        confirmButton?.onClick.AddListener(OnClickConfirmAnswer);

        // 綁定選項按鈕
        for (int i = 0; i < 8; i++)
        {
            int idx = i;
            leftButtons[i]?.onClick.AddListener(() => OnSelectLeft(idx));
            rightButtons[i]?.onClick.AddListener(() => OnSelectRight(idx));
        }

        // 初始化選項圖片（隨機決定正確答案在左或右）
        for (int i = 0; i < 8; i++)
        {
            playerChoices[i] = null;
            correctOnLeft[i] = (Random.Range(0, 2) == 0); // 隨機左或右

            // 若 correctOnLeft = true：左放正確，右放錯誤
            // 若 correctOnLeft = false：左放錯誤，右放正確
            Sprite leftNormal = correctOnLeft[i] ? questions[i].leftOption.normalSprite : questions[i].rightOption.normalSprite;
            Sprite rightNormal = correctOnLeft[i] ? questions[i].rightOption.normalSprite : questions[i].leftOption.normalSprite;

            if (leftImages[i] != null) leftImages[i].sprite = leftNormal;
            if (rightImages[i] != null) rightImages[i].sprite = rightNormal;
        }

        // 抓格子
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 5; c++)
            {
                GameObject obj = GameObject.Find((r + 1) + "_" + (c + 1));
                if (obj != null) cellImages[r, c] = obj.GetComponent<Image>();
            }

        LoadData();
        GeneratePuzzle();
    }

    void Update()
    {
        if (timerRunning && !gamePaused)
        {
            elapsedTime += Time.deltaTime;
            if (txtTimer != null)
            {
                int min = (int)(elapsedTime / 60f);
                int sec = (int)(elapsedTime % 60f);
                txtTimer.text = string.Format("{0:00}:{1:00}", min, sec);
            }
        }
    }

    // ═══════════════ 資料讀寫 ═══════════════════

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

    // ═══════════════ Hint / Back ════════════════

    public void OnClickConfirm()
    {
        hintPanel?.SetActive(false);
        if (!gameStarted)
        {
            gameStarted = true;
            timerRunning = true;
            if (txtTimer) txtTimer.gameObject.SetActive(true);
            javaHintPanel?.SetActive(true);
            confirmButton?.gameObject.SetActive(true);
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
        javaHintPanel?.SetActive(false);
        confirmButton?.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(false);
    }

    public void OnClickJavaHint()
    {
        gamePaused = true;
        javaHintPanel?.SetActive(true);
        if (txtTimer) txtTimer.gameObject.SetActive(false);
    }

    public void OnClickJavaConfirm()
    {
        gamePaused = false;
        javaHintPanel?.SetActive(false);
        confirmButton?.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(true);
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

    // ═══════════════ 選項選擇 ════════════════════

    void OnSelectLeft(int idx)
    {
        if (isRunning || !gameStarted) return;
        playerChoices[idx] = true;
        UpdateOptionVisual(idx);
    }

    void OnSelectRight(int idx)
    {
        if (isRunning || !gameStarted) return;
        playerChoices[idx] = false;
        UpdateOptionVisual(idx);
    }

    void UpdateOptionVisual(int idx)
    {
        // 根據 correctOnLeft 決定哪張是哪個選項的圖
        Sprite leftGlow = correctOnLeft[idx] ? questions[idx].leftOption.glowSprite : questions[idx].rightOption.glowSprite;
        Sprite leftNormal = correctOnLeft[idx] ? questions[idx].leftOption.normalSprite : questions[idx].rightOption.normalSprite;
        Sprite rightGlow = correctOnLeft[idx] ? questions[idx].rightOption.glowSprite : questions[idx].leftOption.glowSprite;
        Sprite rightNormal = correctOnLeft[idx] ? questions[idx].rightOption.normalSprite : questions[idx].leftOption.normalSprite;

        if (playerChoices[idx] == true)
        {
            if (leftImages[idx] != null) leftImages[idx].sprite = leftGlow;
            if (rightImages[idx] != null) rightImages[idx].sprite = rightNormal;
        }
        else if (playerChoices[idx] == false)
        {
            if (leftImages[idx] != null) leftImages[idx].sprite = leftNormal;
            if (rightImages[idx] != null) rightImages[idx].sprite = rightGlow;
        }
    }

    // ═══════════════ 按確認驗證 ══════════════════

    void OnClickConfirmAnswer()
    {
        if (isRunning || !gameStarted) return;

        // 檢查是否全部都選了
        for (int i = 0; i < 8; i++)
        {
            if (playerChoices[i] == null)
            {
                ShowHint("請選擇所有題目！", Color.yellow);
                return;
            }
        }

        // 隱藏選題介面，開始執行
        javaHintPanel?.SetActive(false);
        confirmButton?.gameObject.SetActive(false);

        // 根據玩家選擇組合出執行邏輯
        // Q0: 宣告變數（正確才能執行後續）
        // Q1: 陣列宣告
        // Q2: for迴圈條件
        // Q3~Q6: 四個方向判斷
        bool[] isCorrect = new bool[8];
        for (int i = 0; i < 8; i++)
            // 玩家選左(true) + 正確在左(correctOnLeft) = 對
            // 玩家選右(false) + 正確在右(!correctOnLeft) = 對
            isCorrect[i] = (playerChoices[i] == true) == correctOnLeft[i];

        StartCoroutine(RunWithPlayerLogic(isCorrect));
    }

    // ═══════════════ 執行驗證 ════════════════════

    IEnumerator RunWithPlayerLogic(bool[] isCorrect)
    {
        isRunning = true;
        confirmButton.interactable = false;
        SetAllOptionsInteractable(false);

        // Q0 宣告變數錯 → 什麼都不動
        if (!isCorrect[0])
        {
            ShowHint("變數宣告錯誤，程式無法執行！", Color.red);
            yield return new WaitForSeconds(2f);
            failHintText?.gameObject.SetActive(false);
            ResetAndUnlock();
            yield break;
        }

        // Q1 陣列宣告錯 → 什麼都不動
        if (!isCorrect[1])
        {
            ShowHint("陣列大小錯誤，無法存取步驟！", Color.red);
            yield return new WaitForSeconds(2f);
            failHintText?.gameObject.SetActive(false);
            ResetAndUnlock();
            yield break;
        }

        // Q2 for條件錯 → 多走一步（走出界就停）
        int maxSteps = isCorrect[2] ? path.Count - 1 : path.Count; // 多一步

        Vector2Int curCell = startCell;

        for (int step = 0; step < Mathf.Min(maxSteps, path.Count - 1); step++)
        {
            // 這一步的正確方向
            Vector2Int delta = path[step + 1] - path[step];
            string correctDir = "";
            for (int d = 0; d < 4; d++)
                if (DIRS[d] == delta) { correctDir = DIR_NAMES[d]; break; }

            // 玩家選的方向（根據 Q3~Q6）
            string playerDir = GetPlayerDir(isCorrect, correctDir);

            // 計算實際移動
            Vector2Int moveDir = GetDirVector(playerDir);
            Vector2Int nextCell = curCell + moveDir;

            if (!InBounds(nextCell))
            {
                // 走出界
                ShowHint("走出邊界了！", Color.red);
                yield return new WaitForSeconds(1.5f);
                failHintText?.gameObject.SetActive(false);
                ResetAndUnlock();
                yield break;
            }

            // 移動 YOU
            yield return StartCoroutine(MovePlayer(nextCell));
            curCell = nextCell;

            // 如果走到的不是路徑格（走錯了）
            bool onPath = path.Contains(nextCell);
            if (!onPath && nextCell != endCell)
            {
                ShowHint("走錯路了！", Color.red);
                yield return new WaitForSeconds(1.5f);
                failHintText?.gameObject.SetActive(false);
                ResetAndUnlock();
                yield break;
            }
        }

        // 判斷是否到達終點
        if (curCell == endCell)
        {
            timerRunning = false;
            ShowEndScreen();
        }
        else
        {
            ShowHint("沒有到達終點！", Color.red);
            yield return new WaitForSeconds(1.5f);
            failHintText?.gameObject.SetActive(false);
            ResetAndUnlock();
        }

        isRunning = false;
    }

    /// <summary>
    /// 根據玩家選的方向邏輯，決定這一步實際走哪個方向
    /// Q3=up, Q4=down, Q5=left, Q6=right
    /// 若玩家選錯，該方向的判斷就失效（用 == 而不是 =，或走錯格）
    /// </summary>
    string GetPlayerDir(bool[] isCorrect, string correctDir)
    {
        // 每個方向對應的題目 index
        // Q3=up(3), Q4=down(4), Q5=left(5), Q6=right(6)
        Dictionary<string, int> dirQuestionIdx = new Dictionary<string, int>
        {
            { "up",    3 },
            { "down",  4 },
            { "left",  5 },
            { "right", 6 },
        };

        // 若正確方向對應的題目選錯，就不走那個方向（走反方向或不動）
        if (dirQuestionIdx.ContainsKey(correctDir))
        {
            int qIdx = dirQuestionIdx[correctDir];
            if (!isCorrect[qIdx])
            {
                // 錯誤：slots[i] = up（賦值不比較），相當於條件永遠成立或不成立
                // 這裡模擬「判斷失效，走了上一個方向或原地」
                return "none";
            }
        }
        return correctDir;
    }

    Vector2Int GetDirVector(string dir)
    {
        switch (dir)
        {
            case "up": return new Vector2Int(-1, 0);
            case "down": return new Vector2Int(1, 0);
            case "left": return new Vector2Int(0, -1);
            case "right": return new Vector2Int(0, 1);
            default: return Vector2Int.zero;
        }
    }

    // ═══════════════ 格子生成 ════════════════════

    void GeneratePuzzle()
    {
        int startRow = Random.Range(0, 3);
        int endRow = Random.Range(0, 3);
        startCell = new Vector2Int(startRow, 0);
        endCell = new Vector2Int(endRow, 4);

        path.Clear();
        if (!FindRandomPath(startCell, endCell, 6, path))
        {
            GeneratePuzzle();
            return;
        }

        for (int i = 0; i < 6; i++)
        {
            Vector2Int delta = path[i + 1] - path[i];
            for (int d = 0; d < 4; d++)
                if (DIRS[d] == delta) { correctDirs[i] = DIR_NAMES[d]; break; }
        }

        UpdateCellVisuals();

        if (player != null)
        {
            GameObject startObj = GameObject.Find((startCell.x + 1) + "_" + (startCell.y + 1));
            if (startObj != null)
            {
                Vector3 pos = startObj.transform.position;
                pos.y += yOffset;
                player.transform.position = pos;
                playerStartPos = player.transform.position;
            }
        }
    }

    bool FindRandomPath(Vector2Int cur, Vector2Int target, int stepsLeft, List<Vector2Int> result)
    {
        result.Add(cur);
        if (stepsLeft == 0)
        {
            if (cur == target) return true;
            result.RemoveAt(result.Count - 1);
            return false;
        }
        int[] order = { 0, 1, 2, 3 };
        for (int i = 3; i > 0; i--) { int j = Random.Range(0, i + 1); (order[i], order[j]) = (order[j], order[i]); }
        foreach (int d in order)
        {
            Vector2Int next = cur + DIRS[d];
            if (!InBounds(next) || result.Contains(next)) continue;
            int dist = Mathf.Abs(next.x - target.x) + Mathf.Abs(next.y - target.y);
            if (dist > stepsLeft - 1) continue;
            if (FindRandomPath(next, target, stepsLeft - 1, result)) return true;
        }
        result.RemoveAt(result.Count - 1);
        return false;
    }

    bool InBounds(Vector2Int c) => c.x >= 0 && c.x < 3 && c.y >= 0 && c.y < 5;

    void UpdateCellVisuals()
    {
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 5; c++)
                if (cellImages[r, c] != null && spriteNormal != null)
                    cellImages[r, c].sprite = spriteNormal;

        for (int i = 0; i < path.Count; i++)
        {
            var cell = path[i];
            if (cellImages[cell.x, cell.y] == null) continue;
            if (i == 0) cellImages[cell.x, cell.y].sprite = spriteStart ?? spriteNormal;
            else if (i == path.Count - 1) cellImages[cell.x, cell.y].sprite = spriteEnd ?? spriteNormal;
            else cellImages[cell.x, cell.y].sprite = spritePath ?? spriteNormal;
        }
    }

    // ═══════════════ YOU 移動 ════════════════════

    IEnumerator MovePlayer(Vector2Int targetCell)
    {
        GameObject targetObj = GameObject.Find((targetCell.x + 1) + "_" + (targetCell.y + 1));
        if (targetObj == null) yield break;

        Vector3 targetPos = targetObj.transform.position;
        targetPos.y += yOffset;

        float elapsed = 0f;
        Vector3 startPos = player.transform.position;
        while (elapsed < moveSpeed)
        {
            player.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / moveSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }
        player.transform.position = targetPos;
        yield return new WaitForSeconds(0.15f);
    }

    // ═══════════════ 結算 ════════════════════════

    void ShowEndScreen()
    {
        bool isNewRecord = hasPlayedBefore && elapsedTime < savedBestTime;
        int coinEarned = coinComplete + (isNewRecord ? coinRecord : 0);
        float newBestTime = (!hasPlayedBefore || elapsedTime < savedBestTime) ? elapsedTime : savedBestTime;

        SaveData(savedCoins + coinEarned, newBestTime);
        endScreen?.SetActive(true);

        if (titleText) titleText.text = "恭喜通關！";
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

    // ═══════════════ 工具 ════════════════════════

    void ShowHint(string msg, Color col)
    {
        if (failHintText == null) return;
        failHintText.text = msg;
        failHintText.color = col;
        failHintText.gameObject.SetActive(true);
    }

    void ResetAndUnlock()
    {
        isRunning = false;
        if (player != null) player.transform.position = playerStartPos;
        confirmButton.interactable = true;
        SetAllOptionsInteractable(true);
    }

    void SetAllOptionsInteractable(bool v)
    {
        for (int i = 0; i < 8; i++)
        {
            if (leftButtons[i]) leftButtons[i].interactable = v;
            if (rightButtons[i]) rightButtons[i].interactable = v;
        }
    }
}