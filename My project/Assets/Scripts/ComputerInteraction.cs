using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComputerInteraction : MonoBehaviour
{
    [Header("--- UI 物件連結 ---")]
    public GameObject computerPanel;
    public GameObject printGamePanel;
    public GameObject resultPanel;
    public TextMeshProUGUI targetText;   // 這是 Image-1 下唯一的文字框
    public TextMeshProUGUI warningText;  // 左下角的監控字

    [Header("--- 遊戲規則設定 ---")]
    public string correctWord = "print";
    public int maxLines = 1;

    void Update()
    {
        // 自動更新左下角：{ 這裡很危險 ... 1/1 }
        if (warningText != null && targetText != null)
        {
            // 只有當文字框不是空的時候才算行數
            int currentLines = string.IsNullOrEmpty(targetText.text) ? 0 :
                               targetText.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries).Length;

            warningText.text = $"這裡很危險\n不宜久留\n限用行數 {currentLines}/{maxLines}";
            warningText.color = (currentLines > maxLines) ? Color.red : Color.black;
        }
    }

    public void OnExecuteClick()
    {
        // 判定答案是否正確
        if (targetText != null && targetText.text.Trim() == correctWord)
        {
            ExecuteSuccess();
        }
    }

    void ExecuteSuccess()
    {
        targetText.color = new Color(1f, 0.92f, 0f); // 變金色
        Invoke("ShowResult", 1.5f); // 顯示結算畫面
    }

    void ShowResult()
    {
        if (printGamePanel != null) printGamePanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(true);
    }

    public void OpenBrainGame()
    {
        if (computerPanel != null) computerPanel.SetActive(false);
        if (printGamePanel != null) printGamePanel.SetActive(true);
    }
}