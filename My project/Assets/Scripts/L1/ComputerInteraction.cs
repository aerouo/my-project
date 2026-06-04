using UnityEngine;

public class ComputerInteraction : MonoBehaviour
{
    public GameObject computerPanel;   // 拖入 computer
    public GameObject printGroupPanel; // 拖入 print group

    // 點電腦 → 開啟拖曳視窗
    public void OnClickComputer()
    {
        if (computerPanel != null) computerPanel.SetActive(false);
        if (printGroupPanel != null) printGroupPanel.SetActive(true);
    }

    // 返回電腦 → 關閉拖曳視窗
    public void OnClickRetrun()
    {
        if (printGroupPanel != null) printGroupPanel.SetActive(false);
        if (computerPanel != null) computerPanel.SetActive(true);
    }
}