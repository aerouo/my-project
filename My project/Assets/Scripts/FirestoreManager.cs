using UnityEngine;
using Firebase.Firestore;
using Firebase.Auth;
using System.Collections.Generic;
using System;

public class FirestoreManager : MonoBehaviour
{
    private static FirestoreManager _instance;
    public static FirestoreManager Instance
    {
        get { return _instance; }
    }

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

    // 存闖關碼頭關卡資料
    public void SaveQuestLevel(string levelID, float timeSeconds)
    {
        if (string.IsNullOrEmpty(userID)) return;

        string date = DateTime.Now.ToString("yyyy/MM/dd");

        var data = new Dictionary<string, object>
        {
            { "completed", true },
            { "time", timeSeconds },
            { "date", date }
        };

        db.Collection("users").Document(userID)
          .Collection("questLevels").Document(levelID)
          .SetAsync(data);
    }

    // 存練功坊小遊戲資料
    public void SaveMinigame(string gameID, string level, float timeSeconds)
    {
        if (string.IsNullOrEmpty(userID)) return;

        string date = DateTime.Now.ToString("yyyy/MM/dd");

        var record = new Dictionary<string, object>
        {
            { "time", timeSeconds },
            { "date", date }
        };

        // 存最新紀錄
        db.Collection("users").Document(userID)
          .Collection("minigames").Document(gameID)
          .Collection(level).Document("latest")
          .SetAsync(record);

        // 存到 history（新增一筆）
        db.Collection("users").Document(userID)
          .Collection("minigames").Document(gameID)
          .Collection(level).Document("history")
          .Collection("records").AddAsync(record);
    }

    // 加金幣
    public void AddCoins(int amount)
    {
        if (string.IsNullOrEmpty(userID)) return;

        var userRef = db.Collection("users").Document(userID);
        userRef.GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                long currentCoins = 0;
                if (task.Result.TryGetValue("coins", out object val))
                    currentCoins = (long)val;

                userRef.UpdateAsync("coins", currentCoins + amount);
            }
        });
    }
}