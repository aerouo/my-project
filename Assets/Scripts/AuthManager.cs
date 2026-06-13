using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class AuthManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField inputEmail;
    public TMP_InputField inputPassword;

    [Header("Message")]
    public TMP_Text txtMessage;

    private FirebaseAuth auth;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase 準備就緒");
            }
            else
            {
                Debug.LogError("Firebase 初始化失敗: " + task.Result);
                txtMessage.text = "Firebase 初始化失敗";
            }
        });
    }

    public void OnClickLogin()
    {
        Debug.Log("登入按下");

        if (auth == null)
        {
            txtMessage.text = "Firebase 未準備好";
            return;
        }

        string email = inputEmail.text.Trim();
        string password = inputPassword.text;

        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("登入失敗: " + task.Exception);
                    txtMessage.text = "登入失敗";
                    return;
                }

                Debug.Log("登入成功！");

                // 保險：登入時也補一次資料
                if (FirestoreManager.Instance != null)
                {
                    FirestoreManager.Instance.InitNewUser(null);
                }
                else
                {
                    Debug.LogWarning("FirestoreManager.Instance 是空的，登入後無法補齊資料");
                }

                SceneLoader.Instance.GoTo(SceneName.Home);
            });
    }

    public void OnClickRegister()
    {
        Debug.Log("註冊按下");

        if (auth == null)
        {
            txtMessage.text = "Firebase 未準備好";
            return;
        }

        string email = inputEmail.text.Trim();
        string password = inputPassword.text;

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("註冊失敗: " + task.Exception);
                    txtMessage.text = "註冊失敗";
                    return;
                }

                Debug.Log("註冊成功！");

                if (FirestoreManager.Instance != null)
                {
                    FirestoreManager.Instance.InitNewUser(null);
                    Debug.Log("註冊後開始建立 Firestore 使用者資料");
                }
                else
                {
                    Debug.LogWarning("FirestoreManager.Instance 是空的，註冊後無法建立資料");
                }

                // 清空輸入框
                inputEmail.text = "";
                inputPassword.text = "";

                txtMessage.text = "註冊成功！請登入";
            });
    }
}