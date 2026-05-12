using UnityEngine;
using TMPro; // 記得加這個才能控制文字

public class MapManager : MonoBehaviour
{
    [Header("UI 面板連結")]
    public GameObject levelInfoPanel; // 把你的 LevelInfoPanel 拖進來
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;

    // 點擊地圖上的關卡按鈕呼叫這個
    public void OnClickLevel(int levelNum)
    {
        levelInfoPanel.SetActive(true); // 顯示面板

        
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

    }
    public void OnClickStart()
    {
        SceneManager.LoadScene("Level" + currentSelectedLevel);

    }
    public void OnClickReturn()
    {
        // 回大廳
        UnityEngine.SceneManagement.SceneManager.LoadScene("home");
    }
    public void OnClickback()
    {
        //取消進入關卡
        levelInfoPanel.SetActive(false);
    }
    
}