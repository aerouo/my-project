using UnityEngine;
using UnityEngine.EventSystems;

public class SlotHandler : MonoBehaviour, IDropHandler
{
    public string slotType; // 在 Inspector 設定：Weather, True, 或 False

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedItem = eventData.pointerDrag;
        if (droppedItem == null) return;

        // 規則判斷
        if (slotType == "Weather")
        {
            if (droppedItem.name == "Sun")
            {
                SnapItem(droppedItem);
                Debug.Log("天氣填入正確！");
            }
            else
            {
                Debug.Log("這裡只能放 Sun 喔！");
            }
        }
        else if (slotType == "True" || slotType == "False")
        {
            // Hat 和 Umbrella 都可以放
            if (droppedItem.name == "Hat" || droppedItem.name == "umbrella")
            {
                SnapItem(droppedItem);
                // 這裡可以呼叫 GameManager 切換到你說的「不同畫面」
                CheckResult(droppedItem.name, slotType);
            }
        }
    }

    void SnapItem(GameObject item)
    {
        item.transform.position = transform.position; // 吸附到格子中心
        item.transform.SetParent(transform);
    }

    void CheckResult(string itemName, string slot)
    {
        PanelSwitcher manager = FindAnyObjectByType<PanelSwitcher>();

        // 找出兩個格子物件
        GameObject slotTrue = GameObject.Find("Slot_Hat");      // 請確認你 Hierarchy 的名稱
        GameObject slotFalse = GameObject.Find("Slot_umbrella"); // 請確認你 Hierarchy 的名稱

        // 檢查這兩個格子底下是否都有東西了
        if (slotTrue.transform.childCount > 0 && slotFalse.transform.childCount > 0)
        {
            // 取得這兩個格子裡面的物件名稱
            string itemInTrue = slotTrue.transform.GetChild(0).name;
            string itemInFalse = slotFalse.transform.GetChild(0).name;

            // 判定勝負
            if (itemInTrue == "Hat" && itemInFalse == "umbrella")
            {
                manager.ShowWin(); // 第二格是帽子、第三格是雨傘 -> 贏了！
            }
            else if (itemInTrue == "umbrella" && itemInFalse == "Hat")
            {
                manager.ShowLose(); // 第二格是雨傘、第三格是帽子 -> 輸了！
            }
        }
    }
}