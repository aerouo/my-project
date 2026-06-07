using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ChoiceBoatGame : MonoBehaviour
{
    [Header("背景")]
    public Image bgImage;
    public Sprite bgNormal;
    public Sprite bgFog;

    [Header("Boat 圖片")]
    public Image boatImage;
    public Sprite boatGood;
    public Sprite boatBad;

    [Header("On Boat 圖片")]
    public Image onBoatImage;
    public Sprite onBoatDefault;  // 原本的圖
    public Sprite onBoatRescue;   // 按上鍵（好船）
    public Sprite onBoatHide;     // 按下鍵（壞船）

    [Header("設定")]
    public float fogDelay = 1f;
    public float boatShowDelay = 1f;
    public float holdDuration = 3f;

    [Header("完成通知")]
    public Level5Manager level5Manager;

    private int currentRound = 0;
    private bool[] roundIsGood = new bool[4];
    private bool isPlaying = false;
    private bool roundSuccess = false;
    private float holdTimer = 0f;

    void Start()
    {
        // 1,3輪 bad / 2,4輪 good
        roundIsGood[0] = false;
        roundIsGood[1] = true;
        roundIsGood[2] = false;
        roundIsGood[3] = true;
    }

    public void StartGame()
    {
        Debug.Log("StartGame 被呼叫！");
        StartCoroutine(RunAllRounds());
    }

    IEnumerator RunAllRounds()
    {
        for (currentRound = 0; currentRound < 4; currentRound++)
        {
            bool success = false;
            while (!success)
            {
                yield return StartCoroutine(PlayRound(roundIsGood[currentRound], result => success = result));
            }
        }

        if (level5Manager != null)
            level5Manager.OnChoiceBoat();
    }

    IEnumerator PlayRound(bool isGood, System.Action<bool> callback)
    {
        // 換回預設圖
        if (onBoatImage != null) onBoatImage.sprite = onBoatDefault;

        // 1. 隱藏船，換濃霧
        if (boatImage != null) boatImage.gameObject.SetActive(false);
        if (bgImage != null) bgImage.sprite = bgFog;
        yield return new WaitForSeconds(fogDelay);

        // 2. 換船圖片
        if (boatImage != null) boatImage.sprite = isGood ? boatGood : boatBad;
        yield return new WaitForSeconds(boatShowDelay);

        // 3. 顯示船，換回正常
        if (boatImage != null) boatImage.gameObject.SetActive(true);
        if (bgImage != null) bgImage.sprite = bgNormal;

        // 4. 重置
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        holdTimer = 0f;
        roundSuccess = false;
        isPlaying = true;

        // 5. 等待成功
        yield return new WaitUntil(() => roundSuccess);
        isPlaying = false;

        yield return new WaitForSeconds(1f);

        // 換回預設圖準備下一輪
        if (onBoatImage != null) onBoatImage.sprite = onBoatDefault;

        callback(true);
    }

    void Update()
    {
        if (!isPlaying) return;

        bool isGood = roundIsGood[currentRound];

        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (onBoatImage != null) onBoatImage.sprite = onBoatRescue;
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdDuration && isGood)
                roundSuccess = true;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            if (onBoatImage != null) onBoatImage.sprite = onBoatHide;
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdDuration && !isGood)
                roundSuccess = true;
        }
        else
        {
            if (onBoatImage != null) onBoatImage.sprite = onBoatDefault;
            holdTimer = 0f;
        }
    }
}