using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    [Header("所有成就節點")]
    public AchievementNodeTooltip[] achievementNodes;

    [Header("Toast")]
    public GameObject toastPanel;
    public TMP_Text txtToastTitle;
    public TMP_Text txtToastDesc;

    [Header("Toast Animation")]
    public RectTransform toastRect;

    [Header("Map Content")]
    public GameObject achievementContent;

    public Vector2 hiddenPos = new Vector2(1140, 340);
    public Vector2 showPos = new Vector2(800, 340);

    public float slideSpeed = 8f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // 讓最外層 Canvas 常駐
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(transform.root.gameObject);
        }
    }

    void Start()
    {
        if (toastPanel != null)
        {
            toastPanel.SetActive(true);
            toastRect.anchoredPosition = hiddenPos;
        }

        if (FirestoreManager.Instance != null && FirestoreManager.Instance.AchievementLoaded)
        {
            ApplyAchievementData(FirestoreManager.Instance.GetAchievementCache());
        }
        else
        {
            LoadAchievementStates();
        }
        CheckPendingToast();
    }

    // ===== 從 Firestore 載入成就狀態 =====
    public void LoadAchievementStates()
    {
        if (FirestoreManager.Instance == null)
            return;

        FirestoreManager.Instance.LoadAchievements((data) =>
        {
            foreach (AchievementNodeTooltip node in achievementNodes)
            {
                bool unlocked = false;
                bool completed = false;

                if (data.ContainsKey(node.achievementID + "_unlocked"))
                    unlocked = (bool)data[node.achievementID + "_unlocked"];

                if (data.ContainsKey(node.achievementID + "_completed"))
                    completed = (bool)data[node.achievementID + "_completed"];

                if (node.achievementID == "achievement_basic_01")
                    unlocked = true;

                node.SetAchievementState(unlocked, completed);
            }

            // Firebase 讀完、狀態更新完，才顯示
            foreach (AchievementNodeTooltip node in achievementNodes)
            {
                node.gameObject.SetActive(true);
            }
        });
    }

    public void ShowAchievementToast(string title, string desc)
    {
        StartCoroutine(ShowToastRoutine(title, desc));
    }

    IEnumerator DelayedToast(string title, string desc)
    {
        yield return new WaitForSeconds(1f);

        ShowAchievementToast(title, desc);
    }

    // ===== Toast 顯示流程 =====
    IEnumerator ShowToastRoutine(string title, string desc)
    {
        txtToastTitle.text = title;
        txtToastDesc.text = desc;

        // ===== 滑入 =====
        while (Vector2.Distance(toastRect.anchoredPosition, showPos) > 1f)
        {
            toastRect.anchoredPosition =
                Vector2.Lerp(
                    toastRect.anchoredPosition,
                    showPos,
                    Time.deltaTime * slideSpeed
                );

            yield return null;
        }

        toastRect.anchoredPosition = showPos;

        // 停留
        yield return new WaitForSeconds(3f);

        // ===== 滑出 =====
        while (Vector2.Distance(toastRect.anchoredPosition, hiddenPos) > 1f)
        {
            toastRect.anchoredPosition =
                Vector2.Lerp(
                    toastRect.anchoredPosition,
                    hiddenPos,
                    Time.deltaTime * slideSpeed
                );

            yield return null;
        }

        toastRect.anchoredPosition = hiddenPos;
    }

    public void ApplyAchievementData(Dictionary<string, object> data)
    {
        foreach (AchievementNodeTooltip node in achievementNodes)
        {
            bool unlocked = false;
            bool completed = false;

            if (data.ContainsKey(node.achievementID + "_unlocked"))
                unlocked = (bool)data[node.achievementID + "_unlocked"];

            if (data.ContainsKey(node.achievementID + "_completed"))
                completed = (bool)data[node.achievementID + "_completed"];

            if (node.achievementID == "achievement_basic_01")
                unlocked = true;

            node.SetAchievementState(unlocked, completed);
        }
    }

    void CheckPendingToast()
    {
        if (PlayerPrefs.GetInt("ShowAchievementToast", 0) == 1)
        {
            PlayerPrefs.DeleteKey("ShowAchievementToast");

            string title = PlayerPrefs.GetString("ToastTitle", "成就達成！");
            string desc = PlayerPrefs.GetString("ToastDesc", "");

            PlayerPrefs.DeleteKey("ToastTitle");
            PlayerPrefs.DeleteKey("ToastDesc");

            RefreshAchievements();

            StartCoroutine(DelayedToast(title, desc));
            
        }
    }

    public void RefreshAchievements()
    {
        achievementNodes = FindObjectsByType<AchievementNodeTooltip>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        LoadAchievementStates();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckPendingToast();
    }

    public void TestToast()
    {
        ShowAchievementToast("成就達成！", "新手上路");
    }
}