using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InlineInputGenerator : MonoBehaviour
{
    public TMP_Text textDisplay;
    public GameObject inputFieldPrefab;
    [Header("搜尋標籤，預設為 [input]，你可以改成任何你想用的標籤")]
    public string tagToFind = "[input]"; 
    [Header("Y軸偏移補正，可根據字體或其他應用調整")]
    public float verticalOffset = 10f; // 調整輸入框的垂直位置

    void Start()
    {
        GenerateInputs();
    }

    [ContextMenu("Generate")] // 讓你在 Inspector 點右鍵就能測試
    public void GenerateInputs()
    {
        // 1. 重要：強制刷新文字資訊
        textDisplay.ForceMeshUpdate();
        TMP_TextInfo textInfo = textDisplay.textInfo;

        string content = textDisplay.text;
        int searchIndex = 0;

        // 2. 循環搜尋所有標籤
        while ((searchIndex = content.IndexOf(tagToFind, searchIndex)) != -1)
        {
            // 檢查索引是否安全
            if (searchIndex >= textInfo.characterCount) break;

            TMP_CharacterInfo charInfo = textInfo.characterInfo[searchIndex];

            // 3. 生成並設定父物件 (設為 textDisplay 的子物件最省事)
            GameObject inputObj = Instantiate(inputFieldPrefab, textDisplay.transform);
            RectTransform rt = inputObj.GetComponent<RectTransform>();

            // 4. 設定位置：使用 characterInfo 的局部座標
            // 我們取該字元的左下角
            Vector3 pos = charInfo.bottomLeft;
            // 太低了，稍微往上調整一點（根據字體大小調整）
            // rt.localPosition = new Vector3(pos.x, pos.y, 0);
            rt.localPosition = new Vector3(charInfo.bottomLeft.x, charInfo.baseLine + verticalOffset, 0);

            // 5. (進階) 自動調整寬度：讓 InputField 寬度等於標籤的寬度
            float tagWidth = CalculateTagWidth(searchIndex, tagToFind.Length);
            rt.sizeDelta = new Vector2(tagWidth, rt.sizeDelta.y);

            searchIndex += tagToFind.Length;
        }
    }

    float CalculateTagWidth(int start, int length)
    {
        var textInfo = textDisplay.textInfo;
        float startX = textInfo.characterInfo[start].bottomLeft.x;
        float endX = textInfo.characterInfo[start + length - 1].bottomRight.x;
        return endX - startX;
    }
}