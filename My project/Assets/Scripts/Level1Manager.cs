using UnityEngine;
using TMPro; // 使用 TextMeshPro 必備
using System.Collections;

public class Level1Manager : MonoBehaviour
{
    [Header("UI 元件")]
    public TextMeshProUGUI dialogueText; // 拖入你的文字框

    [Header("劇情設定")]
    [TextArea(5, 10)]
    public string storyContent; // 貼上你的長篇劇情
    public float typingSpeed = 0.05f; // 打字速度（秒/字）
    public float waitBeforeStory = 12.0f; // 等待睜眼動畫播完的時間

    void Start()
    {
        // 遊戲一開始，就啟動「等待並顯示劇情」的排程
        StartCoroutine(PlayLevel1Flow());
    }

    IEnumerator PlayLevel1Flow()
    {
        // 1. 先清空文字框，讓畫面乾淨
        dialogueText.text = "";

        // 2. 等待睜眼動畫播完（例如動畫是3秒，這裡就填3）
        yield return new WaitForSeconds(waitBeforeStory);

        // 3. 開始打字機效果
        foreach (char letter in storyContent.ToCharArray())
        {
            dialogueText.text += letter; // 逐字加上去
            yield return new WaitForSeconds(typingSpeed); // 停頓一下下
        }
    }
}