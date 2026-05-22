using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Lvl2DropSlot : MonoBehaviour, IDropHandler
{
    public string acceptID;
    public bool isCorrect = false;

    [Header("各 BlockID 對應的 Sprite")]
    public Sprite sprite_class;
    public Sprite sprite_main;
    public Sprite sprite_print;
    public Sprite sprite_string;        // 原本的 string（""）
    public Sprite sprite_string_marks;  // 新的 string marks
    public Sprite sprite_name_num;      // name num
    public Sprite sprite_string_num;    // string num
    public Sprite sprite_equals;        // =
    public Sprite sprite_hum001;        // hum-001（原本的 num）
    public Sprite defaultSprite;

    private Lvl2DragBlock currentBlock;
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
        Lvl2DragBlock block = eventData.pointerDrag?.GetComponent<Lvl2DragBlock>();
        if (block == null) { Debug.Log("block 是 null！"); return; }

        Debug.Log("放入的 blockID: " + block.blockID);

        if (currentBlock != null)
            currentBlock.ReturnToOrigin();

        block.transform.SetParent(transform);
        block.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        block.GetComponent<Image>().enabled = false;
        currentBlock = block;

        isCorrect = (block.blockID == acceptID);
        slotImage.sprite = GetSpriteByID(block.blockID);

        Debug.Log("換圖完成，sprite: " + slotImage.sprite?.name);
    }

    Sprite GetSpriteByID(string blockID)
    {
        switch (blockID)
        {
            case "class": return sprite_class;
            case "main": return sprite_main;
            case "print": return sprite_print;
            case "string": return sprite_string;
            case "string marks": return sprite_string_marks;
            case "name num": return sprite_name_num;
            case "string num": return sprite_string_num;
            case "=": return sprite_equals;
            case "hum-001": return sprite_hum001;
            default:
                Debug.Log("找不到對應的 blockID: " + blockID);
                return defaultSprite;
        }
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
        return currentBlock != null ? currentBlock.blockID : "";
    }
}