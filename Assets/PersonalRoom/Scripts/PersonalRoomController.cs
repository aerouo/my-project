using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PersonalRoomPopupController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelAchievement;
    public GameObject panelCloset;
    public GameObject panelLearning;

    [Header("Closet")]
    public ClosetPreviewManager closetPreviewManager;

    [Header("Button Images")]
    public Image btnAchievement;
    public Image btnCloset;
    public Image btnLearning;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.35f, 0.8f, 1f, 1f);

    [Header("Achievement Scroll")]
    public ScrollRect achievementScrollRect;

    [Header("Center Position")]
    public RectTransform achievementContent;
    public Vector2 centerPosition = Vector2.zero;

    void Start()
    {
        ShowAchievement();
    }

    public void BackToHome()
    {
        CloseClosetPreview();
        SceneManager.LoadScene("home");
    }

    public void ShowAchievement()
    {
        CloseClosetPreview();

        OpenOnly(panelAchievement);
        SelectButton(btnAchievement);
        ResetAchievementView();

        if (AchievementManager.Instance != null)
            AchievementManager.Instance.RefreshAchievements();
    }

    public void ShowCloset()
    {
        OpenOnly(panelCloset);
        SelectButton(btnCloset);
    }

    public void ShowLearning()
    {
        CloseClosetPreview();

        OpenOnly(panelLearning);
        SelectButton(btnLearning);
    }

    private void CloseClosetPreview()
    {
        if (closetPreviewManager != null)
            closetPreviewManager.CloseCloset();
    }

    private void OpenOnly(GameObject target)
    {
        if (panelAchievement != null) panelAchievement.SetActive(false);
        if (panelCloset != null) panelCloset.SetActive(false);
        if (panelLearning != null) panelLearning.SetActive(false);

        if (target != null) target.SetActive(true);
    }

    private void SelectButton(Image targetButton)
    {
        if (btnAchievement != null) btnAchievement.color = normalColor;
        if (btnCloset != null) btnCloset.color = normalColor;
        if (btnLearning != null) btnLearning.color = normalColor;

        if (targetButton != null) targetButton.color = selectedColor;
    }

    private void ResetAchievementView()
    {
        Canvas.ForceUpdateCanvases();

        if (achievementScrollRect != null)
        {
            achievementScrollRect.horizontalNormalizedPosition = 0.5f;
            achievementScrollRect.verticalNormalizedPosition = 0.5f;
        }

        if (achievementContent != null)
            achievementContent.anchoredPosition = centerPosition;

        Canvas.ForceUpdateCanvases();
    }
}