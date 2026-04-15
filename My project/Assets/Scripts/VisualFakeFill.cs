using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class VisualFakeFill : MonoBehaviour, IDropHandler
{
    private Image slotImage;

    [Header("輸入框格子")]
    public TMP_InputField input1; // 條件/第一個變數
    public TMP_InputField input2; // if內容/for中間格/switch內容
    public TMP_InputField input3; // else內容/for最後格
    public TMP_InputField input4; // for大括號內容

    [Header("符號物件 (TextMeshPro)")]
    public TextMeshProUGUI semi_Start;  // "if (" / "for (" / "System..."
    public TextMeshProUGUI semi_Mid1;   // ") {" / ";" / "){"
    public TextMeshProUGUI semi_Mid2;   // "} else {" / ";"
    public TextMeshProUGUI semi_Mid3;   // ") {" (給for用)
    public TextMeshProUGUI semi_End;    // "}" / ");" / "};"

    public string currentStoredCode = "";

    void Awake()
    {
        slotImage = GetComponent<Image>();
    }

    void Start()
    {
        // 1. 遊戲一開始顯示虛線框
        if (slotImage != null) slotImage.enabled = true;

        // 2. 隱藏所有拼圖組件，避免一開始看到 New Text
        ResetUI();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null) return;

        // 物理吸附
        draggedObject.GetComponent<RectTransform>().position = transform.position;
        currentStoredCode = draggedObject.name;

        // 成功拖入後，虛線框被「取代」 (隱藏圖片)
        if (slotImage != null) slotImage.enabled = false;

        UpdateInterface(currentStoredCode);
    }

    private void UpdateInterface(string codeName)
    {
        ResetUI();

        // 💡 參數對應：(i1, i2, i3, i4, sStart, sM1, sM2, sM3, sEnd)
        if (codeName.Contains("if else"))
        {
            semi_Start.text = "if (";
            semi_Mid1.text = ") {";
            semi_Mid2.text = "} else {";
            semi_End.text = "}";
            // if ( [in1] ) { [in2] } else { [in3] }
            // 開啟：i1, i2, i3, sStart, sM1, sM2, sEnd
            ToggleElements(true, true, true, false, true, true, true, false, true);
        }
        else if (codeName.Contains("for"))
        {
            semi_Start.text = "for (";
            semi_Mid1.text = ";";
            semi_Mid2.text = ";";
            semi_Mid3.text = ") {";
            semi_End.text = "}";
            // for ( [in1] ; [in2] ; [in3] ) { [in4] }
            // 開啟：全開
            ToggleElements(true, true, true, true, true, true, true, true, true);
        }
        else if (codeName.Contains("print"))
        {
            semi_Start.text = "System.out.println(\"";
            semi_End.text = "\");";
            // System.out.println( [in1] );
            // 開啟：i1, sStart, sEnd
            ToggleElements(true, false, false, false, true, false, false, false, true);
        }
        else if (codeName.Contains("switch case"))
        {
            semi_Start.text = "switch(";
            semi_Mid1.text = "){";
            semi_End.text = "};";
            // switch( [in1] ) { [in2] };
            // 開啟：i1, i2, sStart, sM1, sEnd
            ToggleElements(true, true, false, false, true, true, false, false, true);
        }
    }

    // 核心開關控制器
    private void ToggleElements(bool i1, bool i2, bool i3, bool i4, bool sStart, bool sM1, bool sM2, bool sM3, bool sEnd)
    {
        if (input1) input1.gameObject.SetActive(i1);
        if (input2) input2.gameObject.SetActive(i2);
        if (input3) input3.gameObject.SetActive(i3);
        if (input4) input4.gameObject.SetActive(i4);

        if (semi_Start) semi_Start.gameObject.SetActive(sStart);
        if (semi_Mid1) semi_Mid1.gameObject.SetActive(sM1);
        if (semi_Mid2) semi_Mid2.gameObject.SetActive(sM2);
        if (semi_Mid3) semi_Mid3.gameObject.SetActive(sM3);
        if (semi_End) semi_End.gameObject.SetActive(sEnd);
    }

    private void ResetUI()
    {
        // 關閉所有拼圖物件
        ToggleElements(false, false, false, false, false, false, false, false, false);

        // 清空文字
        if (input1) input1.text = "";
        if (input2) input2.text = "";
        if (input3) input3.text = "";
        if (input4) input4.text = "";
    }
}