using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class LearningRecordManager : MonoBehaviour
{
    [System.Serializable]
    public class HistoryItemUI
    {
        public GameObject root;
        public TMP_Text txtDate;
        public TMP_Text txtProgress;
        public TMP_Text txtDuration;
    }

    [System.Serializable]
    public class LearningRecordUI
    {
        public TMP_Text txtProgress;
        public TMP_Text txtLastDate;
        public TMP_Text txtLastDuration;

        public GameObject historyContainer;
        public TMP_Text txtNoData;

        [Header("展開/收合控制")]
        public HistoryToggle historyToggle;

        public HistoryItemUI[] historyItems;
    }

    [System.Serializable]
    public class BasicLessonUI
    {
        public string basicID;
        public LearningRecordUI videoRecord;
        public LearningRecordUI quizRecord;
    }

    [System.Serializable]
    public class AdvancedLessonUI
    {
        public string advancedID;
        public LearningRecordUI easyRecord;
        public LearningRecordUI normalRecord;
        public LearningRecordUI hardRecord;
    }

    [System.Serializable]
    public class ChallengeLessonUI
    {
        public string questID;
        public LearningRecordUI challengeRecord;
    }

    [Header("基礎課程 01~05")]
    public BasicLessonUI[] basicLessons;

    [Header("進階課程 01~05")]
    public AdvancedLessonUI[] advancedLessons;

    [Header("闖關課程 01~05")]
    public ChallengeLessonUI[] challengeLessons;

    private void OnEnable()
    {
        ResetAllUI();
        LoadAllRecords();
    }

    private void ResetAllUI()
    {
        foreach (var lesson in basicLessons)
        {
            ResetRecordUI(lesson.videoRecord);
            ResetRecordUI(lesson.quizRecord);
        }

        foreach (var lesson in advancedLessons)
        {
            ResetRecordUI(lesson.easyRecord);
            ResetRecordUI(lesson.normalRecord);
            ResetRecordUI(lesson.hardRecord);
        }

        foreach (var lesson in challengeLessons)
        {
            ResetRecordUI(lesson.challengeRecord);
        }
    }

    private void LoadAllRecords()
    {
        if (FirestoreManager.Instance == null)
        {
            Debug.LogWarning("找不到 FirestoreManager");
            return;
        }

        foreach (var lesson in basicLessons)
        {
            string id = lesson.basicID;

            FirestoreManager.Instance.LoadBasicVideoRecord(id, data =>
            {
                ApplyRecordData(lesson.videoRecord, data, false, id + "_video");
            });

            FirestoreManager.Instance.LoadBasicQuizRecord(id, data =>
            {
                ApplyRecordData(lesson.quizRecord, data, true, id + "_quiz");
            });
        }

        foreach (var lesson in advancedLessons)
        {
            string id = lesson.advancedID;

            FirestoreManager.Instance.LoadAdvancedRecord(id, "easy", data =>
            {
                ApplyRecordData(lesson.easyRecord, data, true, id + "_easy");
            });

            FirestoreManager.Instance.LoadAdvancedRecord(id, "normal", data =>
            {
                ApplyRecordData(lesson.normalRecord, data, true, id + "_normal");
            });

            FirestoreManager.Instance.LoadAdvancedRecord(id, "hard", data =>
            {
                ApplyRecordData(lesson.hardRecord, data, true, id + "_hard");
            });
        }

        foreach (var lesson in challengeLessons)
        {
            string id = lesson.questID;

            FirestoreManager.Instance.LoadQuestRecord(id, data =>
            {
                ApplyRecordData(lesson.challengeRecord, data, true, id + "_challenge");
            });
        }
    }

    private void ResetRecordUI(LearningRecordUI ui)
    {
        if (ui == null) return;

        SetText(ui.txtProgress, "0%");
        SetText(ui.txtLastDate, "");
        SetText(ui.txtLastDuration, "");

        if (ui.historyToggle != null)
            ui.historyToggle.SetHasData(false);

        if (ui.historyContainer != null)
            ui.historyContainer.SetActive(false);

        if (ui.txtNoData != null)
            ui.txtNoData.gameObject.SetActive(false);

        if (ui.historyItems != null)
        {
            foreach (var item in ui.historyItems)
            {
                if (item == null) continue;

                if (item.root != null)
                    item.root.SetActive(false);

                SetText(item.txtDate, "");
                SetText(item.txtProgress, "");
                SetText(item.txtDuration, "");
            }
        }
    }

    private void ApplyRecordData(LearningRecordUI ui, Dictionary<string, object> data, bool useHistory, string debugID)
    {
        if (ui == null)
        {
            Debug.LogWarning(debugID + " 的 UI 沒有拖好");
            return;
        }

        if (data == null || data.Count == 0)
        {
            Debug.Log(debugID + " 沒有讀到紀錄");

            if (ui.historyToggle != null)
                ui.historyToggle.SetHasData(false);

            return;
        }

        if (data.TryGetValue("latest", out object latestObj))
        {
            Dictionary<string, object> latest = ConvertToDictionary(latestObj);

            if (latest != null)
            {
                int process = GetInt(latest, "process");
                float time = GetFloat(latest, "time");
                string date = FormatDateOnly(GetString(latest, "date"));

                SetText(ui.txtProgress, process + "%");
                SetText(ui.txtLastDate, date);
                SetText(ui.txtLastDuration, time > 0 ? time.ToString("0.0") + "秒" : "");
            }
        }

        if (!useHistory)
            return;

        if (!data.TryGetValue("history", out object historyObj))
        {
            Debug.Log(debugID + " 沒有 history 欄位");

            if (ui.historyToggle != null)
                ui.historyToggle.SetHasData(false);

            return;
        }

        List<object> history = ConvertToObjectList(historyObj);

        Debug.Log(debugID + " History 數量：" + history.Count);

        bool hasHistory = history.Count > 0;

        if (ui.historyToggle != null)
            ui.historyToggle.SetHasData(hasHistory);

        if (!hasHistory)
            return;

        int itemCount = ui.historyItems == null ? 0 : ui.historyItems.Length;

        for (int i = 0; i < itemCount; i++)
        {
            HistoryItemUI target = ui.historyItems[i];
            if (target == null) continue;

            if (i < history.Count)
            {
                Dictionary<string, object> itemData = ConvertToDictionary(history[i]);

                if (itemData != null)
                {
                    int process = GetInt(itemData, "process");
                    float time = GetFloat(itemData, "time");
                    string date = FormatDateOnly(GetString(itemData, "date"));

                    if (target.root != null)
                        target.root.SetActive(true);

                    SetText(target.txtDate, date);
                    SetText(target.txtProgress, process + "%");
                    SetText(target.txtDuration, time > 0 ? time.ToString("0.0") + "秒" : "");
                }
                else
                {
                    if (target.root != null)
                        target.root.SetActive(false);
                }
            }
            else
            {
                if (target.root != null)
                    target.root.SetActive(false);
            }
        }
    }

    private string FormatDateOnly(string date)
    {
        if (string.IsNullOrEmpty(date))
            return "";

        if (date.Contains(" "))
            return date.Split(' ')[0];

        return date;
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

    private void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value;
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

    private string GetString(Dictionary<string, object> dict, string key)
    {
        if (dict != null && dict.TryGetValue(key, out object value))
            return value.ToString();

        return "";
    }
}