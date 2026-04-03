using TMPro;
using UnityEngine;

public class UI_PageController : MonoBehaviour
{
    [Header("Pages")]
    public GameObject Panel_Lobby;
    public GameObject Panel_TrainingHome;
    public GameObject Panel_LearningList;
    public GameObject Panel_AdvancedList;
    public GameObject Panel_LessonHome;

    [Header("Lesson UI")]
    public TMP_Text Txt_LessonTitle;
    public TMP_Text Txt_VideoTitle;
    public TMP_Text Txt_QuizTitle;

    [Header("Popups")]
    public GameObject Popup_Video;
    public GameObject Popup_Quiz;

    void HideAllPages()
    {
        Panel_Lobby.SetActive(false);
        Panel_TrainingHome.SetActive(false);
        Panel_LearningList.SetActive(false);
        Panel_AdvancedList.SetActive(false);
        Panel_LessonHome.SetActive(false);
    }

    void HideAllPopups()
    {
        if (Popup_Video != null)
            Popup_Video.SetActive(false);

        if (Popup_Quiz != null)
            Popup_Quiz.SetActive(false);
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
        switch (lessonName)
        {
            case "printf":
                Txt_LessonTitle.text = "printf";
                Txt_VideoTitle.text = "printf 教學影片";
                Txt_QuizTitle.text = "printf 基礎測驗";
                break;

            case "for":
                Txt_LessonTitle.text = "for";
                Txt_VideoTitle.text = "for 教學影片";
                Txt_QuizTitle.text = "for 基礎測驗";
                break;

            case "if else":
                Txt_LessonTitle.text = "if else";
                Txt_VideoTitle.text = "if else 教學影片";
                Txt_QuizTitle.text = "if else 基礎測驗";
                break;

            default:
                Txt_LessonTitle.text = lessonName;
                Txt_VideoTitle.text = "教學影片";
                Txt_QuizTitle.text = "基礎測驗";
                break;
        }

        OpenLessonHome();
    }

    public void OpenVideoPopup()
    {
        if (Popup_Video != null)
            Popup_Video.SetActive(true);
    }

    public void CloseVideoPopup()
    {
        if (Popup_Video != null)
            Popup_Video.SetActive(false);
    }

    public void OpenQuizPopup()
    {
        if (Popup_Quiz != null)
            Popup_Quiz.SetActive(true);
    }

    public void CloseQuizPopup()
    {
        if (Popup_Quiz != null)
            Popup_Quiz.SetActive(false);
    }

    void Start()
    {
        OpenLobby();
        HideAllPopups();
    }

    void Update()
    {
    }
}