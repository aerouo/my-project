using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class UI_PageController : MonoBehaviour
{
    [Header("Pages")]
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

    [Header("Popups")]
    public GameObject Popup_Video;

    private string currentLessonName = "";

    void Start()
    {
        HideAllPages();
        HideAllPopups();

        string openPanel = PlayerPrefs.GetString("OpenPanelAfterLoad", "");
        string returnPanel = PlayerPrefs.GetString("ReturnPanel", "");

        // ===== 返回 Boating =====
        if (returnPanel == "Panel_Boating")
        {
            PlayerPrefs.DeleteKey("ReturnPanel");

            OpenBoating();
            return;
        }

        // ===== 返回 MakeBoat =====
        if (returnPanel == "Panel_Makeboat")
        {
            PlayerPrefs.DeleteKey("ReturnPanel");

            OpenMakeboat();
            return;
        }

        // ===== 返回 Puzzle =====
        if (returnPanel == "Panel_Puzzle")
        {
            PlayerPrefs.DeleteKey("ReturnPanel");

            OpenPuzzle();
            return;
        }

        // ===== 返回 Clue =====
        if (returnPanel == "Panel_Clue")
        {
            PlayerPrefs.DeleteKey("ReturnPanel");

            OpenClue();
            return;
        }

        // ===== 返回 Forest =====
        if (returnPanel == "Panel_Forest")
        {
            PlayerPrefs.DeleteKey("ReturnPanel");

            OpenForest();
            return;
        }

        // ===== 返回 Lesson =====
        if (openPanel == "LessonHome")
        {
            string lessonName = PlayerPrefs.GetString("ReturnLessonName", "");

            PlayerPrefs.DeleteKey("OpenPanelAfterLoad");
            PlayerPrefs.DeleteKey("ReturnLessonName");

            if (!string.IsNullOrEmpty(lessonName))
                OpenLessonByName(lessonName);
            else
                OpenLessonHome();

            return;
        }

        OpenTrainingHome();
    }

    void HideAllPages()
    {
        SetActiveSafe(Panel_TrainingHome, false);
        SetActiveSafe(Panel_LearningList, false);
        SetActiveSafe(Panel_AdvancedList, false);
        SetActiveSafe(Panel_LessonHome, false);

        SetActiveSafe(Panel_Clue, false);
        SetActiveSafe(Panel_Puzzle, false);
        SetActiveSafe(Panel_Forest, false);
        SetActiveSafe(Panel_Makeboat, false);
        SetActiveSafe(Panel_Boating, false);
    }

    void HideAllPopups()
    {
        SetActiveSafe(Popup_Video, false);
    }

    void SetActiveSafe(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }

    public void BackToHomeScene()
    {
        SceneManager.LoadScene("home");
    }

    public void OpenTrainingHome()
    {
        HideAllPages();
        HideAllPopups();
        SetActiveSafe(Panel_TrainingHome, true);
    }

    public void OpenLearningList()
    {
        HideAllPages();
        HideAllPopups();
        SetActiveSafe(Panel_LearningList, true);
    }

    public void OpenAdvancedList()
    {
        HideAllPages();
        HideAllPopups();
        SetActiveSafe(Panel_AdvancedList, true);
    }

    public void OpenLessonHome()
    {
        HideAllPages();
        HideAllPopups();
        SetActiveSafe(Panel_LessonHome, true);
    }

    public void OpenClue()
    {
        HideAllPages();
        HideAllPopups();
        SetActiveSafe(Panel_Clue, true);
    }

    public void OpenPuzzle()
    {
        HideAllPages();
        HideAllPopups();
        SetActiveSafe(Panel_Puzzle, true);
    }

    public void OpenForest()
    {
        HideAllPages();
        HideAllPopups();
        SetActiveSafe(Panel_Forest, true);
    }

    public void OpenMakeboat()
    {
        HideAllPages();
        HideAllPopups();
        SetActiveSafe(Panel_Makeboat, true);
    }

    public void OpenBoating()
    {
        HideAllPages();
        HideAllPopups();
        SetActiveSafe(Panel_Boating, true);
    }

    public void GoToRowingLevel1()
    {
        SceneManager.LoadScene("Rowing_Lvl1");
    }

    public void GoToMakeBoatLevel1()
    {
        SceneManager.LoadScene("MakeBoat_Lvl1");
    }

    public void GoToPuzzleLevel1()
    {
        SceneManager.LoadScene("Puzzle_Lvl1");
    }

    public void GoToForestLevel1()
    {
        SceneManager.LoadScene("ForestExploration_Lvl1");
    }

    public void GoToClueLevel1()
    {
        SceneManager.LoadScene("FindClues_Lvl1");
    }

    public void OpenLessonByName(string lessonName)
    {
        currentLessonName = lessonName;

        Sprite lessonIcon = null;

        switch (lessonName)
        {
            case "基礎架構":
                SetLessonText("基礎架構", "基礎架構 教學影片", "基礎架構 基礎測驗");
                lessonIcon = Icon_Basic;
                break;

            case "print":
                SetLessonText("print", "print 教學影片", "print 基礎測驗");
                lessonIcon = Icon_Printf;
                break;

            case "if else":
                SetLessonText("if else", "if else 教學影片", "if else 基礎測驗");
                lessonIcon = Icon_IfElse;
                break;

            case "for":
                SetLessonText("for", "for 教學影片", "for 基礎測驗");
                lessonIcon = Icon_For;
                break;

            case "switch case":
                SetLessonText("switch case", "switch case 教學影片", "switch case 基礎測驗");
                lessonIcon = Icon_SwitchCase;
                break;

            default:
                SetLessonText(lessonName, lessonName + " 教學影片", lessonName + " 基礎測驗");
                break;
        }

        SetLessonTitleIcon(lessonIcon);
        OpenLessonHome();
    }

    void SetLessonText(string lessonTitle, string videoTitle, string quizTitle)
    {
        if (Txt_LessonTitle != null)
            Txt_LessonTitle.text = lessonTitle;

        if (Txt_VideoTitle != null)
            Txt_VideoTitle.text = videoTitle;

        if (Txt_QuizTitle != null)
            Txt_QuizTitle.text = quizTitle;
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

    public void OpenVideoPopup()
    {
        HideAllPopups();
        SetActiveSafe(Popup_Video, true);

        if (videoPlayer == null)
            return;

        videoPlayer.clip = GetVideoClipByLesson(currentLessonName);

        if (videoPlayer.clip == null)
        {
            Debug.LogWarning("沒有對應的影片：" + currentLessonName);
            return;
        }

        videoPlayer.Stop();
        videoPlayer.time = 0;
        videoPlayer.Play();
    }

    VideoClip GetVideoClipByLesson(string lessonName)
    {
        switch (lessonName)
        {
            case "基礎架構":
                return video_Basic;

            case "print":
                return video_Print;

            case "if else":
                return video_IfElse;

            case "for":
                return video_For;

            case "switch case":
                return video_SwitchCase;

            default:
                return null;
        }
    }

    public void CloseVideoPopup()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();

        SetActiveSafe(Popup_Video, false);
    }

    public void ToggleVideoPause()
    {
        if (videoPlayer == null)
            return;

        if (videoPlayer.isPlaying)
            videoPlayer.Pause();
        else
            videoPlayer.Play();
    }

    public void OpenQuizPanel()
    {
        string sceneName = GetQuizSceneByLesson(currentLessonName);

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("沒有對應的測驗場景：" + currentLessonName);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    string GetQuizSceneByLesson(string lessonName)
    {
        switch (lessonName)
        {
            case "基礎架構":
                return "QuizScene_01";

            case "print":
                return "QuizScene_02";

            case "if else":
                return "QuizScene_03";

            case "for":
                return "QuizScene_04";

            case "switch case":
                return "QuizScene_05";

            default:
                return "";
        }
    }

    public void BackToLessonHome()
    {
        OpenLessonHome();
    }
}