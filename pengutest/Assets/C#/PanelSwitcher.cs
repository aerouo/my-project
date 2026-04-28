using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    // 在 Inspector 面板中把你的兩個 Panel 拖進來
    public GameObject tutorialPanel;
    public GameObject gamePanel;

    // 當按下 StartButton 時要執行的 Function
    public void StartGame()
    {
        // 檢查面板是否存在，避免報錯
        if (tutorialPanel != null && gamePanel != null)
        {
            tutorialPanel.SetActive(false); // 隱藏教學面板
            gamePanel.SetActive(true);      // 顯示遊戲面板
        }
    }
}