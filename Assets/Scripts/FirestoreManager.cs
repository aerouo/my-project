using UnityEngine;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FirestoreManager : MonoBehaviour
{
    private static FirestoreManager _instance;
    public static FirestoreManager Instance => _instance;

    private FirebaseFirestore db;
    private string userID;

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

    // ===== 闖關碼頭 =====
    public void SaveQuestLevel(string levelID, float timeSeconds)
    {
        if (!CheckReady()) return;

        string date = DateTime.Now.ToString("yyyy/MM/dd");

        db.Collection("users").Document(userID)
          .Collection("questLevels").Document(levelID)
          .SetAsync(new Dictionary<string, object>
          {
              { "completed", true },
              { "time", timeSeconds },
              { "date", date }
          }, SetOptions.MergeAll);
    }

    // ===== 練功坊小遊戲 =====
    public void SaveMinigame(string gameID, string level, float timeSeconds)
    {
        if (!CheckReady()) return;

        string date = DateTime.Now.ToString("yyyy/MM/dd");

        var docRef = db.Collection("users").Document(userID)
                       .Collection("minigames").Document(gameID);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("讀取小遊戲紀錄失敗：" + task.Exception);
                return;
            }

            var updates = new Dictionary<string, object>
            {
                { level + "_lastTime", timeSeconds },
                { level + "_lastDate", date }
            };

            List<object> history = new List<object>();

            if (task.Result.Exists && task.Result.TryGetValue(level + "_history", out object h))
                history = h as List<object> ?? new List<object>();

            history.Add(new Dictionary<string, object>
            {
                { "time", timeSeconds },
                { "date", date }
            });

            if (history.Count > 5)
                history.RemoveAt(0);

            updates[level + "_history"] = history;

            docRef.SetAsync(updates, SetOptions.MergeAll);
        });
    }

    // ===== 學習影片 =====
    public void SaveVideoWatched(string videoID)
    {
        if (!CheckReady()) return;

        string date = DateTime.Now.ToString("yyyy/MM/dd");

        db.Collection("users").Document(userID)
          .Collection("learning").Document("videos")
          .SetAsync(new Dictionary<string, object>
          {
              { videoID + "_watched", true },
              { videoID + "_date", date }
          }, SetOptions.MergeAll);
    }

    // ===== 學習測驗 =====
    public void SaveQuizDone(string quizID)
    {
        if (!CheckReady()) return;

        string date = DateTime.Now.ToString("yyyy/MM/dd");

        db.Collection("users").Document(userID)
          .Collection("learning").Document("quizzes")
          .SetAsync(new Dictionary<string, object>
          {
              { quizID + "_done", true },
              { quizID + "_date", date }
          }, SetOptions.MergeAll);
    }

    // ===== 紙鶴 / 金幣：增加 =====
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

    // ===== 紙鶴 / 金幣：直接設定 =====
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

    // 保留這個方法名，避免其他舊程式呼叫壞掉
    public void SaveClosetMoney(int coins)
    {
        SaveCoins(coins);
    }

    // ===== 衣櫃：購買狀態 =====
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

    // ===== 衣櫃：穿戴資料 =====
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

    // ===== 衣櫃：讀取資料 =====
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
            Debug.Log("coins 是否存在：" + userDict.ContainsKey("coins"));
            Debug.Log("items 數量：" + itemsDict.Count);
            Debug.Log("equipped 數量：" + equippedDict.Count);

            onLoaded?.Invoke(userDict, itemsDict, equippedDict);
        });
    }

    // ===== 初始化新使用者 =====
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
}