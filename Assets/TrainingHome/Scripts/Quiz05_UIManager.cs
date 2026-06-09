using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Quiz05_UIManager : MonoBehaviour
{
    public GameObject startScreen;
    public GameObject gamePanel;

    public Quiz05_SlotHandler slotSwitch, slotCase, slotDefault;
    public TMP_Dropdown animalDropdown;

    public GameObject winPanda, loseHamster, losePengu, loseGeneral;

    public void StartGame()
    {
        startScreen.SetActive(false);
        gamePanel.SetActive(true);
    }

    public void CheckResult()
    {
        bool isPlacementCorrect =
            slotSwitch.currentSticker?.GetComponent<Quiz05_DragHandler>().buttonType == "switch" &&
            slotCase.currentSticker?.GetComponent<Quiz05_DragHandler>().buttonType == "case" &&
            slotDefault.currentSticker?.GetComponent<Quiz05_DragHandler>().buttonType == "default";

        gamePanel.SetActive(false);

        if (!isPlacementCorrect)
        {
            loseGeneral.SetActive(true);
            return;
        }

        int choice = animalDropdown.value;

        if (choice == 0)
            loseHamster.SetActive(true);
        else if (choice == 1)
            winPanda.SetActive(true);
        else if (choice == 2)
            losePengu.SetActive(true);
    }

    public void BackToLessonHome()
    {
        SaveQuizAndReturn();
    }

    public void RestartGame()
    {
        winPanda.SetActive(false);
        loseHamster.SetActive(false);
        losePengu.SetActive(false);
        loseGeneral.SetActive(false);

        gamePanel.SetActive(true);

        ResetSlot(slotSwitch);
        ResetSlot(slotCase);
        ResetSlot(slotDefault);
    }

    private void ResetSlot(Quiz05_SlotHandler slot)
    {
        if (slot.currentSticker != null)
        {
            Destroy(slot.currentSticker);
            slot.currentSticker = null;
        }
    }

    public void ResetEntireGame()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    private void SaveQuizAndReturn()
    {
        Debug.Log("開始儲存 basic_05");

        PlayerPrefs.SetString("OpenPanelAfterLoad", "LessonHome");
        PlayerPrefs.SetString("ReturnLessonName", "switch case");

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveQuizDone("basic_05", () =>
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