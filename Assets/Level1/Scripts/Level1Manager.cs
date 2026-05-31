using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class Level1Manager : MonoBehaviour
{
    [Header("UI 元件")]
    public TextMeshProUGUI dialogueText;
    public Image backgroundImage;

    [Header("劇情設定")]
    [TextArea(10, 20)]
    public string storyContent;
    public float typingSpeed = 0.05f;
    public float waitBeforeStory = 0.5f;
    public float timeBetweenLines = 1.5f;

    [Header("背景圖片庫")]
    public Sprite scene1;
    public Sprite scene2;

    [Header("小遊戲銜接")]
    public GameObject miniGameCanvas;
    public GameObject miniGameBackground;
    public PrintGameManager printGameManager;

    [Header("Quest Record")]
    public string levelID = "Level1";
    public int currentProgress = 0;

    private bool miniGameStarted = false;

    void Start()
    {
        currentProgress = 0;
        miniGameStarted = false;

        if (backgroundImage != null && scene1 != null)
            backgroundImage.sprite = scene1;

        if (dialogueText != null)
            dialogueText.color = Color.black;

        if (miniGameCanvas != null)
            miniGameCanvas.SetActive(false);

        if (miniGameBackground != null)
            miniGameBackground.SetActive(false);

        if (dialogueText != null)
        {
            dialogueText.text = "";
            StartCoroutine(PlayLevel1Flow());
        }
    }

    IEnumerator PlayLevel1Flow()
    {
        yield return new WaitForSeconds(waitBeforeStory);

        string content = storyContent.Replace("\r", "");
        string[] lines = content.Split('\n');

        foreach (string line in lines)
        {
            string currentLine = line.Trim();

            if (string.IsNullOrEmpty(currentLine))
                continue;

            if (currentLine.Contains("[換圖]"))
            {
                if (backgroundImage != null && scene2 != null)
                {
                    backgroundImage.sprite = scene2;

                    if (dialogueText != null)
                        dialogueText.color = Color.black;
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

        OpenMiniGame();
    }

    public void SkipStory()
    {
        StopAllCoroutines();

        if (dialogueText != null)
            dialogueText.text = "";

        OpenMiniGame();
    }

    private void OpenMiniGame()
    {
        if (miniGameStarted) return;
        miniGameStarted = true;

        // 進入第一段小遊戲時，不存 0%
        currentProgress = 0;

        if (dialogueText != null)
            dialogueText.text = "";

        if (miniGameCanvas != null)
            miniGameCanvas.SetActive(true);

        if (miniGameBackground != null)
            miniGameBackground.SetActive(true);

        if (printGameManager != null)
        {
            Debug.Log("【第一關】第一段小遊戲開始，正式開始計時");
            printGameManager.StartGame();
        }
        else
        {
            Debug.LogWarning("【第一關】printGameManager 沒有拖入");
        }
    }

    // PrintGameManager 第一段正確完成後呼叫這個
    public void OnFirstMiniGameComplete(float elapsedTime)
    {
        currentProgress = 50;
        firstPartTime = elapsedTime;

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveQuestProgress(levelID, currentProgress, elapsedTime, 0);
            Debug.Log("【第一關】第一段完成，儲存 50%，目前累積時間：" + elapsedTime + " 秒");
        }
        else
        {
            Debug.LogWarning("【第一關】FirestoreManager.Instance 不存在，無法儲存 50%");
        }

        if (miniGameCanvas != null)
            miniGameCanvas.SetActive(false);

        if (miniGameBackground != null)
            miniGameBackground.SetActive(false);

        StartStory();
    }

    public void SetProgress(int progress)
    {
        currentProgress = Mathf.Clamp(progress, 0, 100);

        if (currentProgress <= 0)
        {
            Debug.Log("【第一關】目前進度 0%，不儲存");
            return;
        }

        SaveProgress();
    }

    public void SaveProgress()
    {
        if (currentProgress <= 0)
        {
            Debug.Log("【第一關】目前進度 0%，不儲存");
            return;
        }

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveQuestProgress(levelID, currentProgress);
            Debug.Log("【第一關】儲存闖關進度：" + currentProgress + "%");
        }
    }

    private float firstPartTime = 0f;

    public void SetFirstPartTime(float time)
    {
        firstPartTime = time;
    }

    public float GetFirstPartTime()
    {
        return firstPartTime;
    }


    public void QuitLevel()
    {
        SaveProgress();

        PlayerPrefs.SetString("ReturnPanel", "Panel_Clue");
        SceneManager.LoadScene("SampleScene");
    }

    public void StartStory()
    {
        Debug.Log("【劇情控制】收到第一段小遊戲通關訊號，準備下一段...");

        // 之後你第二段劇情 / 第四張圖入口要寫在這裡
        // 例如：
        // backgroundImage.sprite = scene3;
        // dialogueText.text = "...";
        // 或開啟電腦互動 Panel
    }
}