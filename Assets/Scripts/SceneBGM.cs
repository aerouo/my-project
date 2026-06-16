using UnityEngine;

public class SceneBGM : MonoBehaviour
{
    [Header("這個 Scene 要播放的背景音樂")]
    public AudioClip bgmClip;

    [Header("沒有音樂時停止 BGM")]
    public bool stopBGM = false;

    [Header("音量")]
    [Range(0f, 1f)]
    public float volume = 0.4f;

    void Start()
    {
        if (BGMManager.Instance == null)
            return;

        if (stopBGM)
        {
            BGMManager.Instance.StopBGM();
            return;
        }

        BGMManager.Instance.SetVolume(volume);
        BGMManager.Instance.PlayBGM(bgmClip);
    }
}