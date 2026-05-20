using UnityEngine;
using UnityEngine.Video;

public class VideoWatchTracker : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;

    [Header("Learning ID")]
    public string videoID = "basic_01";

    [Header("Complete Setting")]
    public double completeRate = 0.9;

    private bool saved = false;
    private double maxProgress = 0;

    void Update()
    {
        if (saved) return;
        if (videoPlayer == null) return;
        if (videoPlayer.length <= 0) return;

        double progress = videoPlayer.time / videoPlayer.length;

        if (progress > maxProgress)
            maxProgress = progress;

        if (maxProgress >= completeRate)
        {
            SaveWatched();
        }
    }

    void SaveWatched()
    {
        if (saved) return;

        saved = true;

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveVideoWatched(videoID);
            FirestoreManager.Instance.CheckBasicAchievement(GetLevelNumber(videoID));
        }

        Debug.Log("影片觀看完成：" + videoID);
    }

    int GetLevelNumber(string id)
    {
        string[] parts = id.Split('_');

        if (parts.Length >= 2 && int.TryParse(parts[1], out int number))
            return number;

        return 1;
    }
}