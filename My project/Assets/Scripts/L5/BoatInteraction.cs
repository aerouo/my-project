using UnityEngine;

public class BoatInteraction : MonoBehaviour
{
    public GameObject boatPanel;     // 拖入 Boat
    public GameObject forGroupPanel; // 拖入 for group

    public void OnClickBoat()
    {
        if (forGroupPanel != null) forGroupPanel.SetActive(true);
    }

    public void OnClickReturn()
    {
        if (forGroupPanel != null) forGroupPanel.SetActive(false);
    }
}