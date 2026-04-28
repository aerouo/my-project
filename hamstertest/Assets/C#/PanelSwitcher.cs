using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    public GameObject tutorialPanel;
    public GameObject gamePanel;
    public GameObject winPanel;
    public GameObject losePanel;

    public DropSlot[] allSlots; // 把畫面上的 4 個 Slot 都拖進這個陣列
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

    // 新增：按下「檢查答案」按鈕時呼叫此 Function
    public void CheckAnswers()
    {
        bool isAllCorrect = true;

        foreach (DropSlot slot in allSlots)
        {
            // 檢查是否為空，或者名字是否對不起來
            // 這裡假設你的物件名稱就是 correctID (例如物件叫 "for")
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
    { // 關閉教學畫面，開啟遊戲畫面
        tutorialPanel.SetActive(false);
        gamePanel.SetActive(true);

        // 為了保險，順便確保勝負畫面是關閉的
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }
    public void ShowWin() { gamePanel.SetActive(false); winPanel.SetActive(true); }
    public void ShowLose() { gamePanel.SetActive(false); losePanel.SetActive(true); }

    public void BackToGame()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        gamePanel.SetActive(true);

        // 重置 Slot 的紀錄
        foreach (DropSlot slot in allSlots)
        {
            slot.ClearSlot();
        }

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
}