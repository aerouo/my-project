using UnityEngine;
using UnityEngine.SceneManagement;

public class AdvancedLevelBack : MonoBehaviour
{
    [Header("返回頁面")]
    public string returnPanelName;

    public void BackToTraining()
    {
        PlayerPrefs.SetString("ReturnPanel", returnPanelName);
        SceneManager.LoadScene("SampleScene");
    }
}