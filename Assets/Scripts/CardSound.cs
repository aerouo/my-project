using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// 卡片音效控制
/// 功能：
/// 1. 滑鼠移到卡片上時播放 Hover 音效
/// 2. 點擊卡片時播放 Click 音效
/// 3. 可搭配延遲切換場景，避免點擊音效被切 Scene 中斷
/// </summary>
public class CardSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("音效")]
    public AudioClip hoverSound;    // 滑鼠移入音效
    public AudioClip clickSound;    // 點擊音效

    [Header("音量")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("切換場景設定")]
    public bool useDelayLoadScene = false;   // 是否使用延遲切場景
    public string targetSceneName = "";      // 要切換的場景名稱
    public float loadDelay = 0.15f;           // 延遲秒數，讓音效有時間播放

    private AudioSource audioSource;
    private bool isLoadingScene = false;

    void Awake()
    {
        // 取得或自動新增 AudioSource
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // UI 音效不需要一開始自動播放
        audioSource.playOnAwake = false;

        // UI 音效用 2D 聲音
        audioSource.spatialBlend = 0f;
    }

    /// <summary>
    /// 滑鼠移入卡片時播放 Hover 音效
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHover();
    }

    /// <summary>
    /// 點擊卡片時播放 Click 音效
    /// 如果有設定延遲切場景，會播放音效後再切場景
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (useDelayLoadScene && !string.IsNullOrEmpty(targetSceneName))
        {
            PlayClickAndLoadScene(targetSceneName);
        }
        else
        {
            PlayClick();
        }
    }

    /// <summary>
    /// 給外部程式或 Button OnClick 呼叫：播放 Hover 音效
    /// </summary>
    public void PlayHover()
    {
        PlaySound(hoverSound);
    }

    /// <summary>
    /// 給外部程式或 Button OnClick 呼叫：播放 Click 音效
    /// </summary>
    public void PlayClick()
    {
        PlaySound(clickSound);
    }

    /// <summary>
    /// 播放點擊音效後，延遲切換場景
    /// 可在 Button OnClick 中直接呼叫，並填入場景名稱
    /// </summary>
    public void PlayClickAndLoadScene(string sceneName)
    {
        if (isLoadingScene)
            return;

        StartCoroutine(LoadSceneAfterClick(sceneName));
    }

    /// <summary>
    /// 延遲切換場景流程
    /// </summary>
    private IEnumerator LoadSceneAfterClick(string sceneName)
    {
        isLoadingScene = true;

        PlayClick();

        yield return new WaitForSeconds(loadDelay);

        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 實際播放音效
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null)
            return;

        if (!gameObject.activeInHierarchy || !audioSource.enabled)
            return;

        audioSource.PlayOneShot(clip, volume);
    }
}