using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class UI_PageController : MonoBehaviour
{ // 這個腳本負責管理訓練首頁的所有頁面和彈窗，根據不同的課程名稱設定標題和圖示
    [Header("Pages")]
    public GameObject Panel_Lobby;
    public GameObject Panel_TrainingHome;
    public GameObject Panel_LearningList;
    public GameObject Panel_AdvancedList;
    public GameObject Panel_LessonHome;

    public GameObject Panel_Clue;
    public GameObject Panel_Puzzle;
    public GameObject Panel_Forest;
    public GameObject Panel_Makeboat;
    public GameObject Panel_Boating;


    [Header("Lesson UI")]
    public TMP_Text Txt_LessonTitle;
    public TMP_Text Txt_VideoTitle;
    public TMP_Text Txt_QuizTitle;

    [Header("Lesson Title Icon")]
    public Image Img_LessonTitle;
    public Sprite Icon_Basic;
    public Sprite Icon_Printf;
    public Sprite Icon_IfElse;
    public Sprite Icon_For;
    public Sprite Icon_SwitchCase;

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public VideoClip video_Basic;
    public VideoClip video_Print;
    public VideoClip video_IfElse;
    public VideoClip video_For;
    public VideoClip video_SwitchCase;

    [Header("Quiz UI")]
    public TMP_Text Txt_QuizPageTitle;

    [Header("Popups")]
    public GameObject Popup_Video;

    private string currentLessonName = "";

    void HideAllPages()
    { // 關閉所有頁面
        Panel_Lobby.SetActive(false);
        Panel_TrainingHome.SetActive(false);
        Panel_LearningList.SetActive(false);
        Panel_AdvancedList.SetActive(false);
        Panel_LessonHome.SetActive(false);
        Panel_Clue.SetActive(false);
        Panel_Puzzle.SetActive(false);
        Panel_Forest.SetActive(false);
        Panel_Makeboat.SetActive(false);
        Panel_Boating.SetActive(false);
    }

    void HideAllPopups()
    { // 關閉所有彈窗
        if (Popup_Video != null)
            Popup_Video.SetActive(false);
    }

    void SetLessonTitleIcon(Sprite icon)
    { // 根據傳入的圖示設定課程標題的圖示，如果沒有圖示則顯示文字
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
    { // 打開大廳頁面
        HideAllPages();
        HideAllPopups();
        Panel_Lobby.SetActive(true);
    }

    public void OpenTrainingHome()
    { // 打開訓練首頁
        HideAllPages();
        HideAllPopups();
        Panel_TrainingHome.SetActive(true);
    }

    public void OpenLearningList()
    { // 打開學習清單頁面
        HideAllPages();
        HideAllPopups();
        Panel_LearningList.SetActive(true);
    }

    public void OpenAdvancedList()
    { // 打開進階清單頁面
        HideAllPages();
        HideAllPopups();
        Panel_AdvancedList.SetActive(true);
    }

    public void OpenLessonHome()
    { // 打開課程首頁
        HideAllPages();
        HideAllPopups();
        Panel_LessonHome.SetActive(true);
    }

    public void OpenClue()
    { // 打開線索頁面
        HideAllPages();
        HideAllPopups();
        Panel_Clue.SetActive(true);
    }

    public void OpenPuzzle()
    { // 打開謎題頁面
        HideAllPages();
        HideAllPopups();
        Panel_Puzzle.SetActive(true);
    }

    public void OpenForest()
    { // 打開森林頁面
        HideAllPages();
        HideAllPopups();
        Panel_Forest.SetActive(true);
    }

    public void OpenMakeboat()
    { // 打開造船頁面
        HideAllPages();
        HideAllPopups();
        Panel_Makeboat.SetActive(true);
    }

    public void OpenBoating()
    { // 打開划船頁面
        HideAllPages();
        HideAllPopups();
        Panel_Boating.SetActive(true);
    }

    public void GoToRowingLevel1()
    {
        SceneManager.LoadScene("Rowing_Lvl1");
    }

    public void OpenLessonByName(string lessonName)
    {
        currentLessonName = lessonName;

        Sprite lessonIcon = null;

        switch (lessonName)
        {   // 這裡可以根據不同的課程名稱設定不同的標題和圖示
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

            case "if else":
                Txt_LessonTitle.text = "if else";
                Txt_VideoTitle.text = "if else 教學影片";
                Txt_QuizTitle.text = "if else 基礎測驗";
                lessonIcon = Icon_IfElse;
                break;

            case "for":
                Txt_LessonTitle.text = "for";
                Txt_VideoTitle.text = "for 教學影片";
                Txt_QuizTitle.text = "for 基礎測驗";
                lessonIcon = Icon_For;
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

        if (videoPlayer == null) return;

        switch (currentLessonName)
        {
            case "基礎架構":
                videoPlayer.clip = video_Basic;
                break;

            case "print":
                videoPlayer.clip = video_Print;
                break;

            case "if else":
                videoPlayer.clip = video_IfElse;
                break;

            case "for":
                videoPlayer.clip = video_For;
                break;

            case "switch case":
                videoPlayer.clip = video_SwitchCase;
                break;
        }

        videoPlayer.Stop();
        videoPlayer.Play();
    }

    public void CloseVideoPopup()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();

        if (Popup_Video != null)
            Popup_Video.SetActive(false);
    }

    public TMP_Text pauseText;

    public void ToggleVideoPause()
    {
        if (videoPlayer == null) return;

        if (videoPlayer.isPlaying)
            videoPlayer.Pause();
        else
            videoPlayer.Play();
    }

    public void OpenQuizPanel()
    {
        // 根據目前課程名稱切換到對應測驗場景
        switch (currentLessonName)
        {
            case "基礎架構":
                SceneManager.LoadScene("QuizScene_01");
                break;

            case "print":
                SceneManager.LoadScene("QuizScene_02");
                break;

            case "if else":
                SceneManager.LoadScene("QuizScene_03");
                break;

            case "for":
                SceneManager.LoadScene("QuizScene_04");
                break;

            case "switch case":
                SceneManager.LoadScene("QuizScene_05");
                break;

            default:
                Debug.Log("沒有對應的測驗場景：" + currentLessonName);
                break;
        }
    }

    public void BackToLessonHome()
    {
        OpenLessonHome();
    }
    void Start()
    {
        HideAllPages();
        HideAllPopups();

        string openPanel = PlayerPrefs.GetString("OpenPanelAfterLoad", "");
        string returnPanel = PlayerPrefs.GetString("ReturnPanel", "");

        if (returnPanel == "Panel_Boating")
        {
            PlayerPrefs.DeleteKey("ReturnPanel");

            HideAllPages();
            Panel_Boating.SetActive(true);

            return; // 如果是從划船頁面返回，直接打開訓練首頁和划船頁面
        }

        if (openPanel == "LessonHome")
        {
            string lessonName = PlayerPrefs.GetString("ReturnLessonName", "");

            PlayerPrefs.DeleteKey("OpenPanelAfterLoad");
            PlayerPrefs.DeleteKey("ReturnLessonName");


            if (lessonName != "")
            {
                OpenLessonByName(lessonName);
            }
            else
            {
                OpenLessonHome();
            }
        }
        else
        {
            OpenLobby();
        }
    }
}
