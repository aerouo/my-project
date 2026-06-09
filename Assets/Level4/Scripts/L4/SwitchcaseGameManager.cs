using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SwitchcaseGameManager : MonoBehaviour
{
    [Header("Level4 Manager")]
    public Level4Manager level4Manager;

    [Header("UI 連結")]
    public GameObject SwitchcaseGroupPanel;
    public GameObject resultPanel;
    public GameObject draftPanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI wrongText;

    [Header("放置槽（依順序拉入 kuang1~27）")]
    public DropSlot[] slots;

    [Header("結果展示方塊")]
    public Image[] blockImages;
    public Color highlightColor = Color.yellow;
    public Color wrongColor = Color.red;
    public float highlightDuration = 0.5f;

    [Header("Draft 換圖")]
    public Image draftImage;        // 拉入 draft 的 Image
    public Sprite draftSprite2;     // 答對後換成的圖

    [System.Serializable]
    public class BlockSpriteGroup
    {
        public Sprite class_, main, string_, string_name, equals, string_marks, string_mark,
                      minus_x, minus, x, char_, char_name, blueprint, switch_, case_,
                      char_symbol, print, place_wood, break_, binding_rope;
    }

    [Header("Block 圖片分組")]
    public BlockSpriteGroup groupA; // 框1
    public BlockSpriteGroup groupB; // 框2
    public BlockSpriteGroup groupC; // 框3.8.17.24
    public BlockSpriteGroup groupD; // 框4.9.13.15.16.19.20.22.23.26.27
    public BlockSpriteGroup groupE; // 框5.10
    public BlockSpriteGroup groupF; // 框6.7.11.18.25
    public BlockSpriteGroup groupG; // 框12
    public BlockSpriteGroup groupH; // 框14.21

    private BlockSpriteGroup GetGroupByIndex(int i)
    {
        switch (i)
        {
            case 0: return groupA; // 框1
            case 1: return groupB; // 框2
            case 2: case 7: case 16: case 23: return groupC; // 框3.8.17.24
            case 3:
            case 8:
            case 12:
            case 14:
            case 15:
            case 18:
            case 19:
            case 21:
            case 22:
            case 25:
            case 26: return groupD; // 框4.9.13.15.16.19.20.22.23.26.27
            case 4: case 9: return groupE; // 框5.10
            case 5: case 6: case 10: case 17: case 24: return groupF; // 框6.7.11.18.25
            case 11: return groupG; // 框12
            case 13: case 20: return groupH; // 框14.21
            default: return null;
        }
    }

    private float startTime;

    void Start()
    {
    }

    public void StartGame()
    {
        startTime = Time.time;

        if (level4Manager != null)
            level4Manager.OnSwitchcaseStart();
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
            case "string": return b.string_;
            case "string name": return b.string_name;
            case "=": return b.equals;
            case "string marks": return b.string_marks;
            case "string mark": return b.string_mark;
            case "-x": return b.minus_x;
            case "-": return b.minus;
            case "x": return b.x;
            case "char": return b.char_;
            case "char name": return b.char_name;
            case "blueprint": return b.blueprint;
            case "switch": return b.switch_;
            case "case": return b.case_;
            case "char symbol": return b.char_symbol;
            case "print": return b.print;
            case "place wood": return b.place_wood;
            case "break": return b.break_;
            case "binding rope": return b.binding_rope;
            default: return null;
        }
    }

    IEnumerator ExecuteBlocks(DropSlot[] slots)
    {
        if (SwitchcaseGroupPanel != null) SwitchcaseGroupPanel.SetActive(false);
        if (draftPanel != null) draftPanel.SetActive(true);

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

        // 換 draft 圖片
        if (draftImage != null && draftSprite2 != null)
            draftImage.sprite = draftSprite2;

        yield return new WaitForSeconds(0.5f);
        ShowResult(Time.time - startTime);
    }

    IEnumerator ShowWrongHint(DropSlot[] slots)
    {
        foreach (Image img in blockImages)
            if (img != null) img.gameObject.SetActive(false);

        foreach (DropSlot slot in slots)
            slot.ResetSlot();

        if (draftPanel != null) draftPanel.SetActive(false);
        if (SwitchcaseGroupPanel != null) SwitchcaseGroupPanel.SetActive(true);

        if (wrongText != null)
        {
            wrongText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            wrongText.gameObject.SetActive(false);
        }
    }

    void ShowResult(float elapsed)
    {
        if (level4Manager != null)
            level4Manager.OnSwitchcaseComplete();

        float totalElapsed = level4Manager != null ? level4Manager.GetElapsedTime() : elapsed;

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
        {
            int minutes = (int)(totalElapsed / 60);
            int seconds = (int)(totalElapsed % 60);
            resultText.text = $"恭喜通關\n耗時：{minutes:00}:{seconds:00}";
        }
    }
}