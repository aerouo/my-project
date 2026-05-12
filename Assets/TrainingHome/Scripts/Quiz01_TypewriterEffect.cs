using UnityEngine;
using TMPro;
using System.Collections;

public class Quiz01_TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro; // 拖入你的 TutorialText
    public string fullText = "學習完基礎教學了 !\r\n讓我們來一個小測驗試試吧 ! !";
    public float typingSpeed = 0.1f; // 每個字出現的間隔秒數


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