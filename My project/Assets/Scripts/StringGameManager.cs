using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StringGameManager : MonoBehaviour
{
    [Header("UI 連結")]
    public GameObject printGroupPanel;
    public GameObject resultPanel;
    public GameObject computerPanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI wrongText;

    [Header("放置槽（依順序拉入 kuang1~9）")]
    public DropSlot[] slots;

    [Header("結果展示方塊")]
    public Image[] blockImages;
    public Color highlightColor = Color.yellow;
    public Color wrongColor = Color.red;
    public float highlightDuration = 0.5f;

    [Header("背景圖動畫")]
    public Image backgroundImage;   // 拉入 Background (string) 的 Image
    public GameObject puaper;       // 拉入 puaper 物件
    public Sprite background1;      // 原本的背景圖（圖1）
    public Sprite background2;      // 要換的背景圖（圖2）
    public float bgSwitchDelay = 1f; // 換圖等待時間（可調）
    public Image puaperImage;       // 拉入 puaper 的 Image 組件
    public Sprite puaperSprite2;    // puaper 顯示時換成的新圖

    // 每個 slot 對應的各 blockID Sprite（9組）
    [Header("Block 1 圖片")]
    public Sprite block1_class, block1_main, block1_print, block1_string, block1_string_marks, block1_name_num, block1_string_num, block1_equals, block1_hum001;

    [Header("Block 2 圖片")]
    public Sprite block2_class, block2_main, block2_print, block2_string, block2_string_marks, block2_name_num, block2_string_num, block2_equals, block2_hum001;

    [Header("Block 3 圖片")]
    public Sprite block3_class, block3_main, block3_print, block3_string, block3_string_marks, block3_name_num, block3_string_num, block3_equals, block3_hum001;

    [Header("Block 4 圖片")]
    public Sprite block4_class, block4_main, block4_print, block4_string, block4_string_marks, block4_name_num, block4_string_num, block4_equals, block4_hum001;

    [Header("Block 5 圖片")]
    public Sprite block5_class, block5_main, block5_print, block5_string, block5_string_marks, block5_name_num, block5_string_num, block5_equals, block5_hum001;

    [Header("Block 6 圖片")]
    public Sprite block6_class, block6_main, block6_print, block6_string, block6_string_marks, block6_name_num, block6_string_num, block6_equals, block6_hum001;

    [Header("Block 7 圖片")]
    public Sprite block7_class, block7_main, block7_print, block7_string, block7_string_marks, block7_name_num, block7_string_num, block7_equals, block7_hum001;

    [Header("Block 8 圖片")]
    public Sprite block8_class, block8_main, block8_print, block8_string, block8_string_marks, block8_name_num, block8_string_num, block8_equals, block8_hum001;

    [Header("Block 9 圖片")]
    public Sprite block9_class, block9_main, block9_print, block9_string, block9_string_marks, block9_name_num, block9_string_num, block9_equals, block9_hum001;

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
            case 0: sprites = new Sprite[] { block1_class, block1_main, block1_print, block1_string, block1_string_marks, block1_name_num, block1_string_num, block1_equals, block1_hum001 }; break;
            case 1: sprites = new Sprite[] { block2_class, block2_main, block2_print, block2_string, block2_string_marks, block2_name_num, block2_string_num, block2_equals, block2_hum001 }; break;
            case 2: sprites = new Sprite[] { block3_class, block3_main, block3_print, block3_string, block3_string_marks, block3_name_num, block3_string_num, block3_equals, block3_hum001 }; break;
            case 3: sprites = new Sprite[] { block4_class, block4_main, block4_print, block4_string, block4_string_marks, block4_name_num, block4_string_num, block4_equals, block4_hum001 }; break;
            case 4: sprites = new Sprite[] { block5_class, block5_main, block5_print, block5_string, block5_string_marks, block5_name_num, block5_string_num, block5_equals, block5_hum001 }; break;
            case 5: sprites = new Sprite[] { block6_class, block6_main, block6_print, block6_string, block6_string_marks, block6_name_num, block6_string_num, block6_equals, block6_hum001 }; break;
            case 6: sprites = new Sprite[] { block7_class, block7_main, block7_print, block7_string, block7_string_marks, block7_name_num, block7_string_num, block7_equals, block7_hum001 }; break;
            case 7: sprites = new Sprite[] { block8_class, block8_main, block8_print, block8_string, block8_string_marks, block8_name_num, block8_string_num, block8_equals, block8_hum001 }; break;
            case 8: sprites = new Sprite[] { block9_class, block9_main, block9_print, block9_string, block9_string_marks, block9_name_num, block9_string_num, block9_equals, block9_hum001 }; break;
            default: return null;
        }

        switch (blockID)
        {
            case "class": return sprites[0];
            case "main": return sprites[1];
            case "print": return sprites[2];
            case "string": return sprites[3];
            case "string marks": return sprites[4];
            case "name num": return sprites[5];
            case "string num": return sprites[6];
            case "=": return sprites[7];
            case "hum-001": return sprites[8];
            default: return null;
        }
    }

    IEnumerator PlayBackgroundAnimation()
    {
        // 1. 隱藏 puaper
        if (puaper != null) puaper.SetActive(false);

        // 2. 換成背景圖2
        if (backgroundImage != null) backgroundImage.sprite = background2;

        yield return new WaitForSeconds(bgSwitchDelay);

        // 3. 換回背景圖1
        if (backgroundImage != null) backgroundImage.sprite = background1;

        yield return new WaitForSeconds(bgSwitchDelay);

        // 4. 顯示 puaper，同時換圖
        if (puaperImage != null && puaperSprite2 != null)
            puaperImage.sprite = puaperSprite2;
        if (puaper != null) puaper.SetActive(true);
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

            // 最後一個槽（string num）全對後執行背景動畫 + 輸出文字
            if (slots[i].acceptID == "string num")
            {
                // 先播背景動畫（隱藏puaper → 換圖2 → 換回圖1 → 顯示puaper）
                yield return StartCoroutine(PlayBackgroundAnimation());
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