using UnityEngine;
using UnityEngine.SceneManagement;

public class Quiz02_LevelManager : MonoBehaviour
{
    public int totalQuestions = 2;
    private int correctCount = 0;

    [Header("介面切換")]
    public GameObject gamePanel;
    public GameObject winPanel;
    public Animator penguAnimator;

    public void AddScore()
    {
        correctCount++;

        if (correctCount >= totalQuestions)
        {
            Invoke("ShowWinUI", 0.5f);
        }
    }

    public void BackToLessonHome()
    {
        SaveQuizAndReturn();
    }

    void ShowWinUI()
    {
        if (gamePanel != null) gamePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(true);

        if (penguAnimator != null)
        {
            penguAnimator.SetTrigger("Win");
        }
    }

    private void SaveQuizAndReturn()
    {
        PlayerPrefs.SetString("OpenPanelAfterLoad", "LessonHome");
        PlayerPrefs.SetString("ReturnLessonName", "print");

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveQuizDone("basic_02", () =>
            {
                FirestoreManager.Instance.CheckBasicAchievement(1, completed =>
                {
                    SceneManager.LoadScene("SampleScene");
                });
            });
        }
        else
        {
            SceneManager.LoadScene("SampleScene");
        }
    }
}