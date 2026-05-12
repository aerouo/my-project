using UnityEngine;

public class LearningCategoryController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject Panel_Basic;
    public GameObject Panel_Advanced;
    public GameObject Panel_Challenge;

    void Start()
    {
        ShowBasic(); // 一開始預設顯示學習
    }

    void HideAllPanels()
    {
        if (Panel_Basic != null) Panel_Basic.SetActive(false);
        if (Panel_Advanced != null) Panel_Advanced.SetActive(false);
        if (Panel_Challenge != null) Panel_Challenge.SetActive(false);
    }

    public void ShowBasic()
    {
        HideAllPanels();
        if (Panel_Basic != null) Panel_Basic.SetActive(true);
    }

    public void ShowAdvanced()
    {
        HideAllPanels();
        if (Panel_Advanced != null) Panel_Advanced.SetActive(true);
    }

    public void ShowChallenge()
    {
        HideAllPanels();
        if (Panel_Challenge != null) Panel_Challenge.SetActive(true);
    }
}