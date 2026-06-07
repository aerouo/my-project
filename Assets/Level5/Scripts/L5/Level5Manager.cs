using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Level5Manager : MonoBehaviour
{
    [Header("UI 元件")]
    public TextMeshProUGUI dialogueText;
    public Image backgroundImage;

    [Header("第一段劇情（海盜船前）")]
    [TextArea(10, 20)]
    public string storyPart1;

    [Header("第二段劇情（海盜船後）")]
    [TextArea(10, 20)]
    public string storyPart2;

    [Header("劇情設定")]
    public float typingSpeed = 0.05f;
    public float waitBeforeStory = 0.5f;
    public float timeBetweenLines = 1.5f;

    [Header("背景圖片庫")]
    public Sprite scene1;
    public Sprite scene2;
    public Sprite scene3; // 第二段劇情換圖用

    [Header("小遊戲銜接")]
    public GameObject miniGameCanvas;
    public GameObject miniGameBackground;
    public ChoiceBoatGame choiceBoatGame;

    [Header("第二段劇情結束後銜接")]
    public GameObject nextGameUI;

    private bool boatCompleted = false;
    private int currentScene = 1;
    private bool inPart2 = false;

    void Start()
    {
        if (backgroundImage != null && scene1 != null) backgroundImage.sprite = scene1;
        if (miniGameCanvas != null) miniGameCanvas.SetActive(false);
        if (miniGameBackground != null) miniGameBackground.SetActive(false);
        if (nextGameUI != null) nextGameUI.SetActive(false);

        if (dialogueText != null)
        {
            dialogueText.color = Color.black;
            dialogueText.text = "";
            StartCoroutine(PlayPart1());
        }
    }

    IEnumerator PlayPart1()
    {
        yield return new WaitForSeconds(waitBeforeStory);
        yield return StartCoroutine(PlayStory(storyPart1));

        dialogueText.text = "";
        if (miniGameCanvas != null) miniGameCanvas.SetActive(true);
        if (miniGameBackground != null) miniGameBackground.SetActive(true);
        if (choiceBoatGame != null) choiceBoatGame.StartGame();

        Debug.Log("【劇情控制】第一段結束，等待海盜船完成...");
    }

    public void OnChoiceBoat()
    {
        if (boatCompleted) return;
        boatCompleted = true;

        if (miniGameCanvas != null) miniGameCanvas.SetActive(false);
        if (miniGameBackground != null) miniGameBackground.SetActive(false);

        if (backgroundImage != null && scene2 != null)
            backgroundImage.sprite = scene2;

        Debug.Log("【劇情控制】海盜船完成，開始第二段劇情");
        StartCoroutine(PlayPart2());
    }

    IEnumerator PlayPart2()
    {
        inPart2 = true;
        if (dialogueText != null) dialogueText.text = "";
        yield return StartCoroutine(PlayStory(storyPart2));

        dialogueText.text = "";
        if (nextGameUI != null) nextGameUI.SetActive(true);
        Debug.Log("【劇情控制】第二段結束，開啟程式方塊");
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
                Sprite next = inPart2 ? scene3 : scene2;
                if (backgroundImage != null && next != null)
                    backgroundImage.sprite = next;
                Debug.Log("【劇情控制】背景切換");
                continue;
            }

            if (dialogueText != null) dialogueText.text = "";
            foreach (char letter in currentLine.ToCharArray())
            {
                if (dialogueText != null) dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
            yield return new WaitForSeconds(timeBetweenLines);
        }
    }

    public void SkipStory()
    {
        StopAllCoroutines();
        if (dialogueText != null) dialogueText.text = "";

        if (!boatCompleted)
        {
            if (miniGameCanvas != null) miniGameCanvas.SetActive(true);
            if (miniGameBackground != null) miniGameBackground.SetActive(true);
            if (choiceBoatGame != null) choiceBoatGame.StartGame();
        }
        else
        {
            if (nextGameUI != null) nextGameUI.SetActive(true);
        }
    }
}