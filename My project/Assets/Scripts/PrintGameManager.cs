using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PrintGameManager : MonoBehaviour
{
    [Header("UI 連結")]
    public GameObject printGroupPanel;
    public GameObject resultPanel;
    public GameObject computerPanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI wrongText;

    [Header("電腦螢幕")]
    public GameObject outputLine;
    public TextMeshProUGUI outputText;
    public string outputWord = "hum-001";
    public float typingSpeed = 0.08f;

    [Header("放置槽（依順序拉入 kuang1~5）")]
    public DropSlot[] slots;

    [Header("結果展示方塊")]
    public Image[] blockImages;
    public Color highlightColor = Color.yellow;
    public Color wrongColor = Color.red;
    public float highlightDuration = 0.5f;

    [Header("Block 1 圖片")]
    public Sprite block1_class, block1_main, block1_print, block1_string, block1_num;

    [Header("Block 2 圖片")]
    public Sprite block2_class, block2_main, block2_print, block2_string, block2_num;

    [Header("Block 3 圖片")]
    public Sprite block3_class, block3_main, block3_print, block3_string, block3_num;

    [Header("Block 4 圖片")]
    public Sprite block4_class, block4_main, block4_print, block4_string, block4_num;

    [Header("Block 5 圖片")]
    public Sprite block5_class, block5_main, block5_print, block5_string, block5_num;

    private float startTime;

    public void StartGame()
    {
        startTime = Time.time;
    }

    public void CheckAnswer()
    {
        StartCoroutine(ExecuteBlocks(slots));
    }

    Sprite GetBlockSprite(int blockIndex, string blockID)
    {
        Sprite[] sprites;

        switch (blockIndex)
        {
            case 0: sprites = new Sprite[] { block1_class, block1_main, block1_print, block1_string, block1_num }; break;
            case 1: sprites = new Sprite[] { block2_class, block2_main, block2_print, block2_string, block2_num }; break;
            case 2: sprites = new Sprite[] { block3_class, block3_main, block3_print, block3_string, block3_num }; break;
            case 3: sprites = new Sprite[] { block4_class, block4_main, block4_print, block4_string, block4_num }; break;
            case 4: sprites = new Sprite[] { block5_class, block5_main, block5_print, block5_string, block5_num }; break;
            default: return null;
        }

        switch (blockID)
        {
            case "class": return sprites[0];
            case "main": return sprites[1];
            case "print": return sprites[2];
            case "string": return sprites[3];
            case "num": return sprites[4];
            default: return null;
        }
    }

    IEnumerator ExecuteBlocks(DropSlot[] slots)
    {
        if (printGroupPanel != null) printGroupPanel.SetActive(false);
        if (computerPanel != null) computerPanel.SetActive(true);

        if (outputText != null) outputText.text = "";
        if (outputLine != null) outputLine.SetActive(false);

        // 設定 block1~5 圖片
        for (int i = 0; i < blockImages.Length && i < slots.Length; i++)
        {
            if (blockImages[i] == null) continue;
            blockImages[i].gameObject.SetActive(true);
            string placedID = slots[i].GetCurrentBlockID();
            blockImages[i].sprite = GetBlockSprite(i, placedID);
        }

        // 逐框發光並判斷
        for (int i = 0; i < blockImages.Length && i < slots.Length; i++)
        {
            if (blockImages[i] == null) continue;

            Color original = blockImages[i].color;
            blockImages[i].color = highlightColor;
            yield return new WaitForSeconds(highlightDuration);

            if (!slots[i].isCorrect)
            {
                blockImages[i].color = wrongColor;
                yield return new WaitForSeconds(0.5f);
                blockImages[i].color = original;

                yield return StartCoroutine(ShowWrongHint(slots));
                yield break;
            }

            blockImages[i].color = original;

            if (slots[i].acceptID == "num")
            {
                if (outputLine != null) outputLine.SetActive(true);                
                yield return StartCoroutine(TypeText(outputWord));
            }
        }

        yield return new WaitForSeconds(0.5f);
        ShowResult(Time.time - startTime);
    }

    IEnumerator ShowWrongHint(DropSlot[] slots)
    {
        foreach (Image img in blockImages)
            if (img != null) img.gameObject.SetActive(false);

        foreach (DropSlot slot in slots)
            slot.ResetSlot();

        if (computerPanel != null) computerPanel.SetActive(false);
        if (printGroupPanel != null) printGroupPanel.SetActive(true);

        if (wrongText != null)
        {
            wrongText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            wrongText.gameObject.SetActive(false);
        }
    }

    IEnumerator TypeText(string text)
    {
        if (outputText == null) yield break;
        outputText.text = "";
        foreach (char c in text)
        {
            outputText.text += c;
            yield return new WaitForSeconds(typingSpeed);
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