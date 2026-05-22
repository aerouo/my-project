using UnityEngine;
using UnityEngine.SceneManagement;

public class Quiz04_PanelSwitcher : MonoBehaviour
{
    public GameObject tutorialPanel;
    public GameObject gamePanel;
    public GameObject winPanel;
    public GameObject losePanel;

    public Quiz04_DropSlot[] allSlots;
    public GameObject[] itemsToReset;

    private RectTransform[] itemRects;
    private Vector2[] startAnchoredPositions;
    private Transform[] startParents;

    void Awake()
    {
        itemRects = new RectTransform[itemsToReset.Length];
        startAnchoredPositions = new Vector2[itemsToReset.Length];
        startParents = new Transform[itemsToReset.Length];

        for (int i = 0; i < itemsToReset.Length; i++)
        {
            itemRects[i] = itemsToReset[i].GetComponent<RectTransform>();
            startAnchoredPositions[i] = itemRects[i].anchoredPosition;
            startParents[i] = itemsToReset[i].transform.parent;
        }
    }

    public void BackToLessonHome()
    {
        SaveQuizAndReturn();
    }

    public void CheckAnswers()
    {
        bool isAllCorrect = true;

        foreach (Quiz04_DropSlot slot in allSlots)
        {
            if (slot.currentItem == null || slot.currentItem.name != slot.correctID)
            {
                isAllCorrect = false;
                break;
            }
        }

        if (isAllCorrect)
            ShowWin();
        else
            ShowLose();
    }

    public void StartGame()
    {
        tutorialPanel.SetActive(false);
        gamePanel.SetActive(true);

        winPanel.SetActive(false);
        losePanel.SetActive(false);
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

        foreach (Quiz04_DropSlot slot in allSlots)
        {
            slot.ClearSlot();
        }

        for (int i = 0; i < itemsToReset.Length; i++)
        {
            itemsToReset[i].transform.SetParent(startParents[i], false);

            itemRects[i].anchoredPosition = startAnchoredPositions[i];
            itemRects[i].localScale = Vector3.one;
            itemRects[i].localRotation = Quaternion.identity;

            if (itemsToReset[i].TryGetComponent<CanvasGroup>(out var cg))
            {
                cg.blocksRaycasts = true;
                cg.alpha = 1f;
            }
        }
    }

    private void SaveQuizAndReturn()
    {
        Debug.Log("開始儲存 basic_04");

        PlayerPrefs.SetString("OpenPanelAfterLoad", "LessonHome");
        PlayerPrefs.SetString("ReturnLessonName", "for");

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveQuizDone("basic_04", () =>
            {
                SceneManager.LoadScene("SampleScene");
            });
        }
        else
        {
            SceneManager.LoadScene("SampleScene");
        }
    }
}