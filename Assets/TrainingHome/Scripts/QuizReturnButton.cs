using UnityEngine;
using UnityEngine.SceneManagement;

public class QuizReturnButton : MonoBehaviour
{
    public void BackToIfElseLesson()
    {   // 儲存要返回的課程名稱， "if else"
        PlayerPrefs.SetString("ReturnLesson", "ifelse");
        SceneManager.LoadScene("SampleScene"); 
    }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

