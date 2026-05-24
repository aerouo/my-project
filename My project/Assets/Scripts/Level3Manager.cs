using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Level3Manager : MonoBehaviour
{
    [Header("UI ₪¸¥ף")]
    public TextMeshProUGUI dialogueText;
    public Image backgroundImage;

    [Header("²ִ₪@¬q¼@±¡¡]«ק¹ֿ«e¡^")]
    [TextArea(10, 20)]
    public string storyPart1;

    [Header("²ִ₪G¬q¼@±¡¡]«ק¹ֿ«ב¡^")]
    [TextArea(10, 20)]
    public string storyPart2;

    [Header("¼@±¡³]©w")]
    public float typingSpeed = 0.05f;
    public float waitBeforeStory = 0.5f;
    public float timeBetweenLines = 1.5f;

    [Header("­I´÷¹ֿ₪ש®w")]
    public Sprite scene1;
    public Sprite scene2;
    public Sprite scene3;

    [Header("₪p¹Cְ¸»־±µ")]
    public GameObject miniGameCanvas;
    public GameObject miniGameBackground;

    [Header("²ִ₪G¬q¼@±¡µ²§פ«ב»־±µ")]
    public GameObject nextGameUI;

    private bool eatingCompleted = false;
    private int currentScene = 1;

    void Start()
    {
        if (backgroundImage != null && scene1 != null) backgroundImage.sprite = scene1;
        if (dialogueText != null) dialogueText.color = Color.black;

        if (miniGameCanvas != null) miniGameCanvas.SetActive(false);
        if (miniGameBackground != null) miniGameBackground.SetActive(false);
        if (nextGameUI != null) nextGameUI.SetActive(false);

        if (dialogueText != null)
        {
            dialogueText.text = "";
            StartCoroutine(PlayPart1());
        }
    }

    // שששששששששששששששששששששששששששששש ²ִ₪@¬q¼@±¡ שששששששששששששששששששששששששששששששששששששששששששש

    IEnumerator PlayPart1()
    {
        yield return new WaitForSeconds(waitBeforeStory);
        yield return StartCoroutine(PlayStory(storyPart1));

        dialogueText.text = "";
        if (miniGameCanvas != null) miniGameCanvas.SetActive(true);
        if (miniGameBackground != null) miniGameBackground.SetActive(true);

        Debug.Log("¡i¼@±¡±±¨מ¡j²ִ₪@¬qµ²§פ¡Aµ¥«Ý¦Y×F¦ט₪p¹Cְ¸§¹¦¨...");
    }

    // שששששששששששששששששששששששששששששש ¦Y×F¦ט₪p¹Cְ¸§¹¦¨³q×¾ שששששששששששששששששששששששששששששששששששששששש
    // ¥ׁ EatingGame ×÷ RunGame() ₪T½üµ²§פ«ב©I¥s

    public void OnEatingComplete()
    {
        if (eatingCompleted) return;
        eatingCompleted = true;

        // ֱפֲֳ₪p¹Cְ¸
        if (miniGameCanvas != null) miniGameCanvas.SetActive(false);
        if (miniGameBackground != null) miniGameBackground.SetActive(false);

        // ´«¦¨ scene2
        if (backgroundImage != null && scene2 != null)
        {
            backgroundImage.sprite = scene2;
        }

        Debug.Log("¡i¼@±¡±±¨מ¡j¦Y×F¦ט§¹¦¨¡A¶}©l²ִ₪G¬q¼@±¡");
        StartCoroutine(PlayPart2());
    }

    // שששששששששששששששששששששששששששששש ²ִ₪G¬q¼@±¡ שששששששששששששששששששששששששששששששששששששששששששש

    IEnumerator PlayPart2()
    {
        if (dialogueText != null) dialogueText.text = "";
        yield return StartCoroutine(PlayStory(storyPart2));

        dialogueText.text = "";
        if (nextGameUI != null) nextGameUI.SetActive(true);
        Debug.Log("¡i¼@±¡±±¨מ¡j²ִ₪G¬qµ²§פ¡A¶}±ׂ₪U₪@­׃ UI");
    }

    // שששששששששששששששששששששששששששששש ³q¥־¼@±¡¼½©ס שששששששששששששששששששששששששששששששששששששששש

    IEnumerator PlayStory(string content)
    {
        if (string.IsNullOrEmpty(content)) yield break;

        content = content.Replace("\r", "");
        string[] lines = content.Split('\n');

        foreach (string line in lines)
        {
            string currentLine = line.Trim();
            if (string.IsNullOrEmpty(currentLine)) continue;

            if (currentLine.Contains("[´«¹ֿ]"))
            {
                currentScene++;
                Sprite nextSprite = currentScene == 2 ? scene2 : scene3;
                if (backgroundImage != null && nextSprite != null)
                {
                    backgroundImage.sprite = nextSprite;
                    Debug.Log("¡i¼@±¡±±¨מ¡j­I´÷₪ֱ´«¨ל scene" + currentScene);
                }
                continue;
            }

            if (dialogueText != null) dialogueText.text = "";
            foreach (char letter in currentLine.ToCharArray())
            {
                if (dialogueText != null) dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
            yield return new WaitForSeconds(timeBetweenLines);
        }
    }

    // שששששששששששששששששששששששששששששש ¸ץ¹L שששששששששששששששששששששששששששששששששששששששששששששששששששששששש

    public void SkipStory()
    {
        StopAllCoroutines();
        if (dialogueText != null) dialogueText.text = "";

        if (!eatingCompleted)
        {
            if (miniGameCanvas != null) miniGameCanvas.SetActive(true);
            if (miniGameBackground != null) miniGameBackground.SetActive(true);
        }
        else
        {
            if (nextGameUI != null) nextGameUI.SetActive(true);
        }
    }
}