using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FindClues_lvl1 : MonoBehaviour
{
    [Header("角色設定")]
    public GameObject player;

    [Header("格子視覺設定")]
    public Sprite spriteStart;
    public Sprite spritePath;
    public Sprite spriteEnd;
    public Sprite spriteNormal;

    [Header("玩家填箭頭的方框（6個）")]
    public Transform[] slots = new Transform[6];

    [Header("位置微調")]
    public float yOffset = 70f;

    [Header("提示文字")]
    public TextMeshProUGUI failHintText;

    [Header("In-Game UI")]
    public TextMeshProUGUI txtTimer;

    [Header("Hint Panel")]
    public GameObject hintPanel;

    [Header("Quit Confirm Panel")]
    public GameObject quitConfirmPanel;
    public Button btnQuitConfirm;
    public Button btnQuitCancel;

    [Header("End Screen（ResultPanel）")]
    public GameObject endScreen;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI coinRewardText;
    public Button btnEndConfirm;

    [Header("Firebase 紀錄設定")]
    public string advancedID = "advanced_01";
    public string difficulty = "easy";

    [Header("結算設定")]
    public int coinComplete = 50;
    public int coinRecord = 150;

    private const string KEY_BESTTIME = "FindCluesLvl1_BestTime";
    private const string KEY_PLAYED = "FindCluesLvl1_HasPlayed";

    private Image[,] cellImages = new Image[3, 5];
    private Vector2Int startCell;
    private Vector2Int endCell;
    private List<Vector2Int> path = new List<Vector2Int>();
    private string[] correctAnswers = new string[6];

    private static readonly Vector2Int[] DIRS =
    {
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(0, 1)
    };

    private static readonly string[] DIR_NAMES =
    {
        "arrow up",
        "arrow down",
        "arrow left",
        "arrow right"
    };

    private Vector3 playerStartWorldPos;
    private bool isMoving = false;
    private bool gamePaused = false;
    private bool gameStarted = false;
    private float elapsedTime = 0f;
    private bool timerRunning = false;

    private float savedBestTime = 0f;
    private bool hasPlayedBefore = false;

    void OnEnable()
    {
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 5; c++)
            {
                GameObject obj = GameObject.Find((r + 1) + "_" + (c + 1));
                if (obj != null)
                    cellImages[r, c] = obj.GetComponent<Image>();
            }
        }

        for (int i = 0; i < 6; i++)
        {
            if (slots[i] == null)
            {
                GameObject obj = GameObject.Find("虛線方塊 (" + (i + 1) + ")");
                if (obj != null)
                    slots[i] = obj.transform;
            }
        }

        if (failHintText) failHintText.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(false);

        endScreen?.SetActive(false);
        quitConfirmPanel?.SetActive(false);
        hintPanel?.SetActive(true);

        btnQuitCancel?.onClick.AddListener(OnQuitCancel);

        if (player)
            playerStartWorldPos = player.transform.position;

        LoadLocalBestRecord();
        GenerateNewPuzzle();
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

    // =========================================================
    // 本地最佳紀錄：用來判斷是否破紀錄
    // =========================================================

    void LoadLocalBestRecord()
    {
        savedBestTime = PlayerPrefs.GetFloat(KEY_BESTTIME, 0f);
        hasPlayedBefore = PlayerPrefs.GetInt(KEY_PLAYED, 0) == 1;
    }

    void SaveLocalBestRecord(float newBestTime)
    {
        PlayerPrefs.SetFloat(KEY_BESTTIME, newBestTime);
        PlayerPrefs.SetInt(KEY_PLAYED, 1);
        PlayerPrefs.Save();
    }

    // =========================================================
    // Hint / Back
    // =========================================================

    public void OnClickConfirm()
    {
        hintPanel?.SetActive(false);

        if (!gameStarted)
        {
            gameStarted = true;
            timerRunning = true;

            if (txtTimer)
                txtTimer.gameObject.SetActive(true);
        }
        else
        {
            gamePaused = false;

            if (txtTimer)
                txtTimer.gameObject.SetActive(true);
        }
    }

    public void OnClickHint()
    {
        gamePaused = true;
        hintPanel?.SetActive(true);

        if (txtTimer)
            txtTimer.gameObject.SetActive(false);
    }

    public void OnClickBack()
    {
        gamePaused = true;
        quitConfirmPanel?.SetActive(true);
    }

    public void OnQuitCancel()
    {
        gamePaused = false;
        quitConfirmPanel?.SetActive(false);
    }

    // =========================================================
    // 生成題目
    // =========================================================

    void GenerateNewPuzzle()
    {
        int startRow = Random.Range(0, 3);
        int endRow = Random.Range(0, 3);

        startCell = new Vector2Int(startRow, 0);
        endCell = new Vector2Int(endRow, 4);

        path.Clear();

        if (!FindRandomPath(startCell, endCell, 6, path))
        {
            GenerateNewPuzzle();
            return;
        }

        for (int i = 0; i < 6; i++)
        {
            Vector2Int delta = path[i + 1] - path[i];

            for (int d = 0; d < 4; d++)
            {
                if (DIRS[d] == delta)
                {
                    correctAnswers[i] = DIR_NAMES[d];
                    break;
                }
            }
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
                playerStartWorldPos = player.transform.position;
            }
        }
    }

    bool FindRandomPath(Vector2Int cur, Vector2Int target, int stepsLeft, List<Vector2Int> result)
    {
        result.Add(cur);

        if (stepsLeft == 0)
        {
            if (cur == target)
                return true;

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

            if (!InBounds(next) || result.Contains(next))
                continue;

            int dist = Mathf.Abs(next.x - target.x) + Mathf.Abs(next.y - target.y);

            if (dist > stepsLeft - 1)
                continue;

            if (FindRandomPath(next, target, stepsLeft - 1, result))
                return true;
        }

        result.RemoveAt(result.Count - 1);
        return false;
    }

    bool InBounds(Vector2Int c)
    {
        return c.x >= 0 && c.x < 3 && c.y >= 0 && c.y < 5;
    }

    void UpdateCellVisuals()
    {
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 5; c++)
            {
                if (cellImages[r, c] != null && spriteNormal != null)
                    cellImages[r, c].sprite = spriteNormal;
            }
        }

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int cell = path[i];

            if (cellImages[cell.x, cell.y] == null)
                continue;

            if (i == 0)
                cellImages[cell.x, cell.y].sprite = spriteStart ?? spriteNormal;
            else if (i == path.Count - 1)
                cellImages[cell.x, cell.y].sprite = spriteEnd ?? spriteNormal;
            else
                cellImages[cell.x, cell.y].sprite = spritePath ?? spriteNormal;
        }
    }

    // =========================================================
    // 玩家按完成
    // =========================================================

    public void StartWalking()
    {
        if (!isMoving && !gamePaused && gameStarted && player != null)
            StartCoroutine(FollowCommands());
    }

    IEnumerator FollowCommands()
    {
        isMoving = true;

        for (int i = 0; i < 6; i++)
        {
            if (slots[i] == null || slots[i].childCount == 0)
            {
                ShowMessage("請填完所有箭頭！", Color.black);
                yield return new WaitForSeconds(1.5f);

                failHintText?.gameObject.SetActive(false);
                ResetPlayer();

                isMoving = false;
                yield break;
            }

            GameObject arrow = slots[i].GetChild(0).gameObject;
            Image arrowImg = arrow.GetComponent<Image>();
            Color origColor = arrowImg ? arrowImg.color : Color.white;

            if (arrowImg)
                arrowImg.color = new Color(0.4f, 0.2f, 0.6f);

            if (arrow.name.Contains(correctAnswers[i]))
            {
                Vector2Int nextCell = path[i + 1];
                GameObject nextObj = GameObject.Find((nextCell.x + 1) + "_" + (nextCell.y + 1));

                if (nextObj != null)
                {
                    Vector3 targetPos = nextObj.transform.position;
                    targetPos.y += yOffset;

                    float moveElapsed = 0f;
                    Vector3 startPos = player.transform.position;

                    while (moveElapsed < 0.4f)
                    {
                        player.transform.position = Vector3.Lerp(startPos, targetPos, moveElapsed / 0.4f);
                        moveElapsed += Time.deltaTime;
                        yield return null;
                    }

                    player.transform.position = targetPos;
                }

                if (arrowImg)
                    arrowImg.color = origColor;

                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                if (arrowImg)
                    arrowImg.color = Color.red;

                ShowMessage("再試一次吧！", Color.white);
                yield return new WaitForSeconds(1.5f);

                failHintText?.gameObject.SetActive(false);

                if (arrowImg)
                    arrowImg.color = origColor;

                ResetPlayer();

                isMoving = false;
                yield break;
            }
        }

        timerRunning = false;
        ShowEndScreen();

        isMoving = false;
    }

    // =========================================================
    // 結算：時間、破紀錄、紙鶴、Learning紀錄
    // =========================================================

    void ShowEndScreen()
    {
        bool isNewRecord = hasPlayedBefore && elapsedTime < savedBestTime;
        bool firstClear = !hasPlayedBefore;

        int coinEarned = coinComplete;

        if (isNewRecord)
            coinEarned += coinRecord;

        float newBestTime = firstClear || elapsedTime < savedBestTime
            ? elapsedTime
            : savedBestTime;

        SaveLocalBestRecord(newBestTime);

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.AddCoins(coinEarned);
            FirestoreManager.Instance.SaveAdvancedRecord(advancedID, difficulty, elapsedTime, coinEarned);
        }
        else
        {
            Debug.LogWarning("找不到 FirestoreManager，無法儲存進階紀錄與紙鶴");
        }

        endScreen?.SetActive(true);

        if (titleText)
            titleText.text = "恭喜通關！";

        if (finalTimeText)
        {
            int min = (int)(elapsedTime / 60f);
            int sec = (int)(elapsedTime % 60f);
            finalTimeText.text = string.Format("{0:00}:{1:00}", min, sec);
        }

        if (coinRewardText)
        {
            string msg = "金幣 +" + coinComplete;

            if (isNewRecord)
                msg += "\n破紀錄！+" + coinRecord;

            coinRewardText.text = msg;
        }

        Debug.Log($"進階紀錄儲存：{advancedID} / {difficulty} / time={elapsedTime:0.0} / coins={coinEarned}");
    }

    // =========================================================
    // 工具
    // =========================================================

    void ShowMessage(string msg, Color col)
    {
        if (failHintText != null)
        {
            failHintText.text = msg;
            failHintText.color = col;
            failHintText.gameObject.SetActive(true);
        }
    }

    public void ResetPlayer()
    {
        if (player != null)
            player.transform.position = playerStartWorldPos;
    }
}