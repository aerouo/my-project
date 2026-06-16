using UnityEngine;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FirestoreManager : MonoBehaviour
{
    // =========================================================
    // 0. 單例與 Firebase 基本設定
    // =========================================================
    // 這一段負責讓 FirestoreManager 在整個遊戲中只存在一個。
    // 也會初始化 FirebaseFirestore，並取得目前登入使用者的 UID。

    private static FirestoreManager _instance;
    public static FirestoreManager Instance => _instance;

    private FirebaseFirestore db;
    private string userID;

    private Dictionary<string, object> achievementCache = new Dictionary<string, object>();
    public bool AchievementLoaded { get; private set; } = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            db = FirebaseFirestore.DefaultInstance;
            RefreshUser();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void RefreshUser()
    {
        if (db == null)
            db = FirebaseFirestore.DefaultInstance;

        userID = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        Debug.Log("目前 Firebase UserID = " + userID);
    }

    private bool CheckReady()
    {
        if (db == null)
            db = FirebaseFirestore.DefaultInstance;

        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("Firebase Auth 尚未登入，暫時不能讀寫資料");
            return false;
        }

        userID = user.UserId;

        if (string.IsNullOrEmpty(userID))
        {
            Debug.LogWarning("Firebase UserID 是空的");
            return false;
        }

        return true;
    }

    // =========================================================
    // 1. 學習紀錄：共用工具
    // =========================================================
    // 這一段放共用方法，主要給進階關卡與闖關紀錄使用。
    // 可以統一建立紀錄格式、更新 latest / history / best。

    private Dictionary<string, object> CreateRecord(float timeSeconds, int score, int process)
    {
        return new Dictionary<string, object>
    {
        { "time", timeSeconds },
        { "score", score },
        { "process", process },
        { "date", DateTime.Now.ToString("yyyy/MM/dd") }
    };
    }

    private void SaveRecordWithHistoryAndBest(
        DocumentReference docRef,
        Dictionary<string, object> latestRecord,
        System.Action onSaved = null)
    {
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("讀取紀錄失敗：" + task.Exception);
                return;
            }

            List<object> history = new List<object>();
            Dictionary<string, object> bestRecord = null;

            if (task.Result.Exists)
            {
                Dictionary<string, object> oldData = task.Result.ToDictionary();

                if (oldData.TryGetValue("latest", out object oldLatestObj))
                    history.Insert(0, oldLatestObj);

                if (oldData.TryGetValue("history", out object oldHistoryObj))
                    history.AddRange(ConvertToObjectList(oldHistoryObj));

                if (oldData.TryGetValue("best", out object oldBestObj))
                    bestRecord = ConvertToDictionary(oldBestObj);
            }

            if (history.Count > 5)
                history.RemoveRange(5, history.Count - 5);

            int process = GetDictInt(latestRecord, "process");
            float newTime = GetDictFloat(latestRecord, "time");

            if (process == 100)
            {
                if (bestRecord == null)
                {
                    bestRecord = latestRecord;
                }
                else
                {
                    float oldBestTime = GetDictFloat(bestRecord, "time");

                    if (newTime < oldBestTime || oldBestTime <= 0)
                        bestRecord = latestRecord;
                }
            }

            Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "latest", latestRecord },
            { "history", history }
        };

            if (bestRecord != null)
                updates["best"] = bestRecord;

            docRef.SetAsync(updates, SetOptions.MergeAll).ContinueWithOnMainThread(saveTask =>
            {
                if (saveTask.IsCompletedSuccessfully)
                {
                    Debug.Log("紀錄儲存成功");
                    onSaved?.Invoke();
                }
                else
                {
                    Debug.LogWarning("紀錄儲存失敗：" + saveTask.Exception);
                }
            });
        });
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
    private int GetDictInt(Dictionary<string, object> dict, string key)
    {
        if (dict != null && dict.TryGetValue(key, out object value))
            return Convert.ToInt32(value);

        return 0;
    }

    private float GetDictFloat(Dictionary<string, object> dict, string key)
    {
        if (dict != null && dict.TryGetValue(key, out object value))
            return Convert.ToSingle(value);

        return 0f;
    }

    private string GetDictString(Dictionary<string, object> dict, string key)
    {
        if (dict != null && dict.TryGetValue(key, out object value))
            return value.ToString();

        return "";
    }

    // =========================================================
    // 2. 學習紀錄：基礎影片
    // 存在 learning / videos
    // 成就與 Learning 顯示都讀這份
    // =========================================================
    // 這一段負責基礎學習影片的紀錄。
    // 資料存在 users/{userID}/learning/videos。
    // 成就判斷與 Learning 頁面都會讀這份資料。

    public void SaveBasicVideoRecord(string basicID, float timeSeconds = 0f, int process = 100)
    {
        if (!CheckReady()) return;

        string date = DateTime.Now.ToString("yyyy/MM/dd");
        process = Mathf.Clamp(process, 0, 100);

        db.Collection("users").Document(userID)
          .Collection("learning").Document("videos")
          .SetAsync(new Dictionary<string, object>
          {
          { basicID + "_watched", true },
          { basicID + "_date", date },
          { basicID + "_process", process },
          { basicID + "_time", timeSeconds }
          }, SetOptions.MergeAll)
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsCompletedSuccessfully)
                  Debug.Log("基礎影片完成紀錄儲存成功：" + basicID + " / " + process + "%");
              else
                  Debug.LogWarning("基礎影片完成紀錄儲存失敗：" + task.Exception);
          });
    }

    public void SaveBasicVideoProgress(string basicID, int process, float timeSeconds = 0f)
    {
        if (!CheckReady()) return;

        process = Mathf.Clamp(process, 0, 100);

        var docRef = db.Collection("users").Document(userID)
                       .Collection("learning").Document("videos");

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            Dictionary<string, object> data = task.Result.Exists
                ? task.Result.ToDictionary()
                : new Dictionary<string, object>();

            int oldBestProcess = 0;

            if (data.ContainsKey(basicID + "_best_process"))
                oldBestProcess = GetDictInt(data, basicID + "_best_process");

            Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { basicID + "_watched", process >= 90 },
            { basicID + "_date", DateTime.Now.ToString("yyyy/MM/dd") },
            { basicID + "_process", process },
            { basicID + "_time", timeSeconds }
        };

            if (process > oldBestProcess)
            {
                updates[basicID + "_best_process"] = process;
                updates[basicID + "_best_date"] = DateTime.Now.ToString("yyyy/MM/dd");
                updates[basicID + "_best_time"] = timeSeconds;
            }

            docRef.SetAsync(updates, SetOptions.MergeAll);
        });
    }

    public void LoadBasicVideoRecord(string basicID, Action<Dictionary<string, object>> onLoaded)
    {
        if (!CheckReady())
        {
            onLoaded?.Invoke(new Dictionary<string, object>());
            return;
        }

        db.Collection("users").Document(userID)
          .Collection("learning").Document("videos")
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsFaulted || task.IsCanceled || !task.Result.Exists)
              {
                  onLoaded?.Invoke(new Dictionary<string, object>());
                  return;
              }

              Dictionary<string, object> data = task.Result.ToDictionary();

              bool hasProcess = data.ContainsKey(basicID + "_process");
              bool hasWatched = data.ContainsKey(basicID + "_watched");

              if (!hasProcess && !hasWatched)
              {
                  onLoaded?.Invoke(new Dictionary<string, object>());
                  return;
              }

              bool watched = false;

              if (hasWatched)
                  watched = Convert.ToBoolean(data[basicID + "_watched"]);

              int process = 0;

              if (hasProcess)
                  process = GetDictInt(data, basicID + "_process");
              else if (watched)
                  process = 100;

              Dictionary<string, object> latest = new Dictionary<string, object>
              {
              { "process", process },
              { "date", GetDictString(data, basicID + "_date") },
              { "time", GetDictFloat(data, basicID + "_time") }
              };

              onLoaded?.Invoke(new Dictionary<string, object>
              {
              { "latest", latest }
              });
          });
    }

    // =========================================================
    // 3. 學習紀錄：基礎測驗
    // 存在 learning / quizzes
    // 成就看 basic_01_done，Learning 顯示看 process/date/history
    // 日期只顯示年月日，不顯示時間
    // =========================================================

    public void SaveBasicQuizRecord(string basicID, System.Action onSaved = null)
    {
        if (!CheckReady()) return;

        string date = DateTime.Now.ToString("yyyy/MM/dd");

        var docRef = db.Collection("users").Document(userID)
                       .Collection("learning").Document("quizzes");

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("讀取基礎測驗紀錄失敗：" + task.Exception);
                return;
            }

            List<object> history = new List<object>();

            if (task.Result.Exists)
            {
                Dictionary<string, object> oldData = task.Result.ToDictionary();

                bool hadOldLatest = oldData.ContainsKey(basicID + "_done");

                if (hadOldLatest)
                {
                    Dictionary<string, object> oldLatest = new Dictionary<string, object>
                {
                    { "date", GetDictString(oldData, basicID + "_date") },
                    { "process", GetDictInt(oldData, basicID + "_process") },
                    { "time", GetDictFloat(oldData, basicID + "_time") }
                };

                    history.Insert(0, oldLatest);
                }

                if (oldData.TryGetValue(basicID + "_history", out object h))
                {
                    List<object> oldHistory = ConvertToObjectList(h);
                    history.AddRange(oldHistory);
                }
            }

            if (history.Count > 5)
                history.RemoveRange(5, history.Count - 5);

            Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { basicID + "_done", true },
            { basicID + "_date", date },
            { basicID + "_process", 100 },
            { basicID + "_time", 0f },
            { basicID + "_history", history }
        };

            docRef.SetAsync(updates, SetOptions.MergeAll).ContinueWithOnMainThread(saveTask =>
            {
                if (saveTask.IsCompletedSuccessfully)
                {
                    Debug.Log("基礎測驗紀錄儲存成功：" + basicID);

                    // 確定 Firebase 存完後，才去檢查成就
                    onSaved?.Invoke();
                }
                else
                {
                    Debug.LogWarning("基礎測驗紀錄儲存失敗：" + saveTask.Exception);
                }
            });
        });
    }
    public void LoadBasicQuizRecord(string basicID, Action<Dictionary<string, object>> onLoaded)
    {
        if (!CheckReady())
        {
            onLoaded?.Invoke(new Dictionary<string, object>());
            return;
        }

        db.Collection("users").Document(userID)
          .Collection("learning").Document("quizzes")
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsFaulted || task.IsCanceled || !task.Result.Exists)
              {
                  onLoaded?.Invoke(new Dictionary<string, object>());
                  return;
              }

              Dictionary<string, object> data = task.Result.ToDictionary();

              if (!data.ContainsKey(basicID + "_done"))
              {
                  onLoaded?.Invoke(new Dictionary<string, object>());
                  return;
              }

              int process = 100;

              if (data.ContainsKey(basicID + "_process"))
                  process = GetDictInt(data, basicID + "_process");

              Dictionary<string, object> latest = new Dictionary<string, object>
              {
              { "process", process },
              { "date", GetDictString(data, basicID + "_date") },
              { "time", 0f }
              };

              List<object> history = new List<object>();

              if (data.TryGetValue(basicID + "_history", out object historyObj))
                  history = historyObj as List<object> ?? new List<object>();

              onLoaded?.Invoke(new Dictionary<string, object>
              {
              { "latest", latest },
              { "history", history }
              });
          });
    }

    // =========================================================
    // 4. 學習紀錄：進階關卡
    // 存在 learningRecords / advanced / advanced_01 / easy
    // =========================================================

    public void SaveAdvancedRecord(string gameID, string difficulty, float timeSeconds, int score)
    {
        if (!CheckReady()) return;

        Dictionary<string, object> latestRecord = CreateRecord(timeSeconds, score, 100);

        DocumentReference docRef = db.Collection("users").Document(userID)
            .Collection("learningRecords").Document("advanced")
            .Collection(gameID).Document(difficulty);

        SaveRecordWithHistoryAndBest(docRef, latestRecord, () =>
        {
            CheckAdvancedLevelAchievement();
        });
    }

    public void LoadAdvancedRecord(string gameID, string difficulty, Action<Dictionary<string, object>> onLoaded)
    {
        if (!CheckReady())
        {
            onLoaded?.Invoke(new Dictionary<string, object>());
            return;
        }

        db.Collection("users").Document(userID)
          .Collection("learningRecords").Document("advanced")
          .Collection(gameID).Document(difficulty)
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsFaulted || task.IsCanceled || !task.Result.Exists)
              {
                  onLoaded?.Invoke(new Dictionary<string, object>());
                  return;
              }

              onLoaded?.Invoke(task.Result.ToDictionary());
          });
    }

    // =========================================================
    // 5. 學習紀錄：闖關
    // 存在 learningRecords / quest
    // 欄位格式：Level1_latest、Level1_history
    // =========================================================

    public void SaveQuestProgress(string levelID, int process, float timeSeconds = 0f, int score = 0)
    {
        if (!CheckReady()) return;

        process = Mathf.Clamp(process, 0, 99);
        float roundedTime = Mathf.Round(timeSeconds * 100f) / 100f;

        Dictionary<string, object> latestRecord = new Dictionary<string, object>
    {
        { "time", roundedTime },
        { "score", score },
        { "process", process },
        { "date", DateTime.Now.ToString("yyyy/MM/dd") }
    };

        DocumentReference docRef = db.Collection("users").Document(userID)
            .Collection("learningRecords").Document("quest");

        Dictionary<string, object> updates = new Dictionary<string, object>
    {
        { levelID + "_latest", latestRecord }
    };

        docRef.SetAsync(updates, SetOptions.MergeAll).ContinueWithOnMainThread(saveTask =>
        {
            if (saveTask.IsCompletedSuccessfully)
                Debug.Log("闖關進度儲存成功：" + levelID + " = " + process + "%，只更新 latest，不推進 history");
            else
                Debug.LogWarning("闖關進度儲存失敗：" + saveTask.Exception);
        });
    }

    public void SaveQuestRecord(string levelID, float timeSeconds, int score)
    {
        if (!CheckReady()) return;

        float roundedTime = Mathf.Round(timeSeconds * 100f) / 100f;

        Dictionary<string, object> latestRecord = new Dictionary<string, object>
    {
        { "time", roundedTime },
        { "score", score },
        { "process", 100 },
        { "date", DateTime.Now.ToString("yyyy/MM/dd") }
    };

        DocumentReference docRef = db.Collection("users").Document(userID)
            .Collection("learningRecords").Document("quest");

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("讀取闖關紀錄失敗：" + task.Exception);
                return;
            }

            List<object> history = new List<object>();
            Dictionary<string, object> bestRecord = null;

            if (task.Result.Exists)
            {
                Dictionary<string, object> oldData = task.Result.ToDictionary();

                if (oldData.TryGetValue(levelID + "_latest", out object oldLatest))
                {
                    Dictionary<string, object> oldLatestDict = ConvertToDictionary(oldLatest);

                    if (oldLatestDict != null && GetDictInt(oldLatestDict, "process") == 100)
                        history.Insert(0, oldLatestDict);
                }

                if (oldData.TryGetValue(levelID + "_history", out object oldHistory))
                    history.AddRange(ConvertToObjectList(oldHistory));

                if (oldData.TryGetValue(levelID + "_best", out object oldBest))
                    bestRecord = ConvertToDictionary(oldBest);
            }

            if (history.Count > 5)
                history.RemoveRange(5, history.Count - 5);

            if (bestRecord == null)
            {
                bestRecord = latestRecord;
            }
            else
            {
                float oldBestTime = GetDictFloat(bestRecord, "time");

                if (roundedTime < oldBestTime || oldBestTime <= 0)
                    bestRecord = latestRecord;
            }

            Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { levelID + "_latest", latestRecord },
            { levelID + "_history", history },
            { levelID + "_best", bestRecord }
        };

            docRef.SetAsync(updates, SetOptions.MergeAll).ContinueWithOnMainThread(saveTask =>
            {
                if (saveTask.IsCompletedSuccessfully)
                {
                    Debug.Log("闖關 100% 儲存成功：" + levelID + "，已更新 history / best");
                    CheckQuestAllClearAchievement();
                }
                else
                    Debug.LogWarning("闖關 100% 儲存失敗：" + saveTask.Exception);
            });
        });
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

    public void LoadQuestRecord(string levelID, Action<Dictionary<string, object>> onLoaded)
    {
        if (!CheckReady())
        {
            onLoaded?.Invoke(new Dictionary<string, object>());
            return;
        }

        db.Collection("users").Document(userID)
          .Collection("learningRecords").Document("quest")
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsFaulted || task.IsCanceled || !task.Result.Exists)
              {
                  onLoaded?.Invoke(new Dictionary<string, object>());
                  return;
              }

              Dictionary<string, object> data = task.Result.ToDictionary();
              Dictionary<string, object> result = new Dictionary<string, object>();

              if (data.TryGetValue(levelID + "_latest", out object latest))
                  result["latest"] = latest;

              if (data.TryGetValue(levelID + "_history", out object history))
                  result["history"] = history;

              if (data.TryGetValue(levelID + "_best", out object best))
                  result["best"] = best;

              onLoaded?.Invoke(result);
          });
    }

    // =========================================================
    // 6. 舊方法保留：影片 / 測驗 / 小遊戲 / 闖關完成
    // 避免其他舊 Script 呼叫時壞掉
    // =========================================================

    public void SaveVideoWatched(string videoID)
    {
        SaveBasicVideoRecord(videoID);
    }

    public void SaveQuizDone(string quizID, System.Action onSaved = null)
    {
        SaveBasicQuizRecord(quizID, onSaved);
    }


    // =========================================================
    // 7. 紙鶴 / 金幣
    // =========================================================

    public void AddCoins(int amount)
    {
        if (!CheckReady()) return;

        var userRef = db.Collection("users").Document(userID);

        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("讀取 coins 失敗：" + task.Exception);
                return;
            }

            int current = 0;

            if (task.Result.Exists && task.Result.TryGetValue("coins", out object val))
                current = Convert.ToInt32(val);

            SaveCoins(current + amount);
        });
    }

    public void SaveCoins(int coins)
    {
        if (!CheckReady()) return;

        db.Collection("users").Document(userID)
          .SetAsync(new Dictionary<string, object>
          {
              { "coins", coins }
          }, SetOptions.MergeAll)
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsCompletedSuccessfully)
                  Debug.Log("Firebase 儲存 coins 成功：" + coins);
              else
                  Debug.LogWarning("Firebase 儲存 coins 失敗：" + task.Exception);
          });
    }

    public void SaveClosetMoney(int coins)
    {
        SaveCoins(coins);
    }

    // =========================================================
    // 8. 衣櫃
    // =========================================================

    public void SaveWardrobeItem(string itemID, bool owned)
    {
        if (!CheckReady()) return;

        if (string.IsNullOrEmpty(itemID))
        {
            Debug.LogWarning("SaveWardrobeItem 失敗：itemID 是空的");
            return;
        }

        db.Collection("users").Document(userID)
          .Collection("closet").Document("items")
          .SetAsync(new Dictionary<string, object>
          {
              { itemID, owned }
          }, SetOptions.MergeAll)
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsCompletedSuccessfully)
                  Debug.Log("Firebase 儲存購買狀態成功：" + itemID + " = " + owned);
              else
                  Debug.LogWarning("Firebase 儲存購買狀態失敗：" + task.Exception);
          });
    }

    public void SaveClosetEquip(string part, string itemID)
    {
        if (!CheckReady()) return;

        if (string.IsNullOrEmpty(part))
        {
            Debug.LogWarning("SaveClosetEquip 失敗：part 是空的");
            return;
        }

        db.Collection("users").Document(userID)
          .Collection("closet").Document("equipped")
          .SetAsync(new Dictionary<string, object>
          {
              { part, itemID ?? "" }
          }, SetOptions.MergeAll)
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsCompletedSuccessfully)
                  Debug.Log("Firebase 儲存穿戴成功：" + part + " = " + itemID);
              else
                  Debug.LogWarning("Firebase 儲存穿戴失敗：" + task.Exception);
          });
    }

    public void LoadCloset(Action<
        Dictionary<string, object>,
        Dictionary<string, object>,
        Dictionary<string, object>> onLoaded)
    {
        if (!CheckReady())
        {
            onLoaded?.Invoke(
                new Dictionary<string, object>(),
                new Dictionary<string, object>(),
                new Dictionary<string, object>()
            );
            return;
        }

        var userRef = db.Collection("users").Document(userID);
        var closetRef = userRef.Collection("closet");

        Task<DocumentSnapshot> userTask = userRef.GetSnapshotAsync();
        Task<DocumentSnapshot> itemsTask = closetRef.Document("items").GetSnapshotAsync();
        Task<DocumentSnapshot> equippedTask = closetRef.Document("equipped").GetSnapshotAsync();

        Task.WhenAll(userTask, itemsTask, equippedTask).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("Firebase 衣櫃資料讀取失敗：" + task.Exception);

                onLoaded?.Invoke(
                    new Dictionary<string, object>(),
                    new Dictionary<string, object>(),
                    new Dictionary<string, object>()
                );

                return;
            }

            Dictionary<string, object> userDict = new Dictionary<string, object>();
            Dictionary<string, object> itemsDict = new Dictionary<string, object>();
            Dictionary<string, object> equippedDict = new Dictionary<string, object>();

            if (userTask.Result.Exists)
                userDict = userTask.Result.ToDictionary();

            if (itemsTask.Result.Exists)
                itemsDict = itemsTask.Result.ToDictionary();

            if (equippedTask.Result.Exists)
                equippedDict = equippedTask.Result.ToDictionary();

            Debug.Log("Firebase 衣櫃資料讀取完成");

            onLoaded?.Invoke(userDict, itemsDict, equippedDict);
        });
    }

    // =========================================================
    // 9. 使用者資料
    // =========================================================

    public void InitNewUser(string username)
    {
        if (!CheckReady()) return;

        var userRef = db.Collection("users").Document(userID);

        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("初始化使用者失敗：" + task.Exception);
                return;
            }

            string finalUsername = string.IsNullOrWhiteSpace(username) ? "User" : username;

            Dictionary<string, object> userData = new Dictionary<string, object>();

            if (!task.Result.Exists || !task.Result.ContainsField("username"))
                userData["username"] = finalUsername;

            if (!task.Result.Exists || !task.Result.ContainsField("coins"))
                userData["coins"] = 0;

            Dictionary<string, object> defaultItems = new Dictionary<string, object>
        {
            { "Top06", true },
            { "Bottom03", true }
        };

            Dictionary<string, object> defaultEquipped = new Dictionary<string, object>
        {
            { "top", "Top06" },
            { "bottom", "Bottom03" },
            { "hair", "" },
            { "glasses", "" },
            { "gloves", "" },
            { "shoes", "" },
            { "handItem", "" }
        };

            Dictionary<string, object> defaultAchievements = new Dictionary<string, object>
        {
            { "achievement_basic_01_unlocked", true },
            { "achievement_basic_01_completed", false },
            { "achievement_basic_01_date", "" },

            { "achievement_advanced_01_unlocked", false },
            { "achievement_advanced_01_completed", false },
            { "achievement_advanced_01_date", "" },

            { "achievement_quest_01_unlocked", false },
            { "achievement_quest_01_completed", false },
            { "achievement_quest_01_date", "" }
        };

            Dictionary<string, object> defaultVideos = new Dictionary<string, object>();
            Dictionary<string, object> defaultQuizzes = new Dictionary<string, object>();

            for (int i = 1; i <= 5; i++)
            {
                string basicID = "basic_" + i.ToString("00");

                defaultVideos[basicID + "_watched"] = false;
                defaultVideos[basicID + "_process"] = 0;
                defaultVideos[basicID + "_best_process"] = 0;
                defaultVideos[basicID + "_date"] = "";
                defaultVideos[basicID + "_time"] = 0f;

                defaultQuizzes[basicID + "_done"] = false;
                defaultQuizzes[basicID + "_process"] = 0;
                defaultQuizzes[basicID + "_date"] = "";
                defaultQuizzes[basicID + "_time"] = 0f;
                defaultQuizzes[basicID + "_history"] = new List<object>();
            }

            Dictionary<string, object> defaultQuest = new Dictionary<string, object>();

            for (int i = 1; i <= 5; i++)
            {
                string levelID = "Level" + i;

                defaultQuest[levelID + "_latest"] = new Dictionary<string, object>
            {
                { "time", 0f },
                { "score", 0 },
                { "process", 0 },
                { "date", "" }
            };

                defaultQuest[levelID + "_history"] = new List<object>();

                defaultQuest[levelID + "_best"] = new Dictionary<string, object>
            {
                { "time", 0f },
                { "score", 0 },
                { "process", 0 },
                { "date", "" }
            };
            }

            List<Task> initTasks = new List<Task>();

            if (userData.Count > 0)
            {
                initTasks.Add(userRef.SetAsync(userData, SetOptions.MergeAll));
            }

            initTasks.Add(userRef.Collection("closet").Document("items")
                .SetAsync(defaultItems, SetOptions.MergeAll));

            initTasks.Add(userRef.Collection("closet").Document("equipped")
                .SetAsync(defaultEquipped, SetOptions.MergeAll));

            initTasks.Add(userRef.Collection("achievements").Document("data")
                .SetAsync(defaultAchievements, SetOptions.MergeAll));

            initTasks.Add(userRef.Collection("learning").Document("videos")
                .SetAsync(defaultVideos, SetOptions.MergeAll));

            initTasks.Add(userRef.Collection("learning").Document("quizzes")
                .SetAsync(defaultQuizzes, SetOptions.MergeAll));

            initTasks.Add(userRef.Collection("learningRecords").Document("quest")
                .SetAsync(defaultQuest, SetOptions.MergeAll));

            // 進階紀錄：5 個遊戲 × 3 種難度，先建立好
            string[] difficulties = { "easy", "medium", "hard" };

            for (int i = 1; i <= 5; i++)
            {
                string advancedID = "advanced_" + i.ToString("00");

                foreach (string difficulty in difficulties)
                {
                    Dictionary<string, object> defaultAdvancedRecord = new Dictionary<string, object>
                {
                    {
                        "latest", new Dictionary<string, object>
                        {
                            { "time", 0f },
                            { "score", 0 },
                            { "process", 0 },
                            { "date", "" }
                        }
                    },
                    { "history", new List<object>() },
                    {
                        "best", new Dictionary<string, object>
                        {
                            { "time", 0f },
                            { "score", 0 },
                            { "process", 0 },
                            { "date", "" }
                        }
                    }
                };

                    initTasks.Add(userRef.Collection("learningRecords").Document("advanced")
                        .Collection(advancedID).Document(difficulty)
                        .SetAsync(defaultAdvancedRecord, SetOptions.MergeAll));
                }
            }

            Task.WhenAll(initTasks).ContinueWithOnMainThread(initTask =>
            {
                if (initTask.IsCompletedSuccessfully)
                {
                    Debug.Log("使用者完整資料已建立 / 補齊完成：" + finalUsername);
                }
                else
                {
                    Debug.LogWarning("使用者完整資料建立 / 補齊失敗：" + initTask.Exception);
                }
            });
        });
    }

    public void SaveUsername(string username)
    {
        if (!CheckReady()) return;

        if (string.IsNullOrWhiteSpace(username))
        {
            Debug.LogWarning("SaveUsername 失敗：名字是空的");
            return;
        }

        db.Collection("users").Document(userID)
          .SetAsync(new Dictionary<string, object>
          {
              { "username", username }
          }, SetOptions.MergeAll)
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsCompletedSuccessfully)
                  Debug.Log("Firebase 儲存 username 成功：" + username);
              else
                  Debug.LogWarning("Firebase 儲存 username 失敗：" + task.Exception);
          });
    }

    public void LoadUsername(System.Action<string> onLoaded)
    {
        if (!CheckReady())
        {
            onLoaded?.Invoke("user");
            return;
        }

        db.Collection("users").Document(userID)
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsFaulted || task.IsCanceled || !task.Result.Exists)
              {
                  onLoaded?.Invoke("user");
                  return;
              }

              string username = "user";

              if (task.Result.TryGetValue("username", out object nameObj))
                  username = nameObj.ToString();

              onLoaded?.Invoke(username);
          });
    }

    // =========================================================
    // 10. 成就
    // =========================================================

    public void SaveAchievementCompleted(string achievementID)
    {
        if (!CheckReady()) return;

        string date = DateTime.Now.ToString("yyyy/MM/dd");

        db.Collection("users").Document(userID)
          .Collection("achievements").Document("data")
          .SetAsync(new Dictionary<string, object>
          {
              { achievementID + "_completed", true },
              { achievementID + "_unlocked", true },
              { achievementID + "_date", date }
          }, SetOptions.MergeAll);
    }

    public void SaveAchievementUnlocked(string achievementID)
    {
        if (!CheckReady()) return;

        db.Collection("users").Document(userID)
          .Collection("achievements").Document("data")
          .SetAsync(new Dictionary<string, object>
          {
              { achievementID + "_unlocked", true }
          }, SetOptions.MergeAll);
    }

    public void LoadAchievements(System.Action<Dictionary<string, object>> onLoaded)
    {
        if (!CheckReady())
        {
            onLoaded?.Invoke(new Dictionary<string, object>());
            return;
        }

        db.Collection("users").Document(userID)
          .Collection("achievements").Document("data")
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsFaulted || task.IsCanceled || !task.Result.Exists)
              {
                  onLoaded?.Invoke(new Dictionary<string, object>());
                  return;
              }

              onLoaded?.Invoke(task.Result.ToDictionary());
          });
    }

    public void LoadAchievementCache(System.Action onDone = null)
    {
        if (!CheckReady())
        {
            AchievementLoaded = true;
            onDone?.Invoke();
            return;
        }

        db.Collection("users").Document(userID)
          .Collection("achievements").Document("data")
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              achievementCache = new Dictionary<string, object>();

              if (task.IsCompletedSuccessfully && task.Result.Exists)
              {
                  achievementCache = task.Result.ToDictionary();
              }

              AchievementLoaded = true;
              Debug.Log("成就資料已提前讀取，欄位數：" + achievementCache.Count);

              onDone?.Invoke();
          });
    }

    public Dictionary<string, object> GetAchievementCache()
    {
        return achievementCache;
    }

    public void CheckBasicAchievement(int levelNumber, System.Action<bool> onCompleted = null)
    {
        CheckAllBasicLearningAchievement(onCompleted);
    }

    public void CheckAllBasicLearningAchievement(System.Action<bool> onCompleted = null)
    {
        if (!CheckReady()) return;

        string achievementID = "achievement_basic_01";
        string rewardItemID = "Top05"; // 橘子皮痛衣，上衣5；如果你的 itemID 不是 Top05，這裡要改

        var videoRef = db.Collection("users").Document(userID)
                         .Collection("learning").Document("videos");

        var quizRef = db.Collection("users").Document(userID)
                        .Collection("learning").Document("quizzes");

        var achievementRef = db.Collection("users").Document(userID)
                               .Collection("achievements").Document("data");

        Task<DocumentSnapshot> videoTask = videoRef.GetSnapshotAsync();
        Task<DocumentSnapshot> quizTask = quizRef.GetSnapshotAsync();
        Task<DocumentSnapshot> achievementTask = achievementRef.GetSnapshotAsync();

        Task.WhenAll(videoTask, quizTask, achievementTask).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("檢查菜鳥新兵成就失敗：" + task.Exception);
                onCompleted?.Invoke(false);
                return;
            }

            bool alreadyCompleted =
                achievementTask.Result.Exists &&
                achievementTask.Result.ContainsField(achievementID + "_completed") &&
                achievementTask.Result.GetValue<bool>(achievementID + "_completed");

            if (alreadyCompleted)
            {
                SaveAchievementUnlocked("achievement_advanced_01");

                Debug.Log("【成就檢查】菜鳥新兵已完成，補解鎖菜鳥的逆襲");

                onCompleted?.Invoke(false);
                return;
            }

            bool allDone = true;

            for (int i = 1; i <= 5; i++)
            {
                string basicID = "basic_" + i.ToString("00");
                int videoProcess = 0;

                if (videoTask.Result.Exists &&
                    videoTask.Result.ContainsField(basicID + "_process"))
                {
                    videoProcess = Convert.ToInt32(
                        videoTask.Result.GetValue<object>(basicID + "_process")
                    );
                }

                bool videoDone = videoProcess >= 90;

                bool quizDone =
                    quizTask.Result.Exists &&
                    quizTask.Result.ContainsField(basicID + "_done") &&
                    quizTask.Result.GetValue<bool>(basicID + "_done");

                if (!videoDone || !quizDone)
                {
                    allDone = false;
                    break;
                }
            }

            if (!allDone)
            {
                onCompleted?.Invoke(false);
                return;
            }

            SaveAchievementCompleted(achievementID);
            SaveAchievementUnlocked("achievement_advanced_01");
            SaveWardrobeItem(rewardItemID, true);

            PlayerPrefs.SetInt("ShowAchievementToast", 1);
            PlayerPrefs.SetString("ToastTitle", "成就達成！");
            PlayerPrefs.SetString("ToastDesc", "菜鳥新兵，報到！");
            PlayerPrefs.Save();

            LoadAchievementCache(() =>
            {
                onCompleted?.Invoke(true);
            });

            Debug.Log("【成就完成】菜鳥新兵，報到！已解鎖橘子皮痛衣：" + rewardItemID);
        });
    }
    public void CheckAdvancedLevelAchievement(System.Action<bool> onCompleted = null)
    {
        if (!CheckReady()) return;

        string achievementID = "achievement_advanced_01";
        string rewardItemID = "Bottom01";

        var achievementRef = db.Collection("users").Document(userID)
            .Collection("achievements").Document("data");

        achievementRef.GetSnapshotAsync().ContinueWithOnMainThread(achievementTask =>
        {
            if (achievementTask.IsFaulted || achievementTask.IsCanceled)
            {
                onCompleted?.Invoke(false);
                return;
            }

            bool alreadyCompleted =
                achievementTask.Result.Exists &&
                achievementTask.Result.ContainsField(achievementID + "_completed") &&
                achievementTask.Result.GetValue<bool>(achievementID + "_completed");

            if (alreadyCompleted)
            {
                SaveAchievementUnlocked("achievement_quest_01");

                Debug.Log("【成就檢查】菜鳥的逆襲已完成，補解鎖大秘寶成就");

                onCompleted?.Invoke(false);
                return;
            }

            List<Task<DocumentSnapshot>> tasks = new List<Task<DocumentSnapshot>>();

            for (int i = 1; i <= 5; i++)
            {
                string advancedID = "advanced_" + i.ToString("00");

                tasks.Add(db.Collection("users").Document(userID)
                    .Collection("learningRecords").Document("advanced")
                    .Collection(advancedID).Document("easy")
                    .GetSnapshotAsync());

                tasks.Add(db.Collection("users").Document(userID)
                    .Collection("learningRecords").Document("advanced")
                    .Collection(advancedID).Document("medium")
                    .GetSnapshotAsync());
            }

            Task.WhenAll(tasks).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogWarning("檢查進階成就失敗：" + task.Exception);
                    onCompleted?.Invoke(false);
                    return;
                }

                bool allDone = true;

                foreach (var recordTask in tasks)
                {
                    if (!recordTask.Result.Exists)
                    {
                        allDone = false;
                        break;
                    }

                    Dictionary<string, object> data = recordTask.Result.ToDictionary();

                    if (!data.TryGetValue("latest", out object latestObj))
                    {
                        allDone = false;
                        break;
                    }

                    Dictionary<string, object> latest =
                        ConvertToDictionary(latestObj);

                    if (latest == null ||
                        GetDictInt(latest, "process") < 100)
                    {
                        allDone = false;
                        break;
                    }
                }

                if (!allDone)
                {
                    onCompleted?.Invoke(false);
                    return;
                }

                SaveAchievementCompleted(achievementID);
                SaveAchievementUnlocked("achievement_quest_01");
                SaveWardrobeItem(rewardItemID, true);

                PlayerPrefs.SetInt("ShowAchievementToast", 1);
                PlayerPrefs.SetString("ToastTitle", "成就達成！");
                PlayerPrefs.SetString("ToastDesc", "菜鳥的逆襲");
                PlayerPrefs.Save();

                LoadAchievementCache(() =>
                {
                    onCompleted?.Invoke(true);
                });

                Debug.Log("【成就完成】菜鳥的逆襲，已解鎖橘子皮痛褲：" + rewardItemID);
            });
        });
    }

        public void CheckQuestAllClearAchievement(System.Action<bool> onCompleted = null)
    {
        if (!CheckReady()) return;

        string achievementID = "achievement_quest_01";
        string rewardItemID = "Hair07";

        var achievementRef = db.Collection("users").Document(userID)
            .Collection("achievements").Document("data");

        var questRef = db.Collection("users").Document(userID)
            .Collection("learningRecords").Document("quest");

        achievementRef.GetSnapshotAsync().ContinueWithOnMainThread(achievementTask =>
        {
            if (achievementTask.IsFaulted || achievementTask.IsCanceled)
            {
                onCompleted?.Invoke(false);
                return;
            }

            bool alreadyCompleted =
                achievementTask.Result.Exists &&
                achievementTask.Result.ContainsField(achievementID + "_completed") &&
                achievementTask.Result.GetValue<bool>(achievementID + "_completed");

            if (alreadyCompleted)
            {
                onCompleted?.Invoke(false);
                return;
            }

            questRef.GetSnapshotAsync().ContinueWithOnMainThread(questTask =>
            {
                if (questTask.IsFaulted || questTask.IsCanceled || !questTask.Result.Exists)
                {
                    onCompleted?.Invoke(false);
                    return;
                }

                Dictionary<string, object> data = questTask.Result.ToDictionary();

                bool allDone = true;

                for (int i = 1; i <= 5; i++)
                {
                    string levelID = "Level" + i;
                    string bestKey = levelID + "_best";

                    if (!data.TryGetValue(bestKey, out object bestObj))
                    {
                        allDone = false;
                        break;
                    }

                    Dictionary<string, object> best = ConvertToDictionary(bestObj);

                    if (best == null || GetDictInt(best, "process") < 100)
                    {
                        allDone = false;
                        break;
                    }
                }

                if (!allDone)
                {
                    onCompleted?.Invoke(false);
                    return;
                }

                SaveAchievementCompleted(achievementID);
                SaveWardrobeItem(rewardItemID, true);

                PlayerPrefs.SetInt("ShowAchievementToast", 1);
                PlayerPrefs.SetString("ToastTitle", "成就達成！");
                PlayerPrefs.SetString("ToastDesc", "我把大秘寶都留在這了");
                PlayerPrefs.Save();

                LoadAchievementCache(() =>
                {
                    onCompleted?.Invoke(true);
                });

                Debug.Log("【成就完成】我把大秘寶都留在這了，已解鎖：" + rewardItemID);
            });
        });
    }
}

