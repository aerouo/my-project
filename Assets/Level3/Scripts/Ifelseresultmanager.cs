using UnityEngine;
using TMPro;

public class IfElseResultManager : MonoBehaviour
{
    [Header("結果 UI")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    [Header("闖關紀錄")]
    public string questLevelID = "Level3";
    public int clearScore = 0;
    public Level3Manager level3Manager;

    private bool goodDone = false;
    private bool badDone = false;
    private bool progress75Saved = false;
    private bool progress100Saved = false;

    public void OnGoodComplete()
    {
        if (goodDone) return;

        if (level3Manager != null)
            level3Manager.PauseQuestTimer();

        goodDone = true;

        Save75IfNeeded();
        CheckBothComplete();
    }

    public void OnBadComplete()
    {
        if (badDone) return;

        if (level3Manager != null)
            level3Manager.PauseQuestTimer();

        badDone = true;

        Save75IfNeeded();
        CheckBothComplete();
    }

    void Save75IfNeeded()
    {
        if (progress75Saved)
            return;

        if (goodDone || badDone)
        {
            progress75Saved = true;

            float elapsed = GetElapsedTime();

            if (FirestoreManager.Instance != null)
            {
                FirestoreManager.Instance.SaveQuestProgress(
                    questLevelID,
                    75,
                    elapsed,
                    clearScore
                );
            }

            Debug.Log("【Level3】完成 good / bad 其中一邊，停止計時並存 75%");
        }
    }

    void CheckBothComplete()
    {
        if (!goodDone || !badDone)
            return;

        if (progress100Saved)
            return;

        progress100Saved = true;

        if (level3Manager != null)
            level3Manager.PauseQuestTimer();

        float elapsed = GetElapsedTime();

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveQuestRecord(
                questLevelID,
                elapsed,
                clearScore
            );

            FirestoreManager.Instance.AddCoins(200);

            Debug.Log("【Level3】完成關卡，儲存 100% 紀錄，獎勵紙鶴：200");
        }

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
        {
            int minutes = (int)(elapsed / 60);
            int seconds = (int)(elapsed % 60);
            resultText.text = $"恭喜通關\n耗時：{minutes:00}:{seconds:00}";
        }

        Debug.Log("【Level3】good / bad 都完成，停止計時並存 100%");
    }

    float GetElapsedTime()
    {
        if (level3Manager != null)
            return level3Manager.GetElapsedTime();

        return 0f;
    }
}