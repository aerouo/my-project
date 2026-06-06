using UnityEngine;
using UnityEngine.EventSystems;

public class CardSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("音效")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("音量")]
    [Range(0f, 1f)]
    public float volume = 1f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlaySound(hoverSound);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlaySound(clickSound);
    }

    void PlaySound(AudioClip clip)
    {
        Debug.Log("GameObject Active = " + gameObject.activeInHierarchy);
        Debug.Log("AudioSource Enabled = " + audioSource.enabled);

        if (clip == null || audioSource == null)
            return;

        audioSource.PlayOneShot(clip, volume);
    }

}