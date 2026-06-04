using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IfElseResultManager : MonoBehaviour
{
    [Header("結果 UI")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    private bool goodDone = false;
    private bool badDone = false;
    private float startTime;

    void Start()
    {
        startTime = Time.time;
    }

    public void OnGoodComplete()
    {
        goodDone = true;
        CheckBothComplete();
    }

    public void OnBadComplete()
    {
        badDone = true;
        CheckBothComplete();
    }

    void CheckBothComplete()
    {
        if (!goodDone || !badDone) return;

        float elapsed = Time.time - startTime;
        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultText != null)
        {
            int minutes = (int)(elapsed / 60);
            int seconds = (int)(elapsed % 60);
            resultText.text = $"恭喜通關\n耗時：{minutes:00}:{seconds:00}";
        }
    }
}