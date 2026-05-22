using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class QuestBestRecordDisplay : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI historyText;

    public void ShowRecord(string levelID)
    {
        if (historyText != null)
            historyText.text = "歷史紀錄：讀取中...";

        if (FirestoreManager.Instance == null)
        {
            historyText.text = "歷史紀錄：尚無資料";
            return;
        }

        FirestoreManager.Instance.LoadQuestRecord(levelID, data =>
        {
            if (data == null || !data.ContainsKey("latest"))
            {
                historyText.text = "歷史紀錄：尚無資料";
                return;
            }

            float bestTime = GetBestTime(data);

            if (bestTime <= 0)
            {
                historyText.text = "歷史紀錄：尚無資料";
                return;
            }

            historyText.text =
                "歷史紀錄：" + FormatTime(bestTime);
        });
    }

    float GetBestTime(Dictionary<string, object> data)
    {
        float best = 999999f;

        if (data.TryGetValue("latest", out object latestObj))
        {
            float t = GetTimeFromObj(latestObj);
            if (t > 0 && t < best)
                best = t;
        }

        if (data.TryGetValue("history", out object historyObj))
        {
            List<object> history = historyObj as List<object>;

            if (history != null)
            {
                foreach (object item in history)
                {
                    float t = GetTimeFromObj(item);
                    if (t > 0 && t < best)
                        best = t;
                }
            }
        }

        return best == 999999f ? 0f : best;
    }

    float GetTimeFromObj(object obj)
    {
        Dictionary<string, object> dict = obj as Dictionary<string, object>;

        if (dict == null || !dict.ContainsKey("time"))
            return 0f;

        return Convert.ToSingle(dict["time"]);
    }

    string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        return $"{minutes:00}:{secs:00}";
    }
}