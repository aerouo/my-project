using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class FindClues_lvl1 : MonoBehaviour
{
    [Header("¨¤¦ג³]©w")]
    public GameObject player;

    [Header("®ז¤lµרÄ±³]©w")]
    public Sprite spriteStart;
    public Sprite spritePath;
    public Sprite spriteEnd;
    public Sprite spriteNormal;

    [Header("×±®a¶ס½bÀY×÷¤ט®Ø¡]6­Ó¡^")]
    public Transform[] slots = new Transform[6];

    [Header("¦ל¸m·L½Õ")]
    public float yOffset = 70f;

    [Header("´£¥Ü¤ו¦r")]
    public TextMeshProUGUI failHintText;

    [Header("In-Game UI")]
    public TextMeshProUGUI txtTimer;

    [Header("Hint Panel")]
    public GameObject hintPanel;

    [Header("Quit Confirm Panel")]
    public GameObject quitConfirmPanel;
    public Button btnQuitConfirm;
    public Button btnQuitCancel;

    [Header("End Screen¡]ResultPanel¡^")]
    public GameObject endScreen;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI coinRewardText;
    public Button btnEndConfirm;

    [Header("µ²÷ג³]©w")]
    public int coinComplete = 50;
    public int coinRecord = 150;

    private const string KEY_COINS = "TotalCoins";
    private const string KEY_BESTTIME = "FindCluesLvl1_BestTime";
    private const string KEY_PLAYED = "FindCluesLvl1_HasPlayed";

    // ¢w¢w ®ז¤l ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
    private Image[,] cellImages = new Image[3, 5];
    private Vector2Int startCell;
    private Vector2Int endCell;
    private List<Vector2Int> path = new List<Vector2Int>();
    private string[] correctAnswers = new string[6];

    private static readonly Vector2Int[] DIRS = { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(0, 1) };
    private static readonly string[] DIR_NAMES = { "½bÀY (¤W)", "½bÀY (¤U)", "½bÀY (¥×)", "½bÀY (¥k)" };

    // ¢w¢w ×¬÷A ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
    private Vector3 playerStartWorldPos;
    private bool isMoving = false;
    private bool gamePaused = false;
    private bool gameStarted = false;
    private float elapsedTime = 0f;
    private bool timerRunning = false;

    private int savedCoins = 0;
    private float savedBestTime = 0f;
    private bool hasPlayedBefore = false;

    // שששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששששש
    void OnEnable()
    {
        // ¦Û°Ê§ל®ז¤l Image
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 5; c++)
            {
                GameObject obj = GameObject.Find((r + 1) + "_" + (c + 1));
                if (obj != null) cellImages[r, c] = obj.GetComponent<Image>();
            }

        // ¦Û°Ê§ל slots
        for (int i = 0; i < 6; i++)
            if (slots[i] == null)
            {
                GameObject obj = GameObject.Find("µך½u¤ט¶פ (" + (i + 1) + ")");
                if (obj != null) slots[i] = obj.transform;
            }

        if (failHintText) failHintText.gameObject.SetActive(false);
        if (txtTimer) txtTimer.gameObject.SetActive(false);
        endScreen?.SetActive(false);
        quitConfirmPanel?.SetActive(false);
        hintPanel?.SetActive(true);

        btnQuitConfirm?.onClick.AddListener(OnQuitConfirm);
        btnQuitCancel?.onClick.AddListener(OnQuitCancel);
        btnEndConfirm?.onClick.AddListener(OnQuitConfirm);

        if (player) playerStartWorldPos = player.transform.position;

        LoadData();
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

    // שששששששששששששששששששששששששששששש ¸ך®ÆÅ×¼g שששששששששששששששששששששששששששששששששששששששששששששש

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

    // שששששששששששששששששששששששששששששש Hint / Back שששששששששששששששששששששששששששששששששששששששששש

    public void OnClickConfirm()
    {
        hintPanel?.SetActive(false);
        if (!gameStarted)
        {
            gameStarted = true;
            timerRunning = true;
            if (txtTimer) txtTimer.gameObject.SetActive(true);
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
        if (txtTimer) txtTimer.gameObject.SetActive(false);
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

    // שששששששששששששששששששששששששששששש ¥Í¦¨ÃD¥Ø שששששששששששששששששששששששששששששששששששששששששששששששש

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
                if (DIRS[d] == delta) { correctAnswers[i] = DIR_NAMES[d]; break; }
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

    // שששששששששששששששששששששששששששששש ×±®a«צ§¹¦¨ שששששששששששששששששששששששששששששששששששששששששששש

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
                ShowMessage("½Ð¶ס§¹©Ò¦³½bÀY¡I", Color.yellow);
                yield return new WaitForSeconds(1.5f);
                failHintText?.gameObject.SetActive(false);
                ResetPlayer();
                isMoving = false;
                yield break;
            }

            GameObject arrow = slots[i].GetChild(0).gameObject;
            Image arrowImg = arrow.GetComponent<Image>();
            Color origColor = arrowImg ? arrowImg.color : Color.white;
            if (arrowImg) arrowImg.color = new Color(0.4f, 0.2f, 0.6f);

            if (arrow.name.Contains(correctAnswers[i]))
            {
                Vector2Int nextCell = path[i + 1];
                GameObject nextObj = GameObject.Find((nextCell.x + 1) + "_" + (nextCell.y + 1));
                if (nextObj != null)
                {
                    Vector3 targetPos = nextObj.transform.position;
                    targetPos.y += yOffset;
                    float elapsed = 0f;
                    Vector3 startPos = player.transform.position;
                    while (elapsed < 0.4f)
                    {
                        player.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / 0.4f);
                        elapsed += Time.deltaTime;
                        yield return null;
                    }
                    player.transform.position = targetPos;
                }
                if (arrowImg) arrowImg.color = origColor;
                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                if (arrowImg) arrowImg.color = Color.red;
                ShowMessage("¦A¸Õ¤@¦¸§a¡I", Color.white);
                yield return new WaitForSeconds(1.5f);
                failHintText?.gameObject.SetActive(false);
                if (arrowImg) arrowImg.color = origColor;
                ResetPlayer();
                isMoving = false;
                yield break;
            }
        }

        // ³qÃצ¡I
        timerRunning = false;
        ShowEndScreen();
        isMoving = false;
    }

    // שששששששששששששששששששששששששששששש µ²÷ג שששששששששששששששששששששששששששששששששששששששששששששששששששששששש

    void ShowEndScreen()
    {
        bool isNewRecord = hasPlayedBefore && elapsedTime < savedBestTime;

        int coinEarned = coinComplete;
        if (isNewRecord) coinEarned += coinRecord;

        float newBestTime = (!hasPlayedBefore || elapsedTime < savedBestTime)
                            ? elapsedTime : savedBestTime;

        SaveData(savedCoins + coinEarned, newBestTime);
        endScreen?.SetActive(true);

        if (titleText) titleText.text = "®¥³‗³qÃצ¡I";

        if (finalTimeText)
        {
            int min = (int)(elapsedTime / 60f);
            int sec = (int)(elapsedTime % 60f);
            finalTimeText.text = string.Format("{0:00}:{1:00}", min, sec);
        }

        if (coinRewardText)
        {
            string msg = "×ק¹פ +" + coinComplete;
            if (isNewRecord) msg += "\n¯}¬צ¿‎¡I+" + coinRecord;
            coinRewardText.text = msg;
        }
    }

    // שששששששששששששששששששששששששששששש ¤u¨ד שששששששששששששששששששששששששששששששששששששששששששששששששששששששש

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
        if (player != null) player.transform.position = playerStartWorldPos;
    }
}