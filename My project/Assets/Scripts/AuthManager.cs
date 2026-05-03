using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField inputEmail;
    public TMP_InputField inputPassword;

    [Header("Message")]
    public TMP_Text txtMessage;

    private FirebaseAuth auth;
    private bool isFirebaseReady = false;

    void Start()
    {
        // 1. 必須先檢查 Firebase 依賴環境
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                isFirebaseReady = true;
                Debug.Log("Firebase 準備就緒");
            }
            else
            {
                Debug.LogError($"無法初始化 Firebase: {dependencyStatus}");
            }
        });
    }

    public void OnClickLogin()
    {
        if (!isFirebaseReady) return;

        string email = inputEmail.text.Trim();
        string password = inputPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            txtMessage.text = "請填寫帳號和密碼";
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            // 將所有結果處理丟回主執行緒
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                if (task.IsCanceled) return;

                if (task.IsFaulted)
                {
                    // 印出具體錯誤原因（檢查 Console）
                    Debug.LogError("登入錯誤: " + task.Exception.Flatten().InnerExceptions[0].Message);
                    txtMessage.text = "登入失敗，請確認網路或帳密";
                    return;
                }

                AuthResult result = task.Result;
                Debug.LogFormat("用戶登入成功: {0} ({1})", result.User.DisplayName, result.User.UserId);

                // 跳轉場景
                SceneLoader.Instance.GoTo(SceneName.Home);
            });
        });
    }

    public void OnClickRegister()
    {
        if (!isFirebaseReady) return;

        string email = inputEmail.text.Trim();
        string password = inputPassword.text;

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("註冊錯誤: " + task.Exception.Flatten().InnerExceptions[0].Message);
                    txtMessage.text = "註冊失敗：" + task.Exception.Flatten().InnerExceptions[0].Message;
                    return;
                }

                txtMessage.text = "註冊成功！請登入";
            });
        });
    }
}