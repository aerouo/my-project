using UnityEngine;
using UnityEngine.UI;

public class PersonalRoomPopupController : MonoBehaviour
{
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
        ShowAchievement(); // 預設頁面
    }

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