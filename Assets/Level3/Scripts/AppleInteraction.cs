using UnityEngine;

public class AppleInteraction : MonoBehaviour
{
    [Header("Good 相關")]
    public GameObject goodGroup;
    public GameObject goodTxt;
    public GameObject goodGroupPanel;

    [Header("Bad 相關")]
    public GameObject badGroup;
    public GameObject badTxt;
    public GameObject badGroupPanel;

    [Header("Level3 計時")]
    public Level3Manager level3Manager;

    private GameObject currentPanel;

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

        if (goodGroupPanel != null)
            goodGroupPanel.SetActive(true);

        if (level3Manager != null)
            level3Manager.StartQuestTimer();

        Debug.Log("【Level3】進入 Good 拖曳，開始計時");
    }

    public void OnClickBad()
    {
        currentPanel = badGroupPanel;

        if (goodGroup != null) goodGroup.SetActive(false);
        if (goodTxt != null) goodTxt.SetActive(false);

        if (badGroupPanel != null)
            badGroupPanel.SetActive(true);

        if (level3Manager != null)
            level3Manager.StartQuestTimer();

        Debug.Log("【Level3】進入 Bad 拖曳，開始計時");
    }

    public void OnClickReturn()
    {
        if (level3Manager != null)
            level3Manager.PauseQuestTimer();

        if (currentPanel != null)
            currentPanel.SetActive(false);

        currentPanel = null;
        SetGoodBadVisible(true);

        Debug.Log("【Level3】返回 good / bad 選擇畫面，暫停計時");
    }

    public void ShowOtherSide(bool isGood)
    {
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