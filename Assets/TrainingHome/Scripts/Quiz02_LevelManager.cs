using UnityEngine;
using UnityEngine.SceneManagement;

public class Quiz02_LevelManager : MonoBehaviour
{
    public int totalQuestions = 2; // 設定總共有兩題
    private int correctCount = 0;

    [Header("介面切換")]
    public GameObject gamePanel;   // 原本答題的介面
    public GameObject winPanel;    // 你畫的電腦頁面視窗
    public Animator penguAnimator; // 企鵝的 Animator

    public void AddScore()
    {
        correctCount++;

        if (correctCount >= totalQuestions)
        {
            Invoke("ShowWinUI", 0.5f); // 延遲半秒顯示，感官比較順
        }
    }

    public void BackToLessonHome()
    {
        PlayerPrefs.SetString("OpenPanelAfterLoad", "LessonHome");
        PlayerPrefs.SetString("ReturnLessonName", "print");
        SceneManager.LoadScene("SampleScene");
    }

    void ShowWinUI()
    {
        // 1. 讓原本的答題介面隱形
        if (gamePanel != null) gamePanel.SetActive(false);

        // 2. 顯示你畫的電腦頁面視窗
        if (winPanel != null) winPanel.SetActive(true);

        // 3. 讓企鵝動起來
        if (penguAnimator != null)
        {
            penguAnimator.SetTrigger("Win");
        }
    }
}