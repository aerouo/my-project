using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Level3Manager : MonoBehaviour
{
    [Header("UI 元件")]
    public TextMeshProUGUI dialogueText;
    public Image backgroundImage;

    [Header("第一段劇情（吃吃吃前）")]
    [TextArea(10, 20)]
    public string storyPart1;

    [Header("第二段劇情（吃吃吃後）")]
    [TextArea(10, 20)]
    public string storyPart2;

    [Header("劇情設定")]
    public float typingSpeed = 0.05f;
    public float waitBeforeStory = 0.5f;
    public float timeBetweenLines = 1.5f;

    [Header("背景圖片庫")]
    public Sprite scene1;
    public Sprite scene2;

    [Header("小遊戲銜接")]
    public GameObject miniGameCanvas;
    public GameObject miniGameBackground;
    public EatingGame eatingGame;

    [Header("第二段劇情結束後銜接")]
    public GameObject nextGameUI;

    [Header("闖關紀錄")]
    public string questLevelID = "Level3";

    private bool eatingCompleted = false;
    private int currentScene = 1;

    private float elapsedTime = 0f;
    private float timerStartTime = 0f;
    private bool isTimerRunning = false;

    void Start()
    {
        if (backgroundImage != null && scene1 != null)
            backgroundImage.sprite = scene1;

        if (dialogueText != null)
            dialogueText.color = Color.black;

        if (miniGameCanvas != null) miniGameCanvas.SetActive(false);
        if (miniGameBackground != null) miniGameBackground.SetActive(false);
        if (nextGameUI != null) nextGameUI.SetActive(false);

        if (dialogueText != null)
        {
            dialogueText.text = "";
            StartCoroutine(PlayPart1());
        }
    }

    public void StartQuestTimer()
    {
        if (isTimerRunning)
            return;

        timerStartTime = Time.time;
        isTimerRunning = true;

        Debug.Log("【Level3 計時】開始 / 繼續計時");
    }

    public void PauseQuestTimer()
    {
        if (!isTimerRunning)
            return;

        elapsedTime += Time.time - timerStartTime;
        isTimerRunning = false;

        Debug.Log("【Level3 計時】暫停，目前累積：" + elapsedTime);
    }

    public float GetElapsedTime()
    {
        if (isTimerRunning)
            return elapsedTime + (Time.time - timerStartTime);

        return elapsedTime;
    }

    IEnumerator PlayPart1()
    {
        yield return new WaitForSeconds(waitBeforeStory);
        yield return StartCoroutine(PlayStory(storyPart1));

        dialogueText.text = "";

        if (miniGameCanvas != null) miniGameCanvas.SetActive(true);
        if (miniGameBackground != null) miniGameBackground.SetActive(true);

        StartQuestTimer();

        if (eatingGame != null)
            eatingGame.StartGame();

        Debug.Log("【劇情控制】第一段結束，開始吃東西小遊戲並計時");
    }

    public void OnEatingComplete()
    {
        if (eatingCompleted) return;
        eatingCompleted = true;

        PauseQuestTimer();

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveQuestProgress(
                questLevelID,
                50,
                GetElapsedTime()
            );
        }

        if (miniGameCanvas != null) miniGameCanvas.SetActive(false);
        if (miniGameBackground != null) miniGameBackground.SetActive(false);

        Debug.Log("【Level3】吃東西完成，存 50%");
        StartCoroutine(PlayPart2());
    }

    IEnumerator PlayPart2()
    {
        if (dialogueText != null)
            dialogueText.text = "";

        yield return StartCoroutine(PlayStory(storyPart2));

        dialogueText.text = "";

        if (nextGameUI != null)
            nextGameUI.SetActive(true);

        Debug.Log("【劇情控制】第二段結束，開啟 good / bad 選擇畫面，不計時");
    }

    IEnumerator PlayStory(string content)
    {
        if (string.IsNullOrEmpty(content)) yield break;

        content = content.Replace("\r", "");
        string[] lines = content.Split('\n');

        foreach (string line in lines)
        {
            string currentLine = line.Trim();
            if (string.IsNullOrEmpty(currentLine)) continue;

            if (currentLine.Contains("[換圖]"))
            {
                currentScene++;
                Sprite nextSprite = scene2;

                if (backgroundImage != null && nextSprite != null)
                {
                    backgroundImage.sprite = nextSprite;
                    Debug.Log("【劇情控制】背景切換到 scene" + currentScene);
                }

                continue;
            }

            if (dialogueText != null)
                dialogueText.text = "";

            foreach (char letter in currentLine.ToCharArray())
            {
                if (dialogueText != null)
                    dialogueText.text += letter;

                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(timeBetweenLines);
        }
    }

    public void SkipStory()
    {
        StopAllCoroutines();

        if (dialogueText != null)
            dialogueText.text = "";

        if (!eatingCompleted)
        {
            if (miniGameCanvas != null) miniGameCanvas.SetActive(true);
            if (miniGameBackground != null) miniGameBackground.SetActive(true);

            StartQuestTimer();

            if (eatingGame != null)
                eatingGame.StartGame();
        }
        else
        {
            if (nextGameUI != null)
                nextGameUI.SetActive(true);
        }
    }
}