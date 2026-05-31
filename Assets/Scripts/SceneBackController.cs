using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBackController : MonoBehaviour
{
    [Header("返回 SampleScene 的指定 Panel")]
    public string returnPanelName = "Panel_Clue";


    [Header("返回關卡地圖")]
    public string questMapSceneName = "quest-map";

    public void BackToTrainingPanel()
    {
        PlayerPrefs.SetString("ReturnPanel", returnPanelName);
        SceneManager.LoadScene("SampleScene");
    }

    public void BackToQuestMap()
    {
        PlayerPrefs.SetString("ReturnPanel", returnPanelName);
        SceneManager.LoadScene(questMapSceneName);
    }
}