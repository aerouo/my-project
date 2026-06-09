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

    [Header("成就連線")]
    public AchievementWireEffect newbieToAdvancedWire;
    public AchievementWireEffect advancedToQuestWire;

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
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(transform.root.gameObject);
        }
    }

    void Start()
    {
        if (toastPanel != null && toastRect != null)
        {
            toastPanel.SetActive(true);
            toastRect.anchoredPosition = hiddenPos;
        }

        RefreshNodeList();

        if (FirestoreManager.Instance != null && FirestoreManager.Instance.AchievementLoaded)
        {
            ApplyAchievementData(FirestoreManager.Instance.GetAchievementCache());
        }
        else
        {
            LoadAchievementStates();
        }

        CheckPendingToast();
        CheckBasicLearningAchievementOnStart();
        CheckAdvancedAchievementOnStart();
    }

    void CheckBasicLearningAchievementOnStart()
    {
        if (FirestoreManager.Instance == null) return;

        FirestoreManager.Instance.CheckBasicAchievement(1, completed =>
        {
            Debug.Log("【AchievementManager】菜鳥新兵成就檢查結果：" + completed);

            if (completed)
                RefreshAchievements();
            else
                LoadAchievementStates();
        });
    }

    void CheckAdvancedAchievementOnStart()
    {
        if (FirestoreManager.Instance == null) return;

        FirestoreManager.Instance.CheckAdvancedLevelAchievement(completed =>
        {
            Debug.Log("【AchievementManager】菜鳥的逆襲成就檢查結果：" + completed);
            LoadAchievementStates();
        });
    }

    public void LoadAchievementStates()
    {
        if (FirestoreManager.Instance == null) return;

        RefreshNodeList();

        FirestoreManager.Instance.LoadAchievements(data =>
        {
            ApplyAchievementData(data);
        });
    }

    public void ApplyAchievementData(Dictionary<string, object> data)
    {
        RefreshNodeList();

        foreach (AchievementNodeTooltip node in achievementNodes)
        {
            if (node == null) continue;

            bool unlocked = false;
            bool completed = false;

            if (data.ContainsKey(node.achievementID + "_unlocked"))
                unlocked = (bool)data[node.achievementID + "_unlocked"];

            if (data.ContainsKey(node.achievementID + "_completed"))
                completed = (bool)data[node.achievementID + "_completed"];

            if (node.achievementID == "achievement_basic_01")
                unlocked = true;

            node.SetAchievementState(unlocked, completed);
            node.gameObject.SetActive(true);
        }

        RefreshAchievementWires();
    }

    void RefreshAchievementWires()
    {
        bool basicCompleted = false;
        bool advancedCompleted = false;

        foreach (AchievementNodeTooltip node in achievementNodes)
        {
            if (node == null) continue;

            if (node.achievementID == "achievement_basic_01")
                basicCompleted = node.completed;

            if (node.achievementID == "achievement_advanced_01")
                advancedCompleted = node.completed;
        }

        if (newbieToAdvancedWire != null)
            newbieToAdvancedWire.SetPowered(basicCompleted);

        if (advancedToQuestWire != null)
            advancedToQuestWire.SetPowered(advancedCompleted);
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

    IEnumerator ShowToastRoutine(string title, string desc)
    {
        if (toastRect == null) yield break;

        if (txtToastTitle != null)
            txtToastTitle.text = title;

        if (txtToastDesc != null)
            txtToastDesc.text = desc;

        while (Vector2.Distance(toastRect.anchoredPosition, showPos) > 1f)
        {
            toastRect.anchoredPosition =
                Vector2.Lerp(toastRect.anchoredPosition, showPos, Time.deltaTime * slideSpeed);

            yield return null;
        }

        toastRect.anchoredPosition = showPos;

        yield return new WaitForSeconds(3f);

        while (Vector2.Distance(toastRect.anchoredPosition, hiddenPos) > 1f)
        {
            toastRect.anchoredPosition =
                Vector2.Lerp(toastRect.anchoredPosition, hiddenPos, Time.deltaTime * slideSpeed);

            yield return null;
        }

        toastRect.anchoredPosition = hiddenPos;
    }

    void CheckPendingToast()
    {
        if (PlayerPrefs.GetInt("ShowAchievementToast", 0) == 1)
        {
            PlayerPrefs.DeleteKey("ShowAchievementToast");

            string title = PlayerPrefs.GetString("ToastTitle", "成就達成！");
            string desc = PlayerPrefs.GetString("ToastDesc", "菜鳥新兵，報到！");

            PlayerPrefs.DeleteKey("ToastTitle");
            PlayerPrefs.DeleteKey("ToastDesc");

            RefreshAchievements();
            StartCoroutine(DelayedToast(title, desc));
        }
    }

    public void RefreshAchievements()
    {
        RefreshNodeList();
        LoadAchievementStates();
    }

    void RefreshNodeList()
    {
        achievementNodes = FindObjectsByType<AchievementNodeTooltip>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
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
        RefreshNodeList();
        CheckPendingToast();

        if (FirestoreManager.Instance != null && FirestoreManager.Instance.AchievementLoaded)
            ApplyAchievementData(FirestoreManager.Instance.GetAchievementCache());
    }

    public void TestToast()
    {
        ShowAchievementToast("成就達成！", "菜鳥新兵，報到！");
    }
}