using UnityEngine;
using TMPro;
using UnityEngine.UI; // 為了控制背景圖片與顏色
using System.Collections;

public class Level1Manager : MonoBehaviour
{
    [Header("UI 元件")]
    public TextMeshProUGUI dialogueText;
    public Image backgroundImage; // 妳已經拖入 DialogueBackground 了

    [Header("劇情設定")]
    [TextArea(10, 20)]
    public string storyContent;
    public float typingSpeed = 0.05f;
    public float waitBeforeStory = 0.5f; // 睜眼動畫後的等待時間
    public float timeBetweenLines = 1.5f;

    [Header("背景圖片庫")]
    public Sprite scene1; // 妳已經拖入 劇情-廢棄實驗室
    public Sprite scene2; // 妳已經拖入 劇情-膠囊上的貼紙

    [Header("小遊戲銜接")]
    public GameObject miniGameCanvas; // 妳已經拖入 小遊戲-尋找線索
    public GameObject miniGameBackground; // 妳已經拖入 Background

    void Start()
    {
        // 1. 遊戲開始時，先確保第一張背景正確，文字為白色
        if (backgroundImage != null && scene1 != null) backgroundImage.sprite = scene1;
        if (dialogueText != null) dialogueText.color = Color.white;

        // 2. 隱藏小遊戲，等待劇情跑完
        if (miniGameCanvas != null) miniGameCanvas.SetActive(false);
        if (miniGameBackground != null) miniGameBackground.SetActive(false);

        if (dialogueText != null)
        {
            dialogueText.text = "";
            StartCoroutine(PlayLevel1Flow());
        }
    }

    IEnumerator PlayLevel1Flow()
    {
        // 睜眼前的小等待
        yield return new WaitForSeconds(waitBeforeStory);

        // 處理不同系統的換行符號問題
        string content = storyContent.Replace("\r", "");
        string[] lines = content.Split('\n');

        foreach (string line in lines)
        {
            string currentLine = line.Trim();

            // 跳過空行，這樣最後一行後面有按 Enter 也沒關係
            if (string.IsNullOrEmpty(currentLine)) continue;

            // --- 換圖與變色邏輯 ---
            if (currentLine.Contains("[換圖]"))
            {
                if (backgroundImage != null && scene2 != null)
                {
                    backgroundImage.sprite = scene2; // 換成貼紙圖
                    dialogueText.color = Color.black; // 文字變黑色
                    Debug.Log("【劇情控制】背景已切換，文字已變黑");
                }
                continue; // 指令行不顯示在畫面上
            }

            // 打字機流程
            dialogueText.text = "";
            foreach (char letter in currentLine.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
            yield return new WaitForSeconds(timeBetweenLines);
        }

        // --- 劇情全跑完了！切換到小遊戲 ---
        Debug.Log("【劇情控制】劇情結束，開啟小遊戲場景");
        dialogueText.text = ""; // 清空文字以免擋住小遊戲

        // 顯示小遊戲物件
        if (miniGameCanvas != null) miniGameCanvas.SetActive(true);
        if (miniGameBackground != null) miniGameBackground.SetActive(true);
    }

    // 小遊戲過關後呼叫的函數
    public void StartStory()
    {
        Debug.Log("【劇情控制】收到小遊戲通關訊號，準備下一段...");
    }
}