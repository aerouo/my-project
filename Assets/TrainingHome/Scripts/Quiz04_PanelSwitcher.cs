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
    {   // 在 Awake 中記錄每個可拖動物件的初始位置和父物件
        itemRects = new RectTransform[itemsToReset.Length];
        startAnchoredPositions = new Vector2[itemsToReset.Length];
        startParents = new Transform[itemsToReset.Length];

        for (int i = 0; i < itemsToReset.Length; i++)
        {   // 確保每個物件都有 RectTransform，並記錄其初始狀態
            itemRects[i] = itemsToReset[i].GetComponent<RectTransform>();
            startAnchoredPositions[i] = itemRects[i].anchoredPosition;
            startParents[i] = itemsToReset[i].transform.parent;
        }
    }
    public void BackToLessonHome()
    {
        PlayerPrefs.SetString("OpenPanelAfterLoad", "LessonHome");
        PlayerPrefs.SetString("ReturnLessonName", "for");
        SceneManager.LoadScene("SampleScene");
    }

    public void CheckAnswers()
    {   // 檢查所有 DropSlot 是否都放置了正確的物件
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
    {   //  從 Tutorial 進入遊戲畫面，並重置所有狀態
        tutorialPanel.SetActive(false);
        gamePanel.SetActive(true);

        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    public void ShowWin()
    {   //  顯示勝利畫面，並隱藏遊戲畫面
        gamePanel.SetActive(false);
        winPanel.SetActive(true);
    }

    public void ShowLose()
    {   //  顯示失敗畫面，並隱藏遊戲畫面
        gamePanel.SetActive(false);
        losePanel.SetActive(true);
    }

    public void BackToGame()
    {   // 從 Win 或 Lose 畫面返回遊戲畫面，並重置所有狀態
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        gamePanel.SetActive(true);

        foreach (Quiz04_DropSlot slot in allSlots)
        {   // 清空每個 DropSlot 的狀態
            slot.ClearSlot();
        }

        for (int i = 0; i < itemsToReset.Length; i++)
        {   //  將每個可拖動物件重置到初始位置和父物件，並恢復其狀態
            itemsToReset[i].transform.SetParent(startParents[i], false);

            itemRects[i].anchoredPosition = startAnchoredPositions[i];
            itemRects[i].localScale = Vector3.one;
            itemRects[i].localRotation = Quaternion.identity;

            if (itemsToReset[i].TryGetComponent<CanvasGroup>(out var cg))
            {   //  恢復 CanvasGroup 的狀態，使物件可見且可交互
                cg.blocksRaycasts = true;
                cg.alpha = 1f;
            }
        }
    }
}