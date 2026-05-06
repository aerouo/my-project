using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public string acceptID; // 這個槽接受哪個blockID
    public bool isCorrect = false;

    private DragBlock currentBlock;

    public void OnDrop(PointerEventData eventData)
    {
        DragBlock block = eventData.pointerDrag.GetComponent<DragBlock>();
        if (block == null) return;

        // 如果槽已經有方塊，把舊的退回原位
        if (currentBlock != null)
        {
            currentBlock.ReturnToOrigin();
        }

        // 把方塊放進槽
        block.transform.SetParent(transform);
        block.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        currentBlock = block;

        // 判斷對不對
        isCorrect = (block.blockID == acceptID);

        // 通知 GameManager 檢查
        FindObjectOfType<PrintGameManager>().CheckAnswer();
    }
}