using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PageController : MonoBehaviour
{
    [Header("Pages")]
    public GameObject Panel_Lobby;
    public GameObject Panel_TrainingHome;
    public GameObject Panel_LearningList;
    public GameObject Panel_AdvancedList;
    public GameObject Panel_LessonHome;
    public GameObject Panel_Quiz;

    [Header("Lesson UI")]
    public TMP_Text Txt_LessonTitle;
    public TMP_Text Txt_VideoTitle;
    public TMP_Text Txt_QuizTitle;

    [Header("Lesson Title Icon")]
    public Image Img_LessonTitle;
    public Sprite Icon_Basic;
    public Sprite Icon_Printf;
    public Sprite Icon_For;
    public Sprite Icon_IfElse;
    public Sprite Icon_SwitchCase;

    [Header("Quiz UI")]
    public TMP_Text Txt_QuizPageTitle;

    [Header("Popups")]
    public GameObject Popup_Video;

    private string currentLessonName = "";

    void HideAllPages()
    {
        Panel_Lobby.SetActive(false);
        Panel_TrainingHome.SetActive(false);
        Panel_LearningList.SetActive(false);
        Panel_AdvancedList.SetActive(false);
        Panel_LessonHome.SetActive(false);

        if (Panel_Quiz != null)
            Panel_Quiz.SetActive(false);
    }

    void HideAllPopups()
    {
        if (Popup_Video != null)
            Popup_Video.SetActive(false);
    }

    void SetLessonTitleIcon(Sprite icon)
    {
        if (Img_LessonTitle == null)
            return;

        if (icon != null)
        {
            Img_LessonTitle.sprite = icon;
            Img_LessonTitle.gameObject.SetActive(true);
            Img_LessonTitle.SetNativeSize();

            if (Txt_LessonTitle != null)
                Txt_LessonTitle.gameObject.SetActive(false);
        }
        else
        {
            Img_LessonTitle.gameObject.SetActive(false);

            if (Txt_LessonTitle != null)
                Txt_LessonTitle.gameObject.SetActive(true);
        }
    }

    public void OpenLobby()
    {
        HideAllPages();
        HideAllPopups();
        Panel_Lobby.SetActive(true);
    }

    public void OpenTrainingHome()
    {
        HideAllPages();
        HideAllPopups();
        Panel_TrainingHome.SetActive(true);
    }

    public void OpenLearningList()
    {
        HideAllPages();
        HideAllPopups();
        Panel_LearningList.SetActive(true);
    }

    public void OpenAdvancedList()
    {
        HideAllPages();
        HideAllPopups();
        Panel_AdvancedList.SetActive(true);
    }

    public void OpenLessonHome()
    {
        HideAllPages();
        HideAllPopups();
        Panel_LessonHome.SetActive(true);
    }

    public void OpenLessonByName(string lessonName)
    {
        currentLessonName = lessonName;

        Sprite lessonIcon = null;

        switch (lessonName)
        {
            case "基礎架構":
                Txt_LessonTitle.text = "基礎架構";
                Txt_VideoTitle.text = "基礎架構 教學影片";
                Txt_QuizTitle.text = "基礎架構 基礎測驗";
                lessonIcon = Icon_Basic;
                break;

            case "print":
                Txt_LessonTitle.text = "print";
                Txt_VideoTitle.text = "print 教學影片";
                Txt_QuizTitle.text = "print 基礎測驗";
                lessonIcon = Icon_Printf;
                break;

            case "for":
                Txt_LessonTitle.text = "for";
                Txt_VideoTitle.text = "for 教學影片";
                Txt_QuizTitle.text = "for 基礎測驗";
                lessonIcon = Icon_For;
                break;

            case "if else":
                Txt_LessonTitle.text = "if else";
                Txt_VideoTitle.text = "if else 教學影片";
                Txt_QuizTitle.text = "if else 基礎測驗";
                lessonIcon = Icon_IfElse;
                break;

            case "switch case":
                Txt_LessonTitle.text = "switch case";
                Txt_VideoTitle.text = "switch case 教學影片";
                Txt_QuizTitle.text = "switch case 基礎測驗";
                lessonIcon = Icon_SwitchCase;
                break;

            default:
                Txt_LessonTitle.text = lessonName;
                Txt_VideoTitle.text = lessonName + " 教學影片";
                Txt_QuizTitle.text = lessonName + " 基礎測驗";
                lessonIcon = null;
                break;
        }

        SetLessonTitleIcon(lessonIcon);
        OpenLessonHome();
    }

    public void OpenVideoPopup()
    {
        HideAllPopups();

        if (Popup_Video != null)
            Popup_Video.SetActive(true);
    }

    public void CloseVideoPopup()
    {
        if (Popup_Video != null)
            Popup_Video.SetActive(false);
    }

    public void OpenQuizPanel()
    {
        HideAllPages();
        HideAllPopups();

        if (Txt_QuizPageTitle != null)
            Txt_QuizPageTitle.text = currentLessonName + " 基礎測驗";

        if (Panel_Quiz != null)
            Panel_Quiz.SetActive(true);
    }

    public void BackToLessonHome()
    {
        OpenLessonHome();
    }

    void Start()
    {
        OpenLobby();
    }
}