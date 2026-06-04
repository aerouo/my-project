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
            SetHistoryText("歷史紀錄：尚無資料");
            return;
        }

        FirestoreManager.Instance.LoadQuestRecord(levelID, data =>
        {
            if (data == null || data.Count == 0)
            {
                SetHistoryText("歷史紀錄：尚無資料");
                return;
            }

            float bestTime = GetBestCompletedTime(data);

            if (bestTime <= 0)
            {
                SetHistoryText("歷史紀錄：尚無完成紀錄");
                return;
            }

            SetHistoryText("歷史紀錄：" + FormatTime(bestTime));
        });
    }

    private float GetBestCompletedTime(Dictionary<string, object> data)
    {
        float best = float.MaxValue;

        if (data.TryGetValue("best", out object bestObj))
        {
            TryUpdateBestTime(bestObj, ref best);
        }

        if (data.TryGetValue("latest", out object latestObj))
        {
            TryUpdateBestTime(latestObj, ref best);
        }

        if (data.TryGetValue("history", out object historyObj))
        {
            List<object> history = ConvertToObjectList(historyObj);

            foreach (object item in history)
            {
                TryUpdateBestTime(item, ref best);
            }
        }

        return best == float.MaxValue ? 0f : best;
    }

    private void TryUpdateBestTime(object recordObj, ref float best)
    {
        Dictionary<string, object> record = ConvertToDictionary(recordObj);

        if (record == null)
            return;

        int process = GetInt(record, "process");

        if (process < 100)
            return;

        float time = GetFloat(record, "time");

        if (time > 0 && time < best)
            best = time;
    }

    private Dictionary<string, object> ConvertToDictionary(object obj)
    {
        if (obj == null)
            return null;

        if (obj is Dictionary<string, object> dict)
            return dict;

        if (obj is IDictionary<string, object> iDict)
            return new Dictionary<string, object>(iDict);

        return null;
    }

    private List<object> ConvertToObjectList(object obj)
    {
        if (obj == null)
            return new List<object>();

        if (obj is List<object> list)
            return list;

        if (obj is IEnumerable<object> enumerable)
            return new List<object>(enumerable);

        return new List<object>();
    }

    private int GetInt(Dictionary<string, object> dict, string key)
    {
        if (dict != null && dict.TryGetValue(key, out object value))
            return Convert.ToInt32(value);

        return 0;
    }

    private float GetFloat(Dictionary<string, object> dict, string key)
    {
        if (dict != null && dict.TryGetValue(key, out object value))
            return Convert.ToSingle(value);

        return 0f;
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);

        return $"{minutes:00}:{secs:00}";
    }

    private void SetHistoryText(string text)
    {
        if (historyText != null)
            historyText.text = text;
    }
}