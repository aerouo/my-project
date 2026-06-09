using UnityEngine;
using UnityEngine.UI;

public class LearningCategoryController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject Panel_Basic;
    public GameObject Panel_Advanced;
    public GameObject Panel_Challenge;

    [Header("Button Images")]
    public Image basicButtonImage;
    public Image advancedButtonImage;
    public Image challengeButtonImage;

    [Header("Selected Sprites")]
    public Sprite basicSelectedSprite;       // 學習button
    public Sprite advancedSelectedSprite;    // 進階button
    public Sprite challengeSelectedSprite;   // 闖關button

    [Header("Unselected Sprites")]
    public Sprite basicUnselectedSprite;     // 學習未點開
    public Sprite advancedUnselectedSprite;  // 進階未點開
    public Sprite challengeUnselectedSprite; // 闖關未點開

    void Start()
    {
        ShowBasic();
    }

    void HideAllPanels()
    {
        if (Panel_Basic != null) Panel_Basic.SetActive(false);
        if (Panel_Advanced != null) Panel_Advanced.SetActive(false);
        if (Panel_Challenge != null) Panel_Challenge.SetActive(false);
    }

    void UpdateButtonSprites(string selected)
    {
        if (basicButtonImage != null)
            basicButtonImage.sprite = selected == "Basic" ? basicSelectedSprite : basicUnselectedSprite;

        if (advancedButtonImage != null)
            advancedButtonImage.sprite = selected == "Advanced" ? advancedSelectedSprite : advancedUnselectedSprite;

        if (challengeButtonImage != null)
            challengeButtonImage.sprite = selected == "Challenge" ? challengeSelectedSprite : challengeUnselectedSprite;
    }

    public void ShowBasic()
    {
        HideAllPanels();
        if (Panel_Basic != null) Panel_Basic.SetActive(true);
        UpdateButtonSprites("Basic");
    }

    public void ShowAdvanced()
    {
        HideAllPanels();
        if (Panel_Advanced != null) Panel_Advanced.SetActive(true);
        UpdateButtonSprites("Advanced");
    }

    public void ShowChallenge()
    {
        HideAllPanels();
        if (Panel_Challenge != null) Panel_Challenge.SetActive(true);
        UpdateButtonSprites("Challenge");
    }
}