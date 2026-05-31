using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Level4Manager : MonoBehaviour
{
    [Header("UI 元件")]
    public TextMeshProUGUI dialogueText;
    public Image backgroundImage;

    [Header("第一段劇情（挑木頭前）")]
    [TextArea(10, 20)]
    public string storyPart1;

    [Header("第二段劇情（挑木頭後）")]
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

    [Header("第二段劇情結束後銜接")]
    public GameObject nextGameUI;

    private bool woodCompleted = false;

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

        Debug.Log("【劇情控制】第一段結束，等待挑木頭完成...");
    }

    public void OnWoodComplete()
    {
        if (woodCompleted) return;
        woodCompleted = true;

        if (miniGameCanvas != null) miniGameCanvas.SetActive(false);
        if (miniGameBackground != null) miniGameBackground.SetActive(false);

        // 換成第二張背景
        if (backgroundImage != null && scene2 != null)
            backgroundImage.sprite = scene2;

        Debug.Log("【劇情控制】挑木頭完成，開始第二段劇情");
        StartCoroutine(PlayPart2());
    }

    IEnumerator PlayPart2()
    {
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

        if (!woodCompleted)
        {
            if (miniGameCanvas != null) miniGameCanvas.SetActive(true);
            if (miniGameBackground != null) miniGameBackground.SetActive(true);
        }
        else
        {
            if (nextGameUI != null) nextGameUI.SetActive(true);
        }
    }
}