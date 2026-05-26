using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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

    private bool miniGameStarted = false;

    void Start()
    {
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

        if (dialogueText != null)
            dialogueText.text = "";

        if (miniGameCanvas != null)
            miniGameCanvas.SetActive(true);

        if (miniGameBackground != null)
            miniGameBackground.SetActive(true);

        if (printGameManager != null)
        {
            Debug.Log("【第一關】小遊戲開始，正式開始計時");
            printGameManager.StartGame();
        }
        else
        {
            Debug.LogWarning("【第一關】printGameManager 沒有拖入");
        }
    }
    public void StartStory()
    {
        Debug.Log("【劇情控制】收到小遊戲通關訊號，準備下一段...");
    }
}