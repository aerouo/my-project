using UnityEngine;

public class PuaperInteraction : MonoBehaviour
{
    public GameObject puaperPanel;   // 拖入 puaper
    public GameObject printGroupPanel; // 拖入 print group

    // 點便條紙 → 開啟拖曳視窗
    public void OnClickPuaper()
    {
        if (puaperPanel != null) puaperPanel.SetActive(false);
        if (printGroupPanel != null) printGroupPanel.SetActive(true);
    }

    // 返回便條紙 → 關閉拖曳視窗
    public void OnClickRetrun()
    {
        if (printGroupPanel != null) printGroupPanel.SetActive(false);
        if (puaperPanel != null) puaperPanel.SetActive(true);
    }
}