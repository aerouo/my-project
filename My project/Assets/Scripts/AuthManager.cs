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
            }
        });
    }

    public void OnClickLogin()
    {
        Debug.Log("登入按下");
        if (auth == null) { txtMessage.text = "Firebase 未準備好"; return; }

        string email = inputEmail.text.Trim();
        string password = inputPassword.text;

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("登入失敗: " + task.Exception);
                txtMessage.text = "登入失敗";
                return;
            }
            Debug.Log("登入成功！");
            SceneLoader.Instance.GoTo(SceneName.Home);
        });
    }

    public void OnClickRegister()
    {
        Debug.Log("註冊按下");
        if (auth == null) { txtMessage.text = "Firebase 未準備好"; return; }

        string email = inputEmail.text.Trim();
        string password = inputPassword.text;

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("註冊失敗: " + task.Exception);
                txtMessage.text = "註冊失敗";
                return;
            }
            Debug.Log("註冊成功！");
            txtMessage.text = "註冊成功！請登入";
        });
    }
}