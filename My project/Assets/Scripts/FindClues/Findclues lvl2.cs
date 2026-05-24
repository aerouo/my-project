using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class FindClues_lvl2 : MonoBehaviour
{
    [System.Serializable]
    public class QuestionOption
    {
        public Sprite normalSprite;
        public Sprite glowSprite;
    }

    [System.Serializable]
    public class Question
    {
        public string questionTitle;
        public QuestionOption leftOption;
        public QuestionOption rightOption;
    }

    [Header("下方6個空格的Image")]
    public Image[] slotImages = new Image[6];

    [Header("方向圖片（只需填 Sprite Up 和 Sprite Empty）")]
    public Sprite spriteUp;
    public Sprite spriteEmpty;

    [Header("7題題目設定")]
    public Question[] questions = new Question[7];

    [Header("按鈕文字（左右各7個）")]
    public TextMeshProUGUI[] leftTexts = new TextMeshProUGUI[7];
    public TextMeshProUGUI[] rightTexts = new TextMeshProUGUI[7];

    [Header("題目 UI（7題的左右按鈕，順序對應）")]
    public Button[] leftButtons = new Button[7];
    public Button[] rightButtons = new Button[7];
    public Image[] leftImages = new Image[7];
    public Image[] rightImages = new Image[7];

    [Header("確認按鈕")]
    public Button confirmButton;

    [Header("角色設定")]
    public GameObject player;
    public float yOffset = 70f;
    public float moveSpeed = 0.4f;

    [Header("格子視覺設定")]
    public Sprite spriteStart;
    public Sprite spritePath;
    public Sprite spriteEnd;
    public Sprite spriteNormal;

    [Header("提示文字")]
    public TextMeshProUGUI failHintText;
    public TextMeshProUGUI resultText;

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

    private bool?[] playerChoices = new bool?[7];
    private bool[] correctOnLeft = new bool[7];

    private Image[,] cellImages = new Image[3, 5];
    private Vector2Int startCell;
    private Vector2Int endCell;
    private List<Vector2Int> path = new List<Vector2Int>();
    private string[] correctDirs = new string[6];

    private static readonly Vector2Int[] DIRS = {
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(0, 1)
    };
    private static readonly string[] DIR_NAMES = { "up", "down", "left", "right" };

    private Vector3 playerStartPos;
    private bool isRunning = false;
    private bool gamePaused = false;
    private bool gameStarted = false;
    private float elapsedTime = 0f;
    private bool timerRunning = false;

    private int savedCoins = 0;
    private float savedBestTime = 0f;
    private bool hasPlayedBefore = false;

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

        for (int i = 0; i < 7; i++)
        {
            int idx = i;
            leftButtons[i]?.onClick.AddListener(() => OnSelectLeft(idx));
            rightButtons[i]?.onClick.AddListener(() => OnSelectRight(idx));
        }

        for (int i = 0; i < 7; i++)
        {
            playerChoices[i] = null;
            correctOnLeft[i] = (Random.Range(0, 2) == 0);

            if (!correctOnLeft[i])
            {
                if (leftTexts[i] != null && rightTexts[i] != null)
                {
                    string leftStr = leftTexts[i].text;
                    string rightStr = rightTexts[i].text;
                    leftTexts[i].text = rightStr;
                    rightTexts[i].text = leftStr;
                }
            }
        }

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
        bool isActive = javaHintPanel.activeSelf;
        javaHintPanel?.SetActive(!isActive);
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

    public void OnSelectLeft(int idx)
    {
        if (isRunning || !gameStarted) return;
        playerChoices[idx] = true;
        UpdateOptionVisual(idx);
    }

    public void OnSelectRight(int idx)
    {
        if (isRunning || !gameStarted) return;
        playerChoices[idx] = false;
        UpdateOptionVisual(idx);
    }

    void UpdateOptionVisual(int idx)
    {
        if (leftImages[idx] != null)
            leftImages[idx].sprite = (playerChoices[idx] == true)
                ? questions[idx].leftOption.glowSprite
                : questions[idx].leftOption.normalSprite;

        if (rightImages[idx] != null)
            rightImages[idx].sprite = (playerChoices[idx] == false)
                ? questions[idx].rightOption.glowSprite
                : questions[idx].rightOption.normalSprite;
    }

    void OnClickConfirmAnswer()
    {
        if (isRunning || !gameStarted) return;

        for (int i = 0; i < 7; i++)
        {
            if (playerChoices[i] == null)
            {
                ShowHint("請選擇所有題目！", Color.yellow);
                return;
            }
        }

        javaHintPanel?.SetActive(false);
        confirmButton?.gameObject.SetActive(false);

        bool[] isCorrect = new bool[7];
        for (int i = 0; i < 7; i++)
            isCorrect[i] = (playerChoices[i] == true) == correctOnLeft[i];

        // 填入下方空格
        FillSlots(isCorrect);

        // 讓 YOU 走路
        StartCoroutine(RunWithPlayerLogic(isCorrect));
    }

    void FillSlots(bool[] isCorrect)
    {
        // Q4=up(3), Q5=down(4), Q6=left(5), Q7=right(6)
        Dictionary<string, int> dirToQ = new Dictionary<string, int>();
        dirToQ.Add("up", 3);
        dirToQ.Add("down", 4);
        dirToQ.Add("left", 5);
        dirToQ.Add("right", 6);

        // 箭頭預設朝左，旋轉對應方向
        Dictionary<string, float> dirToRotation = new Dictionary<string, float>();
        dirToRotation.Add("up", 180f);
        dirToRotation.Add("down", 0f);
        dirToRotation.Add("left", 270f);
        dirToRotation.Add("right", 90f);

        int slotIndex = 0;

        for (int step = 0; step < path.Count - 1; step++)
        {
            if (slotIndex >= slotImages.Length) break;

            Vector2Int delta = path[step + 1] - path[step];
            string dir = "";
            for (int d = 0; d < 4; d++)
                if (DIRS[d] == delta) { dir = DIR_NAMES[d]; break; }

            if (dirToQ.ContainsKey(dir) && isCorrect[dirToQ[dir]])
            {
                if (slotImages[slotIndex] != null)
                {
                    slotImages[slotIndex].sprite = spriteUp;
                    slotImages[slotIndex].transform.rotation =
                        Quaternion.Euler(0, 0, dirToRotation[dir]);
                }
                slotIndex++;
            }
        }

        for (int i = slotIndex; i < slotImages.Length; i++)
        {
            if (slotImages[i] != null)
            {
                slotImages[i].sprite = spriteEmpty;
                slotImages[i].transform.rotation = Quaternion.identity;
            }
        }
    }

    IEnumerator RunWithPlayerLogic(bool[] isCorrect)
    {
        isRunning = true;
        confirmButton.interactable = false;
        SetAllOptionsInteractable(false);

        // Q1 選錯 → 什麼都不動
        if (!isCorrect[0])
        {
            ShowHint("變數宣告錯誤，程式無法執行！", Color.red);
            yield return new WaitForSeconds(2f);
            failHintText?.gameObject.SetActive(false);
            ResetAndUnlock();
            yield break;
        }

        // Q2 選錯 → 什麼都不動
        if (!isCorrect[1])
        {
            ShowHint("陣列大小錯誤，無法存取步驟！", Color.red);
            yield return new WaitForSeconds(2f);
            failHintText?.gameObject.SetActive(false);
            ResetAndUnlock();
            yield break;
        }

        // Q3 選錯 → 多走一步
        int maxSteps = isCorrect[2] ? path.Count - 1 : path.Count;

        Vector2Int curCell = startCell;

        for (int step = 0; step < Mathf.Min(maxSteps, path.Count - 1); step++)
        {
            Vector2Int delta = path[step + 1] - path[step];
            string correctDir = "";
            for (int d = 0; d < 4; d++)
                if (DIRS[d] == delta) { correctDir = DIR_NAMES[d]; break; }

            string playerDir = GetPlayerDir(isCorrect, correctDir);
            Vector2Int moveDir = GetDirVector(playerDir);
            Vector2Int nextCell = curCell + moveDir;

            if (!InBounds(nextCell))
            {
                ShowHint("走出邊界了！", Color.red);
                yield return new WaitForSeconds(1.5f);
                failHintText?.gameObject.SetActive(false);
                ResetAndUnlock();
                yield break;
            }

            yield return StartCoroutine(MovePlayer(nextCell));
            curCell = nextCell;

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

    string GetPlayerDir(bool[] isCorrect, string correctDir)
    {
        // Q4=up(3), Q5=down(4), Q6=left(5), Q7=right(6)
        Dictionary<string, int> dirQuestionIdx = new Dictionary<string, int>();
        dirQuestionIdx.Add("up", 3);
        dirQuestionIdx.Add("down", 4);
        dirQuestionIdx.Add("left", 5);
        dirQuestionIdx.Add("right", 6);

        if (dirQuestionIdx.ContainsKey(correctDir))
        {
            int qIdx = dirQuestionIdx[correctDir];
            if (!isCorrect[qIdx])
                return "none";
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
        for (int i = 3; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }
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
            if (i == 0)
                cellImages[cell.x, cell.y].sprite = spriteStart ?? spriteNormal;
            else if (i == path.Count - 1)
                cellImages[cell.x, cell.y].sprite = spriteEnd ?? spriteNormal;
            else
                cellImages[cell.x, cell.y].sprite = spritePath ?? spriteNormal;
        }
    }

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

    void ShowEndScreen()
    {
        bool isNewRecord = hasPlayedBefore && elapsedTime < savedBestTime;
        int coinEarned = coinComplete + (isNewRecord ? coinRecord : 0);
        float newBestTime = (!hasPlayedBefore || elapsedTime < savedBestTime)
            ? elapsedTime : savedBestTime;

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
        confirmButton.gameObject.SetActive(true);   
        javaHintPanel?.SetActive(true);
        SetAllOptionsInteractable(true);
    }

    void SetAllOptionsInteractable(bool v)
    {
        for (int i = 0; i < 7; i++)
        {
            if (leftButtons[i]) leftButtons[i].interactable = v;
            if (rightButtons[i]) rightButtons[i].interactable = v;
        }
    }
}