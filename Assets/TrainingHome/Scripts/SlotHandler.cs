
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotHandler : MonoBehaviour, IDropHandler
{
    public string slotType; // Inspector 設定：Weather、True、False

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedItem = eventData.pointerDrag;
        if (droppedItem == null) return;

        bool placed = false;

        if (slotType == "Weather")
        {
            if (droppedItem.name == "Sun")
            {
                SnapItem(droppedItem);
                placed = true;
            }
            else
            {
                Debug.Log("這裡只能放 Sun");
            }
        }
        else if (slotType == "True" || slotType == "False")
        {
            if (droppedItem.name == "Hat" || droppedItem.name == "umbrella")
            {
                SnapItem(droppedItem);
                placed = true;
            }
            else
            {
                Debug.Log("這裡只能放 Hat 或 umbrella");
            }
        }

        // 不管哪一格最後放，只要成功放入，就檢查三格是否都完成
        if (placed)
        {
            CheckResult();
        }
    }

    void SnapItem(GameObject item)
    {
        item.transform.SetParent(transform);
        item.transform.position = transform.position;
    }

    void CheckResult()
    {
        PanelSwitcher manager = FindAnyObjectByType<PanelSwitcher>();

        GameObject slotWeather = GameObject.Find("Slot_Weather");
        GameObject slotTrue = GameObject.Find("Slot_Hat");
        GameObject slotFalse = GameObject.Find("Slot_umbrella");

        if (manager == null || slotWeather == null || slotTrue == null || slotFalse == null)
        {
            Debug.LogWarning("找不到 PanelSwitcher 或 Slot 物件，請檢查 Hierarchy 名稱");
            return;
        }

        // 三個格子都要有東西，才判斷結果
        if (slotWeather.transform.childCount == 0 ||
            slotTrue.transform.childCount == 0 ||
            slotFalse.transform.childCount == 0)
        {
            Debug.Log("還沒填完三個欄位，暫不判斷結果");
            return;
        }

        string itemInWeather = slotWeather.transform.GetChild(0).name;
        string itemInTrue = slotTrue.transform.GetChild(0).name;
        string itemInFalse = slotFalse.transform.GetChild(0).name;

        if (itemInWeather == "Sun" &&
            itemInTrue == "Hat" &&
            itemInFalse == "umbrella")
        {
            manager.ShowWin();
        }
        else
        {
            manager.ShowLose();
        }
    }
}