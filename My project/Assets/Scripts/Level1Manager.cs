using UnityEngine;
using TMPro; // 使用 TextMeshPro 必備
using System.Collections;

public class Level1Manager : MonoBehaviour
{
    [Header("UI 元件")]
    public TextMeshProUGUI dialogueText; // 拖入你的 TextMeshPro 物件

    [Header("劇情設定")]
    [TextArea(5, 10)]
    public string storyContent; // 在 Inspector 貼上你的劇情
    public float typingSpeed = 0.05f; // 每個字的打字速度
    public float waitBeforeStory = 12.0f; // 等待睜眼動畫播完的時間
    public float timeBetweenLines = 1.5f; // 每一行顯示完後的停頓時間

    void Start()
    {
        // 啟動主流程
        if (dialogueText != null)
        {
            StartCoroutine(PlayLevel1Flow());
        }
        else
        {
            Debug.LogError("寶貝，你忘記把 DialogueText 拖進 GameManager 的格子裡了！"); // 避免 NullReferenceException
        }
    }

    IEnumerator PlayLevel1Flow()
    {
        // 1. 一開始先清空文字
        dialogueText.text = "";

        // 2. 等待睜眼動畫播完
        yield return new WaitForSeconds(waitBeforeStory);

        // 3. 處理劇情：按「換行」拆分文字
        string[] lines = storyContent.Split('\n');

        foreach (string line in lines)
        {
            dialogueText.text = ""; // 每一行開始前清空舊文字

            // 逐字打出當前這一行
            foreach (char letter in line.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }

            // 這行打完了，停一下讓玩家看清楚
            yield return new WaitForSeconds(timeBetweenLines);
        }
    }
}