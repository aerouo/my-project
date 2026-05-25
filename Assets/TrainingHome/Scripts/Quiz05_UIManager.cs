using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Quiz05_UIManager : MonoBehaviour
{
    // 在 Inspector 中將你的 TutorialPanel 或 Background 拖進來
    public GameObject startScreen;
    // 在 Inspector 中將你的 gamepanel 拖進來
    public GameObject gamePanel;

    public void StartGame()
    {
        // 隱藏開始畫面（包含 Start 按鈕的那個面板）
        startScreen.SetActive(false);
        // 顯示遊戲畫面
        gamePanel.SetActive(true);
    }

    public Quiz05_SlotHandler slotSwitch, slotCase, slotDefault;
    public TMP_Dropdown animalDropdown;

    // 把你的結果畫面拖進這些欄位
    public GameObject winPanda, loseHamster, losePengu, loseGeneral;
    //public GameObject gamePanel;

    public void CheckResult()
    {
        // 1. 檢查按鈕是否放對地方
        bool isPlacementCorrect =
            slotSwitch.currentSticker?.GetComponent<Quiz05_DragHandler>().buttonType == "switch" &&
            slotCase.currentSticker?.GetComponent<Quiz05_DragHandler>().buttonType == "case" &&
            slotDefault.currentSticker?.GetComponent<Quiz05_DragHandler>().buttonType == "default";

        gamePanel.SetActive(false); // 關閉遊戲介面

        if (!isPlacementCorrect)
        {
            loseGeneral.SetActive(true); // 格子放錯，跳到一般失敗畫面
            return;
        }

        // 2. 如果格子都放對，根據 Dropdown (1, 2, 3) 判斷結局
        // Dropdown 的 Value 從 0 開始算 (0=1, 1=2, 2=3)
        int choice = animalDropdown.value;

        if (choice == 0) // 選了 1 (倉鼠)
            loseHamster.SetActive(true);
        else if (choice == 1) // 選了 2 (熊貓)
            winPanda.SetActive(true);
        else if (choice == 2) // 選了 3 (企鵝)
            losePengu.SetActive(true);
    }
    public void BackToLessonHome()
    {
        PlayerPrefs.SetString("OpenPanelAfterLoad", "LessonHome");
        PlayerPrefs.SetString("ReturnLessonName", "switch case");
        SceneManager.LoadScene("SampleScene");
    }

    public void RestartGame()
    {
        // 1. 關閉所有的結局畫面
        winPanda.SetActive(false);
        loseHamster.SetActive(false);
        losePengu.SetActive(false);
        loseGeneral.SetActive(false);

        // 2. 開啟遊戲主畫面
        gamePanel.SetActive(true);

        // 3. 重置格子狀態 (讓格子忘記原本放了什麼)
        ResetSlot(slotSwitch);
        ResetSlot(slotCase);
        ResetSlot(slotDefault);
    }

    private void ResetSlot(Quiz05_SlotHandler slot)
    {
        if (slot.currentSticker != null)
        {
            // 這裡你可以選擇：
            // A. 直接刪除原本放上去的按鈕 (如果你是用生成的方式)
            // B. 或是讓按鈕回到原本下方的 ButtonGroup (推薦)

            // 假設你下方有一個放按鈕的父物件叫 buttonGroup
            // slot.currentSticker.transform.SetParent(buttonGroup); 

            // 最簡單的做法是直接把那個按鈕移掉或重置位置
            Destroy(slot.currentSticker); // 這會刪除按鈕，建議配合生成系統
            slot.currentSticker = null;
        }
    }

    public void ResetEntireGame()
    {
        // 取得當前場景的名字並重新載入
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}