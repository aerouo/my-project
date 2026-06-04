using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class IfelseGameManager : MonoBehaviour
{
    [Header("UI 連結")]
    public GameObject printGroupPanel;
    public GameObject computerPanel;
    public TextMeshProUGUI wrongText;

    [Header("放置槽（依順序拉入 kuang1~17）")]
    public DropSlot[] slots;

    [Header("結果展示方塊")]
    public Image[] blockImages;
    public Color highlightColor = Color.yellow;
    public Color wrongColor = Color.red;
    public float highlightDuration = 0.5f;

    [Header("蘋果換圖")]
    public Image appleImage;        // 拉入 good 或 bad 蘋果的 Image
    public Sprite appleSprite2;     // 答對後換成的圖
    public UnityEngine.UI.Button appleButton; // 拉入蘋果的 Button，答對後停用

    [Header("結果管理")]
    public IfElseResultManager resultManager;  // 拉入 IfElseResultManager
    public AppleInteraction appleInteraction;  // 拉入 AppleInteraction
    public bool isGoodGroup;                   // good group 勾選，bad group 不勾

    [System.Serializable]
    public class BlockSpriteGroup
    {
        public Sprite class_, main, print, string_marks, equals, zero, one, equalsequals, int_, int_name, int_see, if_, else_, yes, no;
    }

    [Header("Block 圖片分組")]
    public BlockSpriteGroup groupA; // 框1、2
    public BlockSpriteGroup groupB; // 框3、11、15
    public BlockSpriteGroup groupC; // 框4、5、6、8、9、10
    public BlockSpriteGroup groupD; // 框7、14
    public BlockSpriteGroup groupE; // 框12、13、16、17

    private BlockSpriteGroup GetGroupByIndex(int i)
    {
        switch (i)
        {
            case 0: case 1: return groupA;
            case 2: case 10: case 14: return groupB;
            case 3: case 4: case 5: case 7: case 8: case 9: return groupC;
            case 6: case 13: return groupD;
            case 11: case 12: case 15: case 16: return groupE;
            default: return null;
        }
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
            case "print": return b.print;
            case "string marks": return b.string_marks;
            case "=": return b.equals;
            case "0": return b.zero;
            case "1": return b.one;
            case "==": return b.equalsequals;
            case "int": return b.int_;
            case "int name": return b.int_name;
            case "int see": return b.int_see;
            case "if": return b.if_;
            case "else": return b.else_;
            case "yes": return b.yes;
            case "no": return b.no;
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

        // 答對，隱藏 block
        foreach (Image img in blockImages)
            if (img != null) img.gameObject.SetActive(false);

        // 關閉自己的 group panel
        if (computerPanel != null) computerPanel.SetActive(false);
        if (printGroupPanel != null) printGroupPanel.SetActive(false);

        // 換蘋果圖片
        if (appleImage != null && appleSprite2 != null)
            appleImage.sprite = appleSprite2;

        // 停用蘋果按鈕
        if (appleButton != null)
            appleButton.interactable = false;

        // 顯示另一邊
        if (appleInteraction != null)
            appleInteraction.ShowOtherSide(isGoodGroup);

        // 通知 ResultManager
        if (resultManager != null)
        {
            if (isGoodGroup) resultManager.OnGoodComplete();
            else resultManager.OnBadComplete();
        }
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
}