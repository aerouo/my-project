using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelSwitcher : MonoBehaviour
{
    public GameObject tutorialPanel;
    public GameObject gamePanel;
    public GameObject winPanel;
    public GameObject losePanel;

    public GameObject[] itemsToReset;
    private Vector3[] startPositions;

    void Awake()
    {
        startPositions = new Vector3[itemsToReset.Length];

        for (int i = 0; i < itemsToReset.Length; i++)
        {
            startPositions[i] = itemsToReset[i].transform.position;
        }
    }

    public void BackToLessonHome()
    {
        SaveQuizAndReturn();
    }

    public void StartGame()
    {
        tutorialPanel.SetActive(false);
        gamePanel.SetActive(true);
    }

    public void ShowWin()
    {
        gamePanel.SetActive(false);
        winPanel.SetActive(true);
    }

    public void ShowLose()
    {
        gamePanel.SetActive(false);
        losePanel.SetActive(true);
    }

    public void BackToGame()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        gamePanel.SetActive(true);

        for (int i = 0; i < itemsToReset.Length; i++)
        {
            itemsToReset[i].transform.SetParent(gamePanel.transform);
            itemsToReset[i].transform.position = startPositions[i];

            if (itemsToReset[i].TryGetComponent<CanvasGroup>(out var cg))
            {
                cg.blocksRaycasts = true;
            }
        }
    }

    private void SaveQuizAndReturn()
    {
        Debug.Log("開始儲存 basic_03");

        PlayerPrefs.SetString("OpenPanelAfterLoad", "LessonHome");
        PlayerPrefs.SetString("ReturnLessonName", "if else");

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveQuizDone("basic_03", () =>
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