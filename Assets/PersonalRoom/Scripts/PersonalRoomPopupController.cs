using UnityEngine;
using UnityEngine.UI;

public class PersonalRoomPopupController : MonoBehaviour
{
    [Header("Root Panel")]
    public GameObject personalRoomRoot; // 整個小屋UI（最外層）

    [Header("Panels")]
    public GameObject panelAchievement;
    public GameObject panelCloset;
    public GameObject panelShop;
    public GameObject panelLearning;

    [Header("Button Images")]
    public Image btnAchievement;
    public Image btnShop;
    public Image btnCloset;
    public Image btnLearning;

    [Header("Colors")]
    public Color normalColor = new Color(1f, 1f, 1f, 1f);
    public Color selectedColor = new Color(0.35f, 0.8f, 1f, 1f);

    void Start()
    {
        // 一開始先關掉整個小屋（如果你希望一開始不顯示）
        if (personalRoomRoot != null)
            personalRoomRoot.SetActive(false);
    }

    // ====== 外部控制（你新增的按鈕用這兩個） ======

    public void OpenRoom()
    {
        if (personalRoomRoot != null)
            personalRoomRoot.SetActive(true);

        // 打開時預設顯示成就頁
        ShowAchievement();
    }

    public void CloseRoom()
    {
        if (personalRoomRoot != null)
            personalRoomRoot.SetActive(false);
    }

    // ====== 內部頁面切換（你原本的） ======

    public void ShowAchievement()
    {
        OpenOnly(panelAchievement);
        SelectButton(btnAchievement);
    }

    public void ShowCloset()
    {
        OpenOnly(panelCloset);
        SelectButton(btnCloset);
    }

    public void ShowShop()
    {
        OpenOnly(panelShop);
        SelectButton(btnShop);
    }

    public void ShowLearning()
    {
        OpenOnly(panelLearning);
        SelectButton(btnLearning);
    }

    /// <summary>
    /// 只開啟指定 Panel，其餘全部關閉
    /// </summary>
    private void OpenOnly(GameObject target)
    {
        panelAchievement.SetActive(false);
        panelCloset.SetActive(false);
        panelShop.SetActive(false);
        panelLearning.SetActive(false);

        if (target != null)
            target.SetActive(true);
    }

    /// <summary>
    /// 設定按鈕選中顏色
    /// </summary>
    private void SelectButton(Image targetButton)
    {
        btnAchievement.color = normalColor;
        btnShop.color = normalColor;
        btnCloset.color = normalColor;
        btnLearning.color = normalColor;

        if (targetButton != null)
            targetButton.color = selectedColor;
    }
}