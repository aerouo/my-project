using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Level2Manager : MonoBehaviour
{
    [Header("UI 元件")]
    public TextMeshProUGUI dialogueText;
    public Image backgroundImage;

    [Header("第一段劇情（拼圖前）")]
    [TextArea(10, 20)]
    public string storyPart1;

    [Header("第二段劇情（拼圖後）")]
    [TextArea(10, 20)]
    public string storyPart2;

    [Header("劇情設定")]
    public float typingSpeed = 0.05f;
    public float waitBeforeStory = 0.5f;
    public float timeBetweenLines = 1.5f;

    [Header("背景圖片庫")]
    public Sprite scene1;
    public Sprite scene2;
    public Sprite scene3;

    [Header("小遊戲銜接")]
    public GameObject miniGameCanvas;
    public GameObject miniGameBackground;

    [Header("第二段劇情結束後銜接")]
    public GameObject nextGameUI;  // 程式方塊拖曳的 UI，拼圖後劇情結束開啟

    private bool puzzleCompleted = false;
    private int currentScene = 1; // 目前背景編號

    void Start()
    {
        if (backgroundImage != null && scene1 != null) backgroundImage.sprite = scene1;
        if (dialogueText != null) dialogueText.color = Color.white;

        if (miniGameCanvas != null) miniGameCanvas.SetActive(false);
        if (miniGameBackground != null) miniGameBackground.SetActive(false);
        if (nextGameUI != null) nextGameUI.SetActive(false);

        if (dialogueText != null)
        {
            dialogueText.text = "";
            StartCoroutine(PlayPart1());
        }
    }

    // ═══════════════ 第一段劇情 ══════════════════════

    IEnumerator PlayPart1()
    {
        yield return new WaitForSeconds(waitBeforeStory);
        yield return StartCoroutine(PlayStory(storyPart1));

        // 第一段跑完，顯示拼圖
        dialogueText.text = "";
        if (miniGameCanvas != null) miniGameCanvas.SetActive(true);
        if (miniGameBackground != null) miniGameBackground.SetActive(true);

        Debug.Log("【劇情控制】第一段結束，等待拼圖完成...");
    }

    // ═══════════════ 拼圖完成通知 ════════════════════
    // 由 Puzzle_Lvl2 的 CompleteGame() 呼叫

    public void OnPuzzleComplete()
    {
        if (puzzleCompleted) return;
        puzzleCompleted = true;

        // 隱藏拼圖
        if (miniGameCanvas != null) miniGameCanvas.SetActive(false);
        if (miniGameBackground != null) miniGameBackground.SetActive(false);

        Debug.Log("【劇情控制】拼圖完成，開始第二段劇情");
        StartCoroutine(PlayPart2());
    }

    // ═══════════════ 第二段劇情 ══════════════════════

    IEnumerator PlayPart2()
    {
        if (dialogueText != null) dialogueText.text = "";
        yield return StartCoroutine(PlayStory(storyPart2));

        // 第二段跑完，銜接程式方塊
        dialogueText.text = "";
        if (nextGameUI != null) nextGameUI.SetActive(true);
        Debug.Log("【劇情控制】第二段結束，開啟程式方塊");
    }

    // ═══════════════ 通用劇情播放 ════════════════════

    IEnumerator PlayStory(string content)
    {
        if (string.IsNullOrEmpty(content)) yield break;

        content = content.Replace("\r", "");
        string[] lines = content.Split('\n');

        foreach (string line in lines)
        {
            string currentLine = line.Trim();
            if (string.IsNullOrEmpty(currentLine)) continue;

            // 換圖指令
            if (currentLine.Contains("[換圖]"))
            {
                currentScene++;
                Sprite nextSprite = currentScene == 2 ? scene2 : scene3;
                if (backgroundImage != null && nextSprite != null)
                {
                    backgroundImage.sprite = nextSprite;
                    if (dialogueText != null) dialogueText.color = Color.black;
                    Debug.Log("【劇情控制】背景切換到 scene" + currentScene);
                }
                continue;
            }

            // 打字機效果
            if (dialogueText != null) dialogueText.text = "";
            foreach (char letter in currentLine.ToCharArray())
            {
                if (dialogueText != null) dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
            yield return new WaitForSeconds(timeBetweenLines);
        }
    }

    // ═══════════════ 跳過 ════════════════════════════

    public void SkipStory()
    {
        StopAllCoroutines();
        if (dialogueText != null) dialogueText.text = "";

        if (!puzzleCompleted)
        {
            // 跳過第一段，直接顯示拼圖
            if (miniGameCanvas != null) miniGameCanvas.SetActive(true);
            if (miniGameBackground != null) miniGameBackground.SetActive(true);
        }
        else
        {
            // 跳過第二段，直接銜接下一關
            if (nextGameUI != null) nextGameUI.SetActive(true);
        }
    }
}