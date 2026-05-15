using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public string acceptID;
    public bool isCorrect = false;

    public Sprite sprite_class;
    public Sprite sprite_main;
    public Sprite sprite_print;
    public Sprite sprite_string;
    public Sprite sprite_num;
    public Sprite defaultSprite;

    private DragBlock currentBlock;
    private Image slotImage;

    void Awake()
    {
        slotImage = GetComponent<Image>();
        if (defaultSprite == null)
            defaultSprite = slotImage.sprite;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop 觸發！");

        DragBlock block = eventData.pointerDrag.GetComponent<DragBlock>();
        if (block == null)
        {
            Debug.Log("block 是 null！");
            return;
        }

        Debug.Log("放入的 blockID: " + block.blockID);

        // 如果槽已有方塊，退回原位
        if (currentBlock != null)
            currentBlock.ReturnToOrigin();

        block.transform.SetParent(transform);
        block.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        block.GetComponent<Image>().enabled = false;
        currentBlock = block;
        isCorrect = (block.blockID == acceptID);

        switch (block.blockID)
        {
            case "class": slotImage.sprite = sprite_class; break;
            case "main": slotImage.sprite = sprite_main; break;
            case "print": slotImage.sprite = sprite_print; break;
            case "string": slotImage.sprite = sprite_string; break;
            case "num": slotImage.sprite = sprite_num; break;
            default:
                Debug.Log("找不到對應的 blockID: " + block.blockID);
                break;
        }

        Debug.Log("換圖完成，sprite: " + slotImage.sprite?.name);
        // 不在這裡 CheckAnswer，改由完成按鈕觸發
    }

    public void ResetSlot()
    {
        if (currentBlock != null)
        {
            currentBlock.ReturnToOrigin();
            currentBlock = null;
        }
        if (slotImage != null)
            slotImage.sprite = defaultSprite;
        isCorrect = false;
    }
    public string GetCurrentBlockID()
    {
        if (currentBlock != null)
            return currentBlock.blockID;
        return "";
    }
}