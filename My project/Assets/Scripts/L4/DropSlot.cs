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
    public Sprite sprite_string;
    public Sprite sprite_string_marks;
    public Sprite sprite_string_name;
    public Sprite sprite_string_mark;
    public Sprite sprite_equals;
    public Sprite sprite_minus_x;
    public Sprite sprite_minus;
    public Sprite sprite_x;
    public Sprite sprite_char;
    public Sprite sprite_char_name;
    public Sprite sprite_blueprint;
    public Sprite sprite_switch;
    public Sprite sprite_case;
    public Sprite sprite_char_symbol;
    public Sprite sprite_place_wood;
    public Sprite sprite_break;
    public Sprite sprite_binding_rope;
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
            case "string": return sprite_string;
            case "string marks": return sprite_string_marks;
            case "string name": return sprite_string_name;
            case "string mark": return sprite_string_mark;
            case "=": return sprite_equals;
            case "-x": return sprite_minus_x;
            case "-": return sprite_minus;
            case "x": return sprite_x;
            case "char": return sprite_char;
            case "char name": return sprite_char_name;
            case "blueprint": return sprite_blueprint;
            case "switch": return sprite_switch;
            case "case": return sprite_case;
            case "char symbol": return sprite_char_symbol;
            case "place wood": return sprite_place_wood;
            case "break": return sprite_break;
            case "binding rope": return sprite_binding_rope;
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