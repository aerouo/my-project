
using UnityEngine;
using UnityEngine.SceneManagement;



public class SceneController : MonoBehaviour

{

    [Header("主要面板控制")]

    public GameObject tutorialPanel; // 教學畫面

    public GameObject gamePanel;     // 第一關作答畫面

    public GameObject gamePanel2;    // 第二關作答畫面



    // --- 教學頁面跳轉到作答頁面 ---

    public void GoToGame()

    {

        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        if (gamePanel != null) gamePanel.SetActive(true);

    }

    // --- 顯示指定的答案/解釋面板 ---

    // 在 Unity Inspector 的按鈕事件中，把對應的 anserA, anserB 等拖入參數框

    public void ShowAnswer(GameObject targetAnswerPanel)

    {

        // 1. 先把大的答案容器 (anserPanal) 找出來並開啟

        // 我們假設這個容器就是 targetAnswerPanel 的父物件

        if (targetAnswerPanel.transform.parent != null)
        {
            targetAnswerPanel.transform.parent.gameObject.SetActive(true);
        }

        // 2. 隱藏作答畫面

        if (gamePanel != null) gamePanel.SetActive(false);

        // 3. 顯示點選的那個答案 (例如 anserA)

        targetAnswerPanel.SetActive(true);

    }

    // --- 從答案畫面返回第一關作答頁面 ---

    // 在 BACK 按鈕的事件中，把目前的答案面板拖入參數框

    public void BackToGame(GameObject currentAnswerPanel)

    {

        // 1. 隱藏目前的答案

        currentAnswerPanel.SetActive(false);

        // 2. 同時也把大的容器關掉，確保下次切換乾淨

        if (currentAnswerPanel.transform.parent != null)

        {

            currentAnswerPanel.transform.parent.gameObject.SetActive(false);

        }

        // 3. 重新顯示作答畫面

        if (gamePanel != null) gamePanel.SetActive(true);

    }

    // --- 【新功能】按下 NEXT 跳轉到第二關 ---

    public void GoToNextLevel(GameObject currentAnswerPanel)

    {

        // 1. 關閉目前的答案小面板 (例如 anserC)

        currentAnswerPanel.SetActive(false);

        // 2. 關閉大的答案容器 (anserPanal)

        if (currentAnswerPanel.transform.parent != null)

        {
            currentAnswerPanel.transform.parent.gameObject.SetActive(false);
        }


        // 3. 確保第一關的題目畫面也是關閉的

        if (gamePanel != null) gamePanel.SetActive(false);


        // 4. 開啟第二關題目

        if (gamePanel2 != null) gamePanel2.SetActive(true);

    }


    // --- 顯示第二關的答案面板 ---

    public void ShowAnswerLevel2(GameObject targetAnswerPanel)

    {
        // 1. 先把大的答案容器 (anserPanal) 找出來並開啟

        // 我們假設這個容器就是 targetAnswerPanel 的父物件

        if (targetAnswerPanel.transform.parent != null)
        {
            targetAnswerPanel.transform.parent.gameObject.SetActive(true);
        }

        // 2. 隱藏作答畫面

        if (gamePanel2 != null) gamePanel2.SetActive(false);

        // 3. 顯示點選的那個答案 (例如 anserA)

        targetAnswerPanel.SetActive(true);

    }

    // --- 專門給第二關答案頁面用的返回功能 ---
    public void BackToGame2(GameObject currentAnswerPanel)
{
    // 1. 隱藏目前顯示的小面板 (例如 anserC)
    currentAnswerPanel.SetActive(false);

    // 2. 關鍵：同時隱藏它的父物件 (就是 anserPanal 2)
    // 這樣下次點別的答案時，就不會看到這次殘留的畫面
    if (currentAnswerPanel.transform.parent != null)
    {
        currentAnswerPanel.transform.parent.gameObject.SetActive(false);
    }

    // 3. 回到題目畫面
    if (gamePanel2 != null) gamePanel2.SetActive(true);
}

}