using UnityEngine;
using UnityEngine.UI;

public class PersonalRoomPopupController : MonoBehaviour
{
    [Header("Root Panel")]
    public GameObject personalRoomRoot; // Panel_Personalroom

    [Header("Panels")]
    public GameObject panelAchievement;
    public GameObject panelCloset;
    public GameObject panelLearning;

    [Header("Button Images")]
    public Image btnAchievement;
    public Image btnCloset;
    public Image btnLearning;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.35f, 0.8f, 1f, 1f);

    void Start()
    {
        // 一開始先關閉個人小屋 Panel
        if (personalRoomRoot != null)
            personalRoomRoot.SetActive(false);
    }

    // ===== 開啟個人小屋 =====
    public void OpenRoom()
    {
        if (personalRoomRoot != null)
            personalRoomRoot.SetActive(true);

        // 預設開啟成就頁
        ShowAchievement();
    }

    // ===== 關閉個人小屋 =====
    public void CloseRoom()
    {
        if (personalRoomRoot != null)
            personalRoomRoot.SetActive(false);
    }

    // ===== 分頁切換 =====

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

    public void ShowLearning()
    {
        OpenOnly(panelLearning);
        SelectButton(btnLearning);
    }

    // ===== 只開啟指定 Panel =====

    private void OpenOnly(GameObject target)
    {
        if (panelAchievement != null)
            panelAchievement.SetActive(false);

        if (panelCloset != null)
            panelCloset.SetActive(false);

        if (panelLearning != null)
            panelLearning.SetActive(false);

        if (target != null)
            target.SetActive(true);
    }

    // ===== 按鈕選中效果 =====

    private void SelectButton(Image targetButton)
    {
        if (btnAchievement != null)
            btnAchievement.color = normalColor;

        if (btnCloset != null)
            btnCloset.color = normalColor;

        if (btnLearning != null)
            btnLearning.color = normalColor;

        if (targetButton != null)
            targetButton.color = selectedColor;
    }
}