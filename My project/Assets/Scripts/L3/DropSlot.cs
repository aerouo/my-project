using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public string acceptID;
    public bool isCorrect = false;

    [Header("各 BlockID 對應的 Sprite")]
    public Sprite sprite_class;
    public Sprite sprite_main;
    public Sprite sprite_print;
    public Sprite sprite_string_marks;
    public Sprite sprite_0;
    public Sprite sprite_1;
    public Sprite sprite_equals; //=
    public Sprite sprite_equalsequals;  // ==
    public Sprite sprite_int;
    public Sprite sprite_int_name;
    public Sprite sprite_int_see;
    public Sprite sprite_if;
    public Sprite sprite_else;
    public Sprite sprite_yes;
    public Sprite sprite_no;

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
        DragBlock block = eventData.pointerDrag?.GetComponent<DragBlock>();
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
            case "string marks": return sprite_string_marks;            
            case "=": return sprite_equals;            
            case "0": return sprite_0;
            case "1": return sprite_1;
            case "==": return sprite_equalsequals;
            case "int": return sprite_int;
            case "int name": return sprite_int_name;
            case "int see": return sprite_int_see;
            case "if": return sprite_if;
            case "else": return sprite_else;
            case "yes": return sprite_yes;
            case "no": return sprite_no;
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