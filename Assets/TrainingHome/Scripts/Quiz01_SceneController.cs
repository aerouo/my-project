using UnityEngine;
using UnityEngine.SceneManagement;

public class Quiz01_SceneController : MonoBehaviour
{
    [Header("主要面板控制")]
    public GameObject tutorialPanel;
    public GameObject gamePanel;     // Panel_Game_Q1
    public GameObject gamePanel2;    // Panel_Game_Q2

    [Header("第一關選項")]
    public GameObject optionA;
    public GameObject optionB;
    public GameObject optionC;
    public GameObject optionD;

    [Header("第二關選項")]
    public GameObject option2A;
    public GameObject option2B;
    public GameObject option2C;
    public GameObject option2D;

    // =========================
    // 完成測驗返回
    // =========================
    public void BackToLessonHome()
    {
        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveQuizDone("basic_01", () =>
            {
                FirestoreManager.Instance.CheckBasicAchievement(1, completed =>
                {
                    PlayerPrefs.SetString("OpenPanelAfterLoad", "LessonHome");
                    PlayerPrefs.SetString("ReturnLessonName", "基礎架構");
                    SceneManager.LoadScene("SampleScene");
                });
            });
        }
        else
        {
            PlayerPrefs.SetString("OpenPanelAfterLoad", "LessonHome");
            PlayerPrefs.SetString("ReturnLessonName", "基礎架構");

            SceneManager.LoadScene("SampleScene");
        }
    }

    // =========================
    // 教學頁面跳到第一關
    // =========================
    public void GoToGame()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        if (gamePanel != null) gamePanel.SetActive(true);
        if (gamePanel2 != null) gamePanel2.SetActive(false);

        ShowLevel1Options(true);
        ShowLevel2Options(true);
    }

    // =========================
    // 第一關顯示答案
    // =========================
    public void ShowAnswer(GameObject targetAnswerPanel)
    {
        ShowAnswerPanel(targetAnswerPanel);
        ShowLevel1Options(false);
    }

    // =========================
    // 第一關返回題目
    // =========================
    public void BackToGame(GameObject currentAnswerPanel)
    {
        CloseAnswerPanel(currentAnswerPanel);
        ShowLevel1Options(true);
    }

    // =========================
    // 第一關 NEXT 到第二關
    // =========================
    public void GoToNextLevel(GameObject currentAnswerPanel)
    {
        CloseAnswerPanel(currentAnswerPanel);

        ShowLevel1Options(true);
        ShowLevel2Options(true);

        if (gamePanel != null) gamePanel.SetActive(false);
        if (gamePanel2 != null) gamePanel2.SetActive(true);
    }

    // =========================
    // 第二關顯示答案
    // =========================
    public void ShowAnswerLevel2(GameObject targetAnswerPanel)
    {
        ShowAnswerPanel(targetAnswerPanel);
        ShowLevel2Options(false);
    }

    // =========================
    // 第二關返回題目
    // =========================
    public void BackToGame2(GameObject currentAnswerPanel)
    {
        CloseAnswerPanel(currentAnswerPanel);
        ShowLevel2Options(true);
    }

    // =========================
    // 第一關選項開關
    // =========================
    private void ShowLevel1Options(bool show)
    {
        if (optionA != null) optionA.SetActive(show);
        if (optionB != null) optionB.SetActive(show);
        if (optionC != null) optionC.SetActive(show);
        if (optionD != null) optionD.SetActive(show);
    }

    // =========================
    // 第二關選項開關
    // =========================
    private void ShowLevel2Options(bool show)
    {
        if (option2A != null) option2A.SetActive(show);
        if (option2B != null) option2B.SetActive(show);
        if (option2C != null) option2C.SetActive(show);
        if (option2D != null) option2D.SetActive(show);
    }

    // =========================
    // 顯示答案面板
    // =========================
    private void ShowAnswerPanel(GameObject targetAnswerPanel)
    {
        if (targetAnswerPanel == null) return;

        Transform answerContainer = targetAnswerPanel.transform.parent;

        if (answerContainer != null)
        {
            answerContainer.gameObject.SetActive(true);

            foreach (Transform child in answerContainer)
            {
                child.gameObject.SetActive(false);
            }
        }

        targetAnswerPanel.SetActive(true);
    }

    // =========================
    // 關閉答案面板
    // =========================
    private void CloseAnswerPanel(GameObject currentAnswerPanel)
    {
        if (currentAnswerPanel == null) return;

        currentAnswerPanel.SetActive(false);

        if (currentAnswerPanel.transform.parent != null)
        {
            currentAnswerPanel.transform.parent.gameObject.SetActive(false);
        }
    }
}