using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class StringGameManager : MonoBehaviour
{
    [System.Serializable]
    public struct BlockSpriteGroup
    {
        [Tooltip("積木的識別碼，例如: class, main, string marks, hum-001, = 等")]
        public string blockName;
        [Tooltip("依序放入該積木在 1~9 號槽位時各自對應的圖片外觀")]
        public Sprite[] stepSprites;
    }

    [Header("UI 連結面板")]
    public GameObject stringGroupPanel;
    public GameObject resultPanel;
    public GameObject puaperPanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI wrongText;

    [Header("背景圖片切換設定")]
    [Tooltip("要更換的背景 Image 元件（例如 Background 或 back map）")]
    public Image backgroundImage;
    [Tooltip("【第一次換圖】點擊檢查、開始跑亮燈時的背景圖")]
    public Sprite firstNewBackgroundSprite;

    [Header("Puaper 圖片切換設定")]
    [Tooltip("階層裡的 puaper Image 元件")]
    public Image puaperImage;
    [Tooltip("【最後換圖】通關成功時 puaper 要換上的新圖片")]
    public Sprite newPuaperSprite;

    [Header("便條紙結果圖")]
    public GameObject outputLine;

    [Header("放置槽（依順序拉入 kuang 1 ~ kuang 9）")]
    public DropSlot[] slots;

    [Header("結果展示方塊（長度請設為 9）")]
    public Image[] blockImages;
    public Color highlightColor = Color.yellow;
    public Color wrongColor = Color.red;
    public float highlightDuration = 0.5f;

    [Header("積木圖片資源庫（不限數量，動態對照）")]
    public List<BlockSpriteGroup> blockSpriteLibrary = new List<BlockSpriteGroup>();

    // 儲存遊戲最一開始、真正的原始圖片
    private Sprite originalBackgroundSprite;
    private Sprite originalPuaperSprite;
    private float startTime;
    private bool isExecuting = false;

    void Awake()
    {
        // 紀錄最原本的圖片，以便隨時換回來
        if (backgroundImage != null) originalBackgroundSprite = backgroundImage.sprite;
        if (puaperImage != null) originalPuaperSprite = puaperImage.sprite;
    }

    public void StartGame()
    {
        startTime = Time.time;
    }

    public void CheckAnswer()
    {
        if (isExecuting) return;
        StartCoroutine(ExecuteBlocks(slots));
    }

    Sprite GetBlockSprite(int blockIndex, string blockID)
    {
        BlockSpriteGroup group = blockSpriteLibrary.Find(g => g.blockName == blockID);

        if (group.stepSprites != null && blockIndex < group.stepSprites.Length)
        {
            return group.stepSprites[blockIndex];
        }

        Debug.LogWarning($"在資源庫中找不到積木 ID 為 '{blockID}' 且索引值為 {blockIndex} 的圖片！");
        return null;
    }

    IEnumerator ExecuteBlocks(DropSlot[] slots)
    {
        isExecuting = true;

        if (stringGroupPanel != null) stringGroupPanel.SetActive(false);
        if (puaperPanel != null) puaperPanel.SetActive(true);

        // 💥 【步驟 1】開始跑格子時，先換第一次背景圖 
        if (backgroundImage != null && firstNewBackgroundSprite != null)
            backgroundImage.sprite = firstNewBackgroundSprite;

        // 💥 【新增邏輯】開始跑格子時，先將 puaper 隱藏
        if (puaperImage != null)
            puaperImage.gameObject.SetActive(false);

        // 初始化並更換所有展示方塊的圖片
        for (int i = 0; i < blockImages.Length && i < slots.Length; i++)
        {
            if (blockImages[i] == null) continue;
            blockImages[i].gameObject.SetActive(true);

            string placedID = slots[i].GetCurrentBlockID();
            blockImages[i].sprite = GetBlockSprite(i, placedID);
        }

        // 逐框發光並判斷答案
        for (int i = 0; i < blockImages.Length && i < slots.Length; i++)
        {
            if (blockImages[i] == null) continue;

            Color original = blockImages[i].color;
            blockImages[i].color = highlightColor;
            yield return new WaitForSeconds(highlightDuration);

            // 檢查該槽位是否正確
            if (!slots[i].isCorrect)
            {
                blockImages[i].color = wrongColor;
                yield return new WaitForSeconds(0.5f);
                blockImages[i].color = original;

                yield return StartCoroutine(ShowWrongHint(slots));
                isExecuting = false;
                yield break;
            }

            blockImages[i].color = original;

            // 當檢查到最後一個對應 ID 為 hum-001 的槽位，代表「全對通關」了！
            if (slots[i].acceptID == "hum-001")
            {
                if (outputLine != null) outputLine.SetActive(true);

                // 💥 【步驟 2】先把背景圖「換回去」原本最初的背景
                if (backgroundImage != null)
                    backgroundImage.sprite = originalBackgroundSprite;

                // 稍微延遲一小段時間（0.2 秒），讓視覺有先後發生的層次感
                yield return new WaitForSeconds(0.2f);

                // 💥 【步驟 3】接著才顯示 puaper 並換成通關新圖片
                if (puaperImage != null && newPuaperSprite != null)
                {
                    puaperImage.sprite = newPuaperSprite;
                    puaperImage.gameObject.SetActive(true); // 重新顯示 puaper
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        yield return new WaitForSeconds(0.5f);
        ShowResult(Time.time - startTime);
        isExecuting = false;
    }

    IEnumerator ShowWrongHint(DropSlot[] slots)
    {
        foreach (Image img in blockImages)
            if (img != null) img.gameObject.SetActive(false);

        foreach (DropSlot slot in slots)
            slot.ResetSlot();

        // 答錯重置：確保背景和 puaper 都退回初始狀態，且 puaper 恢復顯示
        if (backgroundImage != null) backgroundImage.sprite = originalBackgroundSprite;
        if (puaperImage != null)
        {
            puaperImage.sprite = originalPuaperSprite;
            puaperImage.gameObject.SetActive(true); // 確保答錯時玩家還看得到原始 puaper
        }

        if (puaperPanel != null) puaperPanel.SetActive(false);
        if (stringGroupPanel != null) stringGroupPanel.SetActive(true);

        if (wrongText != null)
        {
            wrongText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            wrongText.gameObject.SetActive(false);
        }
    }

    void ShowResult(float elapsed)
    {
        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultText != null)
        {
            int minutes = (int)(elapsed / 60);
            int seconds = (int)(elapsed % 60);
            resultText.text = $"通關！\n耗時：{minutes:00}:{seconds:00}";
        }
    }
}