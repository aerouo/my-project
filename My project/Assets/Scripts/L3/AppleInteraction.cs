using UnityEngine;

public class AppleInteraction : MonoBehaviour
{
    [Header("Good 相關")]
    public GameObject goodGroup;         // 拉入 good 物件
    public GameObject goodTxt;           // 拉入 good txt
    public GameObject goodGroupPanel;    // 拉入 If else group-good

    [Header("Bad 相關")]
    public GameObject badGroup;          // 拉入 bad 物件
    public GameObject badTxt;            // 拉入 bad txt
    public GameObject badGroupPanel;     // 拉入 If else group-bad

    private GameObject currentPanel;

    // game-if else 顯示時呼叫，初始化狀態
    public void Init()
    {
        SetGoodBadVisible(true);
        if (goodGroupPanel != null) goodGroupPanel.SetActive(false);
        if (badGroupPanel != null) badGroupPanel.SetActive(false);
    }

    public void OnClickGood()
    {
        currentPanel = goodGroupPanel;
        if (badGroup != null) badGroup.SetActive(false);
        if (badTxt != null) badTxt.SetActive(false);
        if (goodGroupPanel != null) goodGroupPanel.SetActive(true);
    }

    public void OnClickBad()
    {
        currentPanel = badGroupPanel;
        if (goodGroup != null) goodGroup.SetActive(false);
        if (goodTxt != null) goodTxt.SetActive(false);
        if (badGroupPanel != null) badGroupPanel.SetActive(true);
    }

    public void OnClickReturn()
    {
        if (currentPanel != null) currentPanel.SetActive(false);
        currentPanel = null;
        SetGoodBadVisible(true);
    }

    // 答對後顯示另一邊（由 IfelseGameManager 呼叫）
    public void ShowOtherSide(bool isGood)
    {
        Debug.Log($"ShowOtherSide called, isGood={isGood}, badGroup={badGroup?.name}, goodGroup={goodGroup?.name}");
        if (isGood)
        {
            if (badGroup != null) badGroup.SetActive(true);
            if (badTxt != null) badTxt.SetActive(true);
        }
        else
        {
            if (goodGroup != null) goodGroup.SetActive(true);
            if (goodTxt != null) goodTxt.SetActive(true);
        }
    }


    void SetGoodBadVisible(bool visible)
    {
        if (goodGroup != null) goodGroup.SetActive(visible);
        if (goodTxt != null) goodTxt.SetActive(visible);
        if (badGroup != null) badGroup.SetActive(visible);
        if (badTxt != null) badTxt.SetActive(visible);
    }
}