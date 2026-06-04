using UnityEngine;

public class DraftInteraction : MonoBehaviour
{
    public GameObject draftPanel;   // 拖入 draft
    public GameObject switchcaseGroupPanel; // 拖入 switchcase group

    // 點草稿 → 開啟拖曳視窗
    public void OnClickDraft()
    {
        if (draftPanel != null) draftPanel.SetActive(false);
        if (switchcaseGroupPanel != null) switchcaseGroupPanel.SetActive(true);
    }

    // 返回草稿 → 關閉拖曳視窗
    public void OnClickRetrun()
    {
        if (switchcaseGroupPanel != null) switchcaseGroupPanel.SetActive(false);
        if (draftPanel != null) draftPanel.SetActive(true);
    }
}