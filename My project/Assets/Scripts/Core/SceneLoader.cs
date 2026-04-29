using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Loading Screen")]
    public string loadingSceneName = SceneName.Loading;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 直接切換（不過場）
    public void GoTo(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 帶 Loading 畫面切換
    public void GoToWithLoading(string sceneName)
    {
        StartCoroutine(LoadWithTransition(sceneName));
    }

    IEnumerator LoadWithTransition(string targetScene)
    {
        SceneManager.LoadScene(loadingSceneName);
        yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f) yield return null;

        // 這裡可以通知 LoadingScreen UI 播完動畫
        yield return new WaitForSeconds(0.5f);
        op.allowSceneActivation = true;
    }
}