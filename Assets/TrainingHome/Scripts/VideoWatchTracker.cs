using UnityEngine;
using UnityEngine.Video;

public class VideoWatchTracker : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;

    [Header("Learning ID")]
    public string videoID = "basic_01";

    [Header("Complete Setting")]
    [Range(0.1f, 1f)]
    public double completeRate = 0.9;

    [Header("Anti Skip Setting")]
    public double maxValidTimeJump = 5.0;

    [Header("Save Setting")]
    [Tooltip("Firebase 每幾%儲存一次，建議 5。")]
    public int saveStepPercent = 5;

    [Header("Debug")]
    public bool showDebugLog = true;

    [Tooltip("播放過程中每增加 1% 是否顯示 Console 訊息")]
    public bool logEveryPercent = true;

    private bool completedSaved = false;
    private float watchedSeconds = 0f;
    private double lastVideoTime = 0;
    private bool hasStartedTracking = false;

    private int lastSavedPercent = 0; // Firebase 儲存用
    private int lastLoggedPercent = 0; // Console 顯示用

    void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
    }

    void OnEnable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnDisable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    void Start()
    {
        ResetTracking();
    }

    void Update()
    {
        if (videoPlayer == null) return;
        if (videoPlayer.clip == null) return;
        if (videoPlayer.length <= 0) return;

        TrackWatchingProgress();
    }

    void TrackWatchingProgress()
    {
        if (!videoPlayer.isPlaying)
            return;

        double currentTime = videoPlayer.time;

        if (!hasStartedTracking)
        {
            lastVideoTime = currentTime;
            hasStartedTracking = true;
            DebugLog("開始追蹤影片：" + videoID);
            return;
        }

        double timeDiff = currentTime - lastVideoTime;

        if (timeDiff > 0 && timeDiff <= maxValidTimeJump)
        {
            watchedSeconds += (float)timeDiff;
        }
        else if (timeDiff > maxValidTimeJump)
        {
            DebugLog("偵測到拖拉影片，不累積秒數：" + timeDiff);
        }

        lastVideoTime = currentTime;

        int percent = GetRealWatchPercent();

        // Console 1% 一次顯示，不代表每 1% 都寫 Firebase
        if (logEveryPercent && percent > lastLoggedPercent)
        {
            DebugLog(videoID + " 目前觀看進度 = " + percent + "%");
            lastLoggedPercent = percent;
        }

        // Firebase 仍然依 saveStepPercent 儲存，避免寫太頻繁
        if (percent >= lastSavedPercent + saveStepPercent)
        {
            SaveProgress(percent);
            lastSavedPercent = percent;
        }

        // 超過 90% 後標記完成，但 process 保留實際百分比
        if (!completedSaved && GetRealWatchRate() >= completeRate)
        {
            SaveCompleted(percent);
        }
    }
    void OnVideoFinished(VideoPlayer vp)
    {
        watchedSeconds = (float)videoPlayer.length;

        SaveProgress(100);
        lastSavedPercent = 100;

        if (!completedSaved)
            SaveCompleted(100);

        DebugLog(videoID + " 影片自然播放結束，進度 = 100%");
    }

    public int ForceSaveProgress()
    {
        int percent = GetRealWatchPercent();

        Debug.Log("[VideoWatchTracker] 關閉影片：" + videoID + "，本次觀看進度 = " + percent + "%");

        if (percent <= 0)
            return percent;

        // 關閉時補存目前實際進度，即使未達 5%
        if (percent > lastSavedPercent)
        {
            SaveProgress(percent);
            lastSavedPercent = percent;
        }

        if (!completedSaved && GetRealWatchRate() >= completeRate)
        {
            SaveCompleted(percent);
        }

        return percent;
    }

    double GetRealWatchRate()
    {
        if (videoPlayer == null || videoPlayer.length <= 0)
            return 0;

        return watchedSeconds / videoPlayer.length;
    }

    int GetRealWatchPercent()
    {
        return Mathf.Clamp(
            Mathf.FloorToInt((float)(GetRealWatchRate() * 100)),
            0,
            100
        );
    }

    void SaveProgress(int percent)
    {
        if (FirestoreManager.Instance == null)
        {
            Debug.LogWarning("FirestoreManager.Instance 是 null，無法儲存影片進度：" + videoID);
            return;
        }

        FirestoreManager.Instance.SaveBasicVideoProgress(videoID, percent, watchedSeconds);
        DebugLog("背後儲存影片進度：" + videoID + " = " + percent + "%");
    }

    void SaveCompleted(int percent)
    {
        if (completedSaved)
            return;

        completedSaved = true;

        if (FirestoreManager.Instance == null)
        {
            Debug.LogWarning("FirestoreManager.Instance 是 null，影片無法儲存完成：" + videoID);
            return;
        }

        FirestoreManager.Instance.SaveBasicVideoRecord(videoID, watchedSeconds, percent);
        FirestoreManager.Instance.CheckBasicAchievement(GetLevelNumber(videoID));

        Debug.Log("影片觀看達標並儲存：" + videoID + " / " + percent + "%");
    }

    public void ResetTracking()
    {
        completedSaved = false;
        watchedSeconds = 0f;
        lastVideoTime = 0;
        hasStartedTracking = false;

        lastSavedPercent = 0;
        lastLoggedPercent = 0;

        DebugLog("重置影片觀看進度：" + videoID);
    }

    int GetLevelNumber(string id)
    {
        string[] parts = id.Split('_');

        if (parts.Length >= 2 && int.TryParse(parts[1], out int number))
            return number;

        return 1;
    }

    void DebugLog(string message)
    {
        if (showDebugLog)
            Debug.Log("[VideoWatchTracker] " + message);
    }
}