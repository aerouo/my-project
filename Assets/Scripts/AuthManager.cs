using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class AuthManager : MonoBehaviour
{
    // =========================================================
    // 1. 面板設定
    // =========================================================
    [Header("Panels")]
    public GameObject loginPanel;       // 登入頁面
    public GameObject registerPanel;    // 註冊頁面

    // =========================================================
    // 2. 登入頁輸入欄位
    // =========================================================
    [Header("Login Input Fields")]
    public TMP_InputField loginEmail;       // 登入 Email
    public TMP_InputField loginPassword;    // 登入密碼

    // =========================================================
    // 3. 註冊頁輸入欄位
    // =========================================================
    [Header("Register Input Fields")]
    public TMP_InputField registerEmail;              // 註冊 Email
    public TMP_InputField registerPassword;           // 註冊密碼
    public TMP_InputField registerConfirmPassword;    // 確認密碼

    // =========================================================
    // 4. 訊息文字
    // =========================================================
    [Header("Messages")]
    public TMP_Text loginMessage;       // 登入頁訊息
    public TMP_Text registerMessage;    // 註冊頁訊息

    [Header("Titles")]
    public GameObject loginTitle;     // 登入標題
    public GameObject registerTitle;  // 註冊標題

    private FirebaseAuth auth;

    void Start()
    {
        // 一開始顯示登入頁
        ShowLoginPanel();

        // 檢查 Firebase 是否可用
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
                SetLoginMessage("Firebase 初始化失敗");
            }
        });
    }

    // =========================================================
    // 5. 顯示登入頁
    // 切回登入頁時，清空登入欄位與訊息
    // =========================================================
    public void ShowLoginPanel()
    {
        if (loginPanel != null)
            loginPanel.SetActive(true);

        if (registerPanel != null)
            registerPanel.SetActive(false);

        if (loginTitle != null)
            loginTitle.SetActive(true);

        if (registerTitle != null)
            registerTitle.SetActive(false);

        ClearLoginInputs();
        ClearMessages();
    }

    // =========================================================
    // 6. 顯示註冊頁
    // 切到註冊頁時，清空註冊欄位與訊息
    // =========================================================
    public void ShowRegisterPanel()
    {
        if (loginPanel != null)
            loginPanel.SetActive(false);

        if (registerPanel != null)
            registerPanel.SetActive(true);

        if (loginTitle != null)
            loginTitle.SetActive(false);

        if (registerTitle != null)
            registerTitle.SetActive(true);

        ClearRegisterInputs();
        ClearMessages();
    }

    // =========================================================
    // 7. 點擊登入
    // =========================================================
    public void OnClickLogin()
    {
        Debug.Log("登入按下");

        if (auth == null)
        {
            SetLoginMessage("Firebase 未準備好");
            return;
        }

        string email = loginEmail != null ? loginEmail.text.Trim() : "";
        string password = loginPassword != null ? loginPassword.text : "";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            SetLoginMessage("請輸入電子郵件與密碼");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("登入失敗: " + task.Exception);
                    SetLoginMessage("登入失敗");
                    return;
                }

                Debug.Log("登入成功！");

                // 登入成功後補齊 Firestore 使用者資料
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

    // =========================================================
    // 8. 點擊註冊
    // =========================================================
    public void OnClickRegister()
    {
        Debug.Log("註冊按下");

        if (auth == null)
        {
            SetRegisterMessage("Firebase 未準備好");
            return;
        }

        string email = registerEmail != null ? registerEmail.text.Trim() : "";
        string password = registerPassword != null ? registerPassword.text : "";
        string confirmPassword = registerConfirmPassword != null ? registerConfirmPassword.text : "";

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            SetRegisterMessage("請完整輸入註冊資料");
            return;
        }

        if (password != confirmPassword)
        {
            SetRegisterMessage("兩次密碼輸入不一致");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("註冊失敗: " + task.Exception);
                    SetRegisterMessage("註冊失敗");
                    return;
                }

                Debug.Log("註冊成功！");

                // 註冊成功後建立 Firestore 預設資料
                // username 傳 null，FirestoreManager 裡會自動變成 user
                if (FirestoreManager.Instance != null)
                {
                    FirestoreManager.Instance.InitNewUser(null);
                    Debug.Log("註冊後開始建立 Firestore 使用者資料");
                }
                else
                {
                    Debug.LogWarning("FirestoreManager.Instance 是空的，註冊後無法建立資料");
                }

                ClearRegisterInputs();

                // 切回登入頁
                ShowLoginPanel();

                // 因為 ShowLoginPanel 會清空訊息，所以成功訊息要放在它後面
                SetLoginMessage("註冊成功！請登入");
            });
    }

    // =========================================================
    // 9. 清空登入輸入欄位
    // =========================================================
    private void ClearLoginInputs()
    {
        if (loginEmail != null)
            loginEmail.text = "";

        if (loginPassword != null)
            loginPassword.text = "";

        if (loginEmail != null)
            loginEmail.DeactivateInputField();

        if (loginPassword != null)
            loginPassword.DeactivateInputField();
    }

    // =========================================================
    // 10. 清空註冊輸入欄位
    // =========================================================
    private void ClearRegisterInputs()
    {
        if (registerEmail != null)
            registerEmail.text = "";

        if (registerPassword != null)
            registerPassword.text = "";

        if (registerConfirmPassword != null)
            registerConfirmPassword.text = "";

        if (registerEmail != null)
            registerEmail.DeactivateInputField();

        if (registerPassword != null)
            registerPassword.DeactivateInputField();

        if (registerConfirmPassword != null)
            registerConfirmPassword.DeactivateInputField();
    }

    // =========================================================
    // 11. 清空訊息
    // =========================================================
    private void ClearMessages()
    {
        if (loginMessage != null)
            loginMessage.text = "";

        if (registerMessage != null)
            registerMessage.text = "";
    }

    // =========================================================
    // 12. 設定登入頁訊息
    // =========================================================
    private void SetLoginMessage(string message)
    {
        if (loginMessage != null)
            loginMessage.text = message;
    }

    // =========================================================
    // 13. 設定註冊頁訊息
    // =========================================================
    private void SetRegisterMessage(string message)
    {
        if (registerMessage != null)
            registerMessage.text = message;
    }
}