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

    [Header("Slot 顏色設定")]
    public Color slotNormalColor = Color.white;
    public Color slotActiveColor = new Color(0.6f, 0.6f, 0.6f, 1f); // 變深

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
        new Vector2Int(-1, 0),  // up
        new Vector2Int( 1, 0),  // down
        new Vector2Int( 0,-1),  // left
        new Vector2Int( 0, 1)   // right
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

    // ─────────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────────

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
                    string tmp = leftTexts[i].text;
                    leftTexts[i].text = rightTexts[i].text;
                    rightTexts[i].text = tmp;
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
        if (!timerRunning || gamePaused) return;

        elapsedTime += Time.deltaTime;
        if (txtTimer != null)
        {
            int min = (int)(elapsedTime / 60f);
            int sec = (int)(elapsedTime % 60f);
            txtTimer.text = string.Format("{0:00}:{1:00}", min, sec);
        }
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
    //  UI BUTTON HANDLERS
    // ─────────────────────────────────────────────

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
        javaHintPanel?.SetActive(!javaHintPanel.activeSelf);
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

    // ─────────────────────────────────────────────
    //  CONFIRM ANSWER
    // ─────────────────────────────────────────────

    void UpdateSlotHighlight(int currentStep)
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] == null) continue;
            slotImages[i].color = (i == currentStep) ? slotActiveColor : slotNormalColor;
        }
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

        FillSlots(isCorrect);
        StartCoroutine(RunWithPlayerLogic(isCorrect));
    }

    // ─────────────────────────────────────────────
    //  FILL SLOTS
    //  Q1 錯 → 全清
    //  Q2 錯 → 只顯示第 1 格
    //  Q4-Q7 對應方向錯 → 那格不顯示
    // ─────────────────────────────────────────────

    void FillSlots(bool[] isCorrect)
    {
        // Q1 選錯：全清
        if (!isCorrect[0])
        {
            ClearAllSlots();
            return;
        }

        var dirToQ = new Dictionary<string, int>
        { { "up", 3 }, { "down", 4 }, { "left", 5 }, { "right", 6 } };
        var dirToRot = new Dictionary<string, float>
        { { "up", 180f }, { "down", 0f }, { "left", 270f }, { "right", 90f } };

        // Q2 選錯：最多顯示 1 格
        int maxSlots = isCorrect[1] ? slotImages.Length : 1;

        for (int step = 0; step < path.Count - 1; step++)
        {
            if (step >= slotImages.Length) break;

            // 超過 maxSlots 的格子清空
            if (step >= maxSlots)
            {
                if (slotImages[step] != null)
                {
                    slotImages[step].sprite = spriteEmpty;
                    slotImages[step].transform.rotation = Quaternion.identity;
                }
                continue;
            }

            string dir = DeltaToDir(path[step + 1] - path[step]);

            // 對應方向選錯 → 這格留空，不往前補
            bool dirCorrect = !dirToQ.ContainsKey(dir) || isCorrect[dirToQ[dir]];

            if (slotImages[step] != null)
            {
                if (dirCorrect)
                {
                    slotImages[step].sprite = spriteUp;
                    slotImages[step].transform.rotation =
                        Quaternion.Euler(0, 0, dirToRot[dir]);
                }
                else
                {
                    slotImages[step].sprite = spriteEmpty;
                    slotImages[step].transform.rotation = Quaternion.identity;
                }
            }
        }
    }

    void ClearAllSlots()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] != null)
            {
                slotImages[i].sprite = spriteEmpty;
                slotImages[i].transform.rotation = Quaternion.identity;
            }
        }
    }

    // ─────────────────────────────────────────────
    //  MAIN COROUTINE
    // ─────────────────────────────────────────────

    IEnumerator RunWithPlayerLogic(bool[] isCorrect)
    {
        isRunning = true;
        confirmButton.interactable = false;
        SetAllOptionsInteractable(false);

        // ── Q1 選錯：不動、不提示 ──
        if (!isCorrect[0])
        {
            ShowHint("a變數宣告錯誤，程式無法執行！", Color.red);
            yield return new WaitForSeconds(2f);
            failHintText?.gameObject.SetActive(false);
            ResetAndUnlock();
            yield break;
        }

        // ── Q2 選錯：不動、提示 ──
        if (!isCorrect[1])
        {
            ShowHint("b陣列大小錯誤，無法存取步驟！", Color.red);
            yield return new WaitForSeconds(2f);
            failHintText?.gameObject.SetActive(false);
            ResetAndUnlock();
            yield break;
        }

        int normalSteps = path.Count - 1; // = 6
        Vector2Int curCell = startCell;
        bool failed = false;

        // ── 正常走 6 步 ──
        for (int step = 0; step < normalSteps; step++)
        {
            string correctDir = DeltaToDir(path[step + 1] - path[step]);
            string playerDir = GetPlayerDir(isCorrect, correctDir);

            // Q4-Q7 對應方向選錯 → 停住
            if (playerDir == "none")
            {
                ShowHint("c方向指令錯誤，角色無法移動！", Color.red);
                yield return new WaitForSeconds(2f);
                failHintText?.gameObject.SetActive(false);
                failed = true;
                break;
            }

            Vector2Int nextCell = curCell + GetDirVector(playerDir);

            if (!InBounds(nextCell))
            {
                ShowHint("d走出邊界了！", Color.red);
                yield return new WaitForSeconds(1.5f);
                failHintText?.gameObject.SetActive(false);
                failed = true;
                break;
            }

            UpdateSlotHighlight(step);                          // ← 先變色
            yield return StartCoroutine(MovePlayer(nextCell));  // ← 再移動
            curCell = nextCell;
            

            // 提早踩到終點（還未走完）→ 失敗
            if (curCell == endCell && step < normalSteps - 1)
            {
                ShowHint("e路徑不完整，需要走完所有格子！", Color.red);
                yield return new WaitForSeconds(1.5f);
                failHintText?.gameObject.SetActive(false);
                failed = true;
                break;
            }

            // 走到不在路徑上的格子 → 失敗
            if (!path.Contains(curCell) && curCell != endCell)
            {
                ShowHint("f走錯路了！", Color.red);
                yield return new WaitForSeconds(1.5f);
                failHintText?.gameObject.SetActive(false);
                failed = true;
                break;
            }
        }

        // ── Q3 選錯：走完 6 步後，最後方向再多走一步 ──
        if (!failed && !isCorrect[2])
        {
            string lastDir = DeltaToDir(path[normalSteps] - path[normalSteps - 1]);
            string playerDir = GetPlayerDir(isCorrect, lastDir);

            if (playerDir == "none")
            {
                // 對應方向題也選錯，停在原地提示
                ShowHint("g迴圈判斷錯誤，多執行了一次！", Color.red);
                yield return new WaitForSeconds(2f);
                failHintText?.gameObject.SetActive(false);
                failed = true;
            }
            else
            {
                Vector2Int extraCell = curCell + GetDirVector(playerDir);

                if (InBounds(extraCell))
                {
                    // 在界內：正常播動畫走過去
                    UpdateSlotHighlight(normalSteps);                   // ← 先變色
                    yield return StartCoroutine(MovePlayer(extraCell));
                    curCell = extraCell;
                    
                    ShowHint("h迴圈判斷錯誤，多走了一步！", Color.red);
                    yield return new WaitForSeconds(2f);
                    failHintText?.gameObject.SetActive(false);
                    failed = true;
                }
                else
                {
                    // 走出界：計算出界外世界座標，播動畫後提示
                    yield return StartCoroutine(MovePlayerOutOfBounds(curCell, playerDir));
                    ShowHint("i迴圈判斷錯誤，走出邊界！", Color.red);
                    yield return new WaitForSeconds(2f);
                    failHintText?.gameObject.SetActive(false);
                    failed = true;
                }
            }
        }

        // ── 最終判斷 ──
        if (!failed)
        {
            if (curCell == endCell)
            {
                timerRunning = false;
                ShowEndScreen();
            }
            else
            {
                ShowHint("j沒有到達終點！", Color.red);
                yield return new WaitForSeconds(1.5f);
                failHintText?.gameObject.SetActive(false);
                ResetAndUnlock();
            }
        }
        else
        {
            ResetAndUnlock();
        }

        isRunning = false;
    }

    // ─────────────────────────────────────────────
    //  HELPER：走出界外的動畫
    // ─────────────────────────────────────────────

    IEnumerator MovePlayerOutOfBounds(Vector2Int fromCell, string dir)
    {
        if (player == null) yield break;

        GameObject fromObj = GameObject.Find((fromCell.x + 1) + "_" + (fromCell.y + 1));
        if (fromObj == null) yield break;

        // 找任意鄰格算出格子間距
        Vector3 cellOffset = Vector3.zero;
        for (int d = 0; d < 4; d++)
        {
            Vector2Int neighbor = fromCell + DIRS[d];
            if (!InBounds(neighbor)) continue;
            GameObject neighborObj = GameObject.Find((neighbor.x + 1) + "_" + (neighbor.y + 1));
            if (neighborObj != null)
            {
                cellOffset = neighborObj.transform.position - fromObj.transform.position;
                break;
            }
        }

        // 把格子間距投影到移動方向
        Vector2Int dirVec = GetDirVector(dir);
        // dirVec.x = row 變化（對應 world Y），dirVec.y = col 變化（對應 world X）
        // 用已知鄰格間距推算單軸位移
        Vector3 moveWorld = Vector3.zero;

        // 找一個在同 row 或同 col 的鄰格來精確取對應軸的間距
        // 否則直接用 cellOffset 的 x/y 分量
        // row 方向（up/down）→ 用 cellOffset.y
        // col 方向（left/right）→ 用 cellOffset.x
        if (dirVec.x != 0) // up or down：row 改變
        {
            // 找上或下方向的鄰格間距
            Vector2Int rowNeighbor = fromCell + new Vector2Int(dirVec.x, 0);
            if (InBounds(rowNeighbor))
            {
                GameObject rObj = GameObject.Find((rowNeighbor.x + 1) + "_" + (rowNeighbor.y + 1));
                if (rObj != null)
                    moveWorld = rObj.transform.position - fromObj.transform.position;
                else
                    moveWorld = new Vector3(0, dirVec.x * Mathf.Abs(cellOffset.y), 0);
            }
            else
            {
                // 用反方向鄰格取絕對間距，再反向
                Vector2Int opposite = fromCell + new Vector2Int(-dirVec.x, 0);
                if (InBounds(opposite))
                {
                    GameObject oObj = GameObject.Find((opposite.x + 1) + "_" + (opposite.y + 1));
                    if (oObj != null)
                        moveWorld = fromObj.transform.position - oObj.transform.position;
                    else
                        moveWorld = new Vector3(0, dirVec.x * Mathf.Abs(cellOffset.y), 0);
                }
                else
                    moveWorld = new Vector3(0, dirVec.x * Mathf.Abs(cellOffset.y), 0);
            }
        }
        else // left or right：col 改變
        {
            Vector2Int colNeighbor = fromCell + new Vector2Int(0, dirVec.y);
            if (InBounds(colNeighbor))
            {
                GameObject cObj = GameObject.Find((colNeighbor.x + 1) + "_" + (colNeighbor.y + 1));
                if (cObj != null)
                    moveWorld = cObj.transform.position - fromObj.transform.position;
                else
                    moveWorld = new Vector3(dirVec.y * Mathf.Abs(cellOffset.x), 0, 0);
            }
            else
            {
                Vector2Int opposite = fromCell + new Vector2Int(0, -dirVec.y);
                if (InBounds(opposite))
                {
                    GameObject oObj = GameObject.Find((opposite.x + 1) + "_" + (opposite.y + 1));
                    if (oObj != null)
                        moveWorld = fromObj.transform.position - oObj.transform.position;
                    else
                        moveWorld = new Vector3(dirVec.y * Mathf.Abs(cellOffset.x), 0, 0);
                }
                else
                    moveWorld = new Vector3(dirVec.y * Mathf.Abs(cellOffset.x), 0, 0);
            }
        }

        Vector3 targetPos = fromObj.transform.position + moveWorld;
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

    // ─────────────────────────────────────────────
    //  HELPER METHODS
    // ─────────────────────────────────────────────

    string DeltaToDir(Vector2Int delta)
    {
        for (int d = 0; d < 4; d++)
            if (DIRS[d] == delta) return DIR_NAMES[d];
        return "";
    }

    string GetPlayerDir(bool[] isCorrect, string correctDir)
    {
        var map = new Dictionary<string, int>
            { { "up", 3 }, { "down", 4 }, { "left", 5 }, { "right", 6 } };

        if (map.ContainsKey(correctDir) && !isCorrect[map[correctDir]])
            return "none";

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

    // ─────────────────────────────────────────────
    //  PUZZLE GENERATION
    // ─────────────────────────────────────────────

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
            correctDirs[i] = DeltaToDir(path[i + 1] - path[i]);

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
                if (cellImages[r, c] != null)
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

    // ─────────────────────────────────────────────
    //  MOVE PLAYER
    // ─────────────────────────────────────────────

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

    // ─────────────────────────────────────────────
    //  END SCREEN
    // ─────────────────────────────────────────────

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
        isRunning = false;
        if (player != null) player.transform.position = playerStartPos;

        // 重置所有 slot 顏色
        for (int i = 0; i < slotImages.Length; i++)
            if (slotImages[i] != null)
                slotImages[i].color = slotNormalColor;

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