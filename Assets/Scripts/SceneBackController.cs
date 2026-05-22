using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBackController : MonoBehaviour
{
    [Header("返回 SampleScene 的指定 Panel")]
    public string returnPanelName;

    [Header("返回闖關地圖")]
    public string questMapSceneName = "quest-map";

    public void BackToTrainingPanel()
    {
        PlayerPrefs.SetString("ReturnPanel", returnPanelName);
        SceneManager.LoadScene("SampleScene");
    }

    public void BackToQuestMap()
    {
        SceneManager.LoadScene(questMapSceneName);
    }
}