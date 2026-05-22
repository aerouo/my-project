using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    [Header("UI 面板連結")]
    public GameObject levelInfoPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;

    [Header("歷史紀錄顯示")]
    public QuestBestRecordDisplay bestRecordDisplay;

    private int currentSelectedLevel = 1;

    public void OnClickLevel(int levelNum)
    {
        currentSelectedLevel = levelNum;

        if (levelInfoPanel != null)
            levelInfoPanel.SetActive(true);

        if (levelNum == 1)
        {
            titleText.text = "第一關";
            descText.text = "你在一個陌生的環境醒來，求生的本能使你想辦法逃出去，但卻不是很順利...";
        }
        else if (levelNum == 2)
        {
            titleText.text = "第二關";
            descText.text = "第一次的失敗後，你有點挫折，在思緒中就聽到一些聲響，驚恐的你發現...";
        }
        else if (levelNum == 3)
        {
            titleText.text = "第三關";
            descText.text = "你被帶到安全的地方，放鬆警惕的你疲憊不堪，填飽肚子後你又決定繼續尋找出路...";
        }
        else if (levelNum == 4)
        {
            titleText.text = "第四關";
            descText.text = "你大致熟悉了這個地方，但本能驅使你仍想趕快逃離，你盤算著如何離開島嶼...";
        }
        else if (levelNum == 5)
        {
            titleText.text = "第五關";
            descText.text = "天氣晴朗，你的心情和天氣一樣好，未知的旅程混雜著不安，你還是勇敢踏出了這一步...";
        }

        // 讀取該關卡最佳紀錄
        if (bestRecordDisplay != null)
        {
            string levelID = "Level" + levelNum;
            bestRecordDisplay.ShowRecord(levelID);
        }
    }

    public void OnClickStart()
    {
        SceneManager.LoadScene("Level" + currentSelectedLevel);
    }

    public void OnClickReturn()
    {
        SceneManager.LoadScene("home");
    }

    public void OnClickback()
    {
        if (levelInfoPanel != null)
            levelInfoPanel.SetActive(false);
    }
}