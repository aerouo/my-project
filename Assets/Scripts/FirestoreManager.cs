using UnityEngine;
using Firebase.Firestore;
using Firebase.Auth;
using System.Collections.Generic;
using System;

public class FirestoreManager : MonoBehaviour
{
    private static FirestoreManager _instance;
    public static FirestoreManager Instance { get { return _instance; } }

    private FirebaseFirestore db;
    private string userID;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        userID = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
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
          });
    }

    // ===== 練功坊小遊戲 =====
    public void SaveMinigame(string gameID, string level, float timeSeconds)
    {
        if (!CheckReady()) return;
        string date = DateTime.Now.ToString("yyyy/MM/dd");

        var docRef = db.Collection("users").Document(userID)
                       .Collection("minigames").Document(gameID);

        // 讀取目前資料，更新 history
        docRef.GetSnapshotAsync().ContinueWith(task =>
        {
            var updates = new Dictionary<string, object>
            {
                { level + "_lastTime", timeSeconds },
                { level + "_lastDate", date }
            };

            // 取得舊 history
            List<object> history = new List<object>();
            if (task.IsCompleted && task.Result.Exists)
            {
                if (task.Result.TryGetValue(level + "_history", out object h))
                    history = h as List<object> ?? new List<object>();
            }

            // 加入新紀錄，最多保留5筆
            history.Add(new Dictionary<string, object>
            {
                { "time", timeSeconds },
                { "date", date }
            });
            if (history.Count > 5)
                history.RemoveAt(0);

            updates[level + "_history"] = history;

            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                docRef.SetAsync(updates, SetOptions.MergeAll));
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

    // ===== 金幣 =====
    public void AddCoins(int amount)
    {
        if (!CheckReady()) return;

        var userRef = db.Collection("users").Document(userID);
        userRef.GetSnapshotAsync().ContinueWith(task =>
        {
            long current = 0;
            if (task.IsCompleted && task.Result.Exists)
                if (task.Result.TryGetValue("coins", out object val))
                    current = (long)val;

            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                userRef.UpdateAsync("coins", current + amount));
        });
    }

    // ===== 衣櫃 =====
    public void SaveWardrobeItem(string itemID, bool owned)
    {
        if (!CheckReady()) return;

        db.Collection("users").Document(userID)
          .Collection("wardrobe").Document("items")
          .SetAsync(new Dictionary<string, object>
          {
              { itemID + "_owned", owned }
          }, SetOptions.MergeAll);
    }

    // ===== 初始化新使用者 =====
    public void InitNewUser(string username)
    {
        if (!CheckReady()) return;

        db.Collection("users").Document(userID)
          .SetAsync(new Dictionary<string, object>
          {
              { "username", username },
              { "coins", 0 }
          });
    }

    private bool CheckReady()
    {
        if (db == null || string.IsNullOrEmpty(userID))
        {
            Debug.LogWarning("FirestoreManager 還沒準備好！");
            return false;
        }
        return true;
    }
}