using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Loading Settings")]
    // 預設的 Loading 場景名稱，請確保 Unity 裡有一個場景叫 "Loading"
    public string loadingSceneName = "Loading";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- 方法 A：直接切換 (不經過 Loading 畫面) ---
    public void GoTo(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"[SceneLoader] 無法切換！場景 '{sceneName}' 不存在或未加入 Build Settings。");
        }
    }

    // --- 方法 B：帶 Loading 畫面的異步切換 ---
    public void GoToWithLoading(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            StartCoroutine(LoadWithTransition(sceneName));
        }
        else
        {
            Debug.LogError($"[SceneLoader] 無法載入！場景 '{sceneName}' 不存在或未加入 Build Settings。");
        }
    }

    private IEnumerator LoadWithTransition(string targetScene)
    {
        // 1. 先載入 Loading 畫面
        SceneManager.LoadScene(loadingSceneName);
        yield return null; // 等一幀讓 Loading 場景初始化

        // 2. 開始異步載入目標場景
        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);

        if (op == null)
        {
            Debug.LogError($"[SceneLoader] AsyncOperation 失敗：場景 '{targetScene}' 載入異常。");
            yield break;
        }

        // 暫時不讓場景自動跳轉 (為了播完動畫或顯示進度)
        op.allowSceneActivation = false;

        // 3. 載入循環
        while (op.progress < 0.9f)
        {
            // 這裡可以傳進度值給 UI (例如 Slider.value = op.progress)
            yield return null;
        }

        // 4. 緩衝時間 (避免 Loading 閃太快，也可以等動畫播完)
        yield return new WaitForSeconds(0.5f);

        // 5. 正式進入場景
        op.allowSceneActivation = true;
    }
}