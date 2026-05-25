using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    public GameObject tutorialPanel;
    public GameObject gamePanel;
    public GameObject winPanel;
    public GameObject losePanel;

    // 用來重置物件位置的參考（在 Inspector 拖入 Hat, umbrella, Sun）
    public GameObject[] itemsToReset;
    private Vector3[] startPositions;

    void Awake()
    {
        // 紀錄所有物件的初始位置
        startPositions = new Vector3[itemsToReset.Length];
        for (int i = 0; i < itemsToReset.Length; i++)
        {
            startPositions[i] = itemsToReset[i].transform.position;
        }
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

    // Again 按鈕點擊時呼叫
    public void BackToGame()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        gamePanel.SetActive(true);

        // 重置所有物件到原本的位置
        for (int i = 0; i < itemsToReset.Length; i++)
        {
            // 先把父物件設回 gamePanel，這樣 Slot 才會變回空的
            itemsToReset[i].transform.SetParent(gamePanel.transform);
            itemsToReset[i].transform.position = startPositions[i];

            // 記得把之前為了拖拽關掉的 Raycast 打開，不然 Again 之後會不能抓
            if (itemsToReset[i].TryGetComponent<CanvasGroup>(out var cg))
            {
                cg.blocksRaycasts = true;
            }
        }
    }
}