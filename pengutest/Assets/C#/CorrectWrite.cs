using UnityEngine;
using TMPro;
using System.Collections;

public class CorrectWrite : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro; // 拖入你的 TutorialText
    public string fullText = "恭喜你答對啦 ! ! !\r\n\r\n讓我們接下去學習吧 !\r\n\r\nGoGoGoooo~~";
    public float typingSpeed = 0.1f; // 每個字出現的間隔秒數

    //歡迎來到企鵝程式室！\n請按照提示把方塊拉到底線上。

    void Start()
    {
        // 遊戲開始時先清空文字，然後開始打字
        textMeshPro.text = "";
        StartCoroutine(ShowText());
    }

    IEnumerator ShowText()
    {
        foreach (char c in fullText)
        {
            textMeshPro.text += c;
            // 這裡可以加入一個簡單的打字音效
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
