using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ForGameManager : MonoBehaviour
{
    [Header("UI 連結")]
    public GameObject printGroupPanel;
    public GameObject resultPanel;
    public GameObject computerPanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI wrongText;

    [Header("放置槽（依順序拉入 kuang1~15）")]
    public DropSlot[] slots;

    [Header("結果展示方塊")]
    public Image[] blockImages;
    public Color highlightColor = Color.yellow;
    public Color wrongColor = Color.red;
    public float highlightDuration = 0.5f;

    [Header("完成後換圖")]
    public Image backgroundForImage;  // 拉入 Background (for) 的 Image
    public Sprite backgroundSprite2;  // 換成的新圖
    public GameObject boatGogo;       // 拉入 Boat gogo 物件

    [System.Serializable]
    public class BlockSpriteGroup
    {
        public Sprite class_, main, for_, int_, int_name, equals, one,
                      int_count, lessEqual, ten, plusplus, print,
                      string_marks, paddle_hard;
    }

    [Header("Block 圖片分組")]
    public BlockSpriteGroup groupA; // 框1
    public BlockSpriteGroup groupB; // 框2
    public BlockSpriteGroup groupC; // 框3
    public BlockSpriteGroup groupD; // 框4~12
    public BlockSpriteGroup groupE; // 框13
    public BlockSpriteGroup groupF; // 框14~15

    private BlockSpriteGroup GetGroupByIndex(int i)
    {
        switch (i)
        {
            case 0: return groupA;
            case 1: return groupB;
            case 2: return groupC;
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11: return groupD;
            case 12: return groupE;
            case 13: case 14: return groupF;
            default: return null;
        }
    }

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
        BlockSpriteGroup b = GetGroupByIndex(blockIndex);
        if (b == null) return null;
        switch (blockID)
        {
            case "class": return b.class_;
            case "main": return b.main;
            case "for": return b.for_;
            case "int": return b.int_;
            case "int name": return b.int_name;
            case "=": return b.equals;
            case "1": return b.one;
            case "int count": return b.int_count;
            case "<=": return b.lessEqual;
            case "10": return b.ten;
            case "++": return b.plusplus;
            case "print": return b.print;
            case "string marks": return b.string_marks;
            case "paddle hard": return b.paddle_hard;
            default: return null;
        }
    }

    IEnumerator ExecuteBlocks(DropSlot[] slots)
    {
        if (printGroupPanel != null) printGroupPanel.SetActive(false);
        if (computerPanel != null) computerPanel.SetActive(true);

        for (int i = 0; i < blockImages.Length && i < slots.Length; i++)
        {
            if (blockImages[i] == null) continue;
            blockImages[i].gameObject.SetActive(true);
            string placedID = slots[i].GetCurrentBlockID();
            blockImages[i].sprite = GetBlockSprite(i, placedID);
        }

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
        }

        // 全部答對：隱藏 block
        foreach (Image img in blockImages)
            if (img != null) img.gameObject.SetActive(false);

        // 隱藏 Boat gogo，換背景圖
        if (boatGogo != null) boatGogo.SetActive(false);
        if (backgroundForImage != null && backgroundSprite2 != null)
            backgroundForImage.sprite = backgroundSprite2;

        yield return new WaitForSeconds(3f);
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

    void ShowResult(float elapsed)
    {
        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultText != null)
        {
            int minutes = (int)(elapsed / 60);
            int seconds = (int)(elapsed % 60);
            resultText.text = $"恭喜通關\n耗時：{minutes:00}:{seconds:00}";
        }
    }
}