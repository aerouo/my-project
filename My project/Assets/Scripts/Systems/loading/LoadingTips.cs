using UnityEngine;
using TMPro;

public class LoadingTips : MonoBehaviour
{
    public TextMeshProUGUI tipText;

   
    private string[] tips = new string[]
    {
        "print就是.....",
        "if 你給我錢 我就會很有錢",
        "分號跟程式碼是不可拆開的生命共同體",
        "希望我的財富跟for一樣不要停",
        
    };

    void Start()
    {
        // 每次進入 Loading，就隨機選一句
        int index = Random.Range(0, tips.Length);
        tipText.text = tips[index];
    }
}
