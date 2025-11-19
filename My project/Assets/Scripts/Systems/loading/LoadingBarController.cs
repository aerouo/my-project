using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingController : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        float progress = 0;

        while (progress < 1f)
        {
            progress += Time.deltaTime * 0.3f; // 越大越快
            slider.value = progress;
            yield return null;
        }

        // loading 完成後切換場景
        SceneManager.LoadScene("下一個場景名稱");
    }
}
