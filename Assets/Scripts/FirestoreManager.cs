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
    }

    private bool CheckReady()
    {
        RefreshUser();

        if (db == null || string.IsNullOrEmpty(userID))
        {
            Debug.LogWarning("FirestoreManager 還沒準備好！");
            return false;
        }

        return true;
    }

    // =========================================================
    // 1. 學習紀錄：共用工具
    // =========================================================

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

    private void SaveRecordWithHistory(DocumentReference docRef, Dictionary<string, object> latestRecord)
    {
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("讀取紀錄失敗：" + task.Exception);
                return;
            }

            List<object> history = new List<object>();

            if (task.Result.Exists)
            {
                Dictionary<string, object> oldData = task.Result.ToDictionary();

                if (oldData.TryGetValue("latest", out object oldLatestObj))
                {
                    history.Insert(0, oldLatestObj);
                }

                if (oldData.TryGetValue("history", out object oldHistoryObj))
                {
                    List<object> oldHistory = ConvertToObjectList(oldHistoryObj);
                    history.AddRange(oldHistory);
                }
            }

            if (history.Count > 5)
                history.RemoveRange(5, history.Count - 5);

            Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "latest", latestRecord },
            { "history", history }
        };

            docRef.SetAsync(updates, SetOptions.MergeAll).ContinueWithOnMainThread(saveTask =>
            {
                if (saveTask.IsCompletedSuccessfully)
                    Debug.Log("學習紀錄儲存成功");
                else
                    Debug.LogWarning("學習紀錄儲存失敗：" + saveTask.Exception);
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

    public void SaveBasicVideoRecord(string basicID, float timeSeconds = 0f)
    {
        if (!CheckReady()) return;

        string date = DateTime.Now.ToString("yyyy/MM/dd");

        db.Collection("users").Document(userID)
          .Collection("learning").Document("videos")
          .SetAsync(new Dictionary<string, object>
          {
          { basicID + "_watched", true },
          { basicID + "_date", date },
          { basicID + "_process", 100 },
          { basicID + "_time", timeSeconds }
          }, SetOptions.MergeAll)
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsCompletedSuccessfully)
                  Debug.Log("基礎影片紀錄儲存成功：" + basicID);
              else
                  Debug.LogWarning("基礎影片紀錄儲存失敗：" + task.Exception);
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

              if (!data.ContainsKey(basicID + "_watched"))
              {
                  onLoaded?.Invoke(new Dictionary<string, object>());
                  return;
              }

              bool watched = Convert.ToBoolean(data[basicID + "_watched"]);

              int process = 0;

              if (data.ContainsKey(basicID + "_process"))
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

        SaveRecordWithHistory(docRef, latestRecord);
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

            if (task.Result.Exists)
            {
                Dictionary<string, object> oldData = task.Result.ToDictionary();

                if (oldData.TryGetValue(levelID + "_latest", out object oldLatest))
                    history.Insert(0, oldLatest);

                if (oldData.TryGetValue(levelID + "_history", out object oldHistory))
                    history.AddRange(ConvertToObjectList(oldHistory));
            }

            if (history.Count > 5)
                history.RemoveRange(5, history.Count - 5);

            Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { levelID + "_latest", latestRecord },
            { levelID + "_history", history }
        };

            docRef.SetAsync(updates, SetOptions.MergeAll).ContinueWithOnMainThread(saveTask =>
            {
                if (saveTask.IsCompletedSuccessfully)
                    Debug.Log("闖關紀錄儲存成功：" + levelID);
                else
                    Debug.LogWarning("闖關紀錄儲存失敗：" + saveTask.Exception);
            });
        });
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

        db.Collection("users").Document(userID)
          .SetAsync(new Dictionary<string, object>
          {
              { "username", username },
              { "coins", 10000 }
          }, SetOptions.MergeAll);
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
        if (!CheckReady()) return;

        string levelID = "basic_" + levelNumber.ToString("00");
        string achievementID = "achievement_basic_" + levelNumber.ToString("00");
        string nextAchievementID = "achievement_basic_" + (levelNumber + 1).ToString("00");

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
                Debug.LogWarning("檢查成就失敗：" + task.Exception);
                onCompleted?.Invoke(false);
                return;
            }

            bool videoDone =
                videoTask.Result.Exists &&
                videoTask.Result.ContainsField(levelID + "_watched") &&
                videoTask.Result.GetValue<bool>(levelID + "_watched");

            bool quizDone =
                quizTask.Result.Exists &&
                quizTask.Result.ContainsField(levelID + "_done") &&
                quizTask.Result.GetValue<bool>(levelID + "_done");

            bool alreadyCompleted =
                achievementTask.Result.Exists &&
                achievementTask.Result.ContainsField(achievementID + "_completed") &&
                achievementTask.Result.GetValue<bool>(achievementID + "_completed");

            if (alreadyCompleted)
            {
                onCompleted?.Invoke(false);
                return;
            }

            if (videoDone && quizDone)
            {
                SaveAchievementCompleted(achievementID);

                if (levelNumber < 5)
                    SaveAchievementUnlocked(nextAchievementID);

                LoadAchievementCache(() =>
                {
                    onCompleted?.Invoke(true);
                });
            }
            else
            {
                onCompleted?.Invoke(false);
            }
        });
    }
}