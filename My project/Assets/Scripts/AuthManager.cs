using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;

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
        auth = FirebaseAuth.DefaultInstance;
    }

    // 登入按鈕呼叫
    public void OnClickLogin()
    {
        string email = inputEmail.text.Trim();
        string password = inputPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            txtMessage.text = "請填寫帳號和密碼";
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                txtMessage.text = "登入失敗，請確認帳號密碼";
                return;
            }
            // 登入成功 → 跳到遊戲大廳
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                SceneLoader.Instance.GoTo(SceneName.Home);
            });
        });
    }

    // 註冊按鈕呼叫
    public void OnClickRegister()
    {
        string email = inputEmail.text.Trim();
        string password = inputPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            txtMessage.text = "請填寫帳號和密碼";
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                txtMessage.text = "註冊失敗，請換一個帳號";
                return;
            }
            txtMessage.text = "註冊成功！請登入";
        });
    }
}