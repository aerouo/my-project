using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PrintGameManager : MonoBehaviour
{
    [Header("UI 連結")]
    public GameObject printGroupPanel;   // 拖曳視窗
    public GameObject resultPanel;       // 結算畫面
    public TextMeshProUGUI timeText;     // 顯示耗時
    public TextMeshProUGUI resultText;   // 結算畫面的文字

    [Header("錯誤提示")]
    public TextMeshProUGUI wrongText;    // 錯誤提示文字

    [Header("方塊發光設定")]
    public Image[] blockImages;          // 依序放 tuo1~5 的 Image
    public Color highlightColor = Color.yellow;
    public float highlightDuration = 0.5f;

    private float startTime;

    public void StartGame()
    {
        startTime = Time.time;
    }

    public void CheckAnswer()
    {
        DropSlot[] slots = FindObjectsByType<DropSlot>(FindObjectsSortMode.None);
        foreach (DropSlot slot in slots)
        {
            if (!slot.isCorrect)
            {
                StartCoroutine(ShowWrongHint());
                return;
            }
        }
        // 全部正確！
        StartCoroutine(ExecuteBlocks());
    }

    IEnumerator ShowWrongHint()
    {
        if (wrongText != null)
        {
            wrongText.text = "順序錯誤，再試試看吧！";
            wrongText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            wrongText.gameObject.SetActive(false);
        }
        if (printGroupPanel != null) printGroupPanel.SetActive(true);
    }

    IEnumerator ExecuteBlocks()
    {
        float elapsed = Time.time - startTime;

        // 關閉拖曳視窗
        if (printGroupPanel != null) printGroupPanel.SetActive(false);

        // 方塊依序發光
        foreach (Image img in blockImages)
        {
            if (img == null) continue;
            Color original = img.color;
            img.color = highlightColor;
            yield return new WaitForSeconds(highlightDuration);
            img.color = original;
        }

        // 顯示結算
        ShowResult(elapsed);
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