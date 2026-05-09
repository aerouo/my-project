using UnityEngine;
using UnityEngine.UI;

public class ClosetPreviewManager : MonoBehaviour
{
    [Header("角色穿戴部位")]
    public Image equippedHair;
    public Image equippedTop;
    public Image equippedBottom;
    public Image equippedAccessory;

    [Header("上衣物品")]
    public ClosetPreviewItem[] topItems;

    [Header("下衣物品")]
    public ClosetPreviewItem[] bottomItems;

    [Header("頭髮物品")]
    public ClosetPreviewItem[] hairItems;

    [Header("配件物品")]
    public ClosetPreviewItem[] accessoryItems;

    private Sprite savedHair;
    private Sprite savedTop;
    private Sprite savedBottom;
    private Sprite savedAccessory;

    private Vector2 savedHairSize;
    private Vector2 savedTopSize;
    private Vector2 savedBottomSize;
    private Vector2 savedAccessorySize;

    private Vector2 savedHairPosition;
    private Vector2 savedTopPosition;
    private Vector2 savedBottomPosition;
    private Vector2 savedAccessoryPosition;

    void Start()
    {
        SaveCurrentOutfit();
        UpdateAllStatus();
    }

    void OnDisable()
    {
        CancelPreview();
    }

    public void PreviewItem(ClosetPreviewItem item)
    {
        if (item == null || item.itemSprite == null)
            return;

        Image target = GetTargetImage(item.part);
        if (target == null)
            return;

        if (target.sprite == item.itemSprite)
        {
            target.sprite = null;
            target.color = new Color(1, 1, 1, 0);
        }
        else
        {
            target.sprite = item.itemSprite;
            target.color = new Color(1, 1, 1, 1);

            target.rectTransform.sizeDelta = item.equippedSize;
            target.rectTransform.anchoredPosition = item.equippedPosition;
        }

        UpdateAllStatus();
    }

    public void ApplyOutfit()
    {
        SaveCurrentOutfit();
        UpdateAllStatus();
        Debug.Log("已套用目前穿搭");
    }

    public void CancelPreview()
    {
        RestoreImage(equippedHair, savedHair, savedHairSize, savedHairPosition);
        RestoreImage(equippedTop, savedTop, savedTopSize, savedTopPosition);
        RestoreImage(equippedBottom, savedBottom, savedBottomSize, savedBottomPosition);
        RestoreImage(equippedAccessory, savedAccessory, savedAccessorySize, savedAccessoryPosition);

        UpdateAllStatus();
        Debug.Log("已取消預覽");
    }

    void SaveCurrentOutfit()
    {
        savedHair = equippedHair != null ? equippedHair.sprite : null;
        savedTop = equippedTop != null ? equippedTop.sprite : null;
        savedBottom = equippedBottom != null ? equippedBottom.sprite : null;
        savedAccessory = equippedAccessory != null ? equippedAccessory.sprite : null;

        savedHairSize = GetSize(equippedHair);
        savedTopSize = GetSize(equippedTop);
        savedBottomSize = GetSize(equippedBottom);
        savedAccessorySize = GetSize(equippedAccessory);

        savedHairPosition = GetPosition(equippedHair);
        savedTopPosition = GetPosition(equippedTop);
        savedBottomPosition = GetPosition(equippedBottom);
        savedAccessoryPosition = GetPosition(equippedAccessory);
    }

    Vector2 GetSize(Image image)
    {
        return image != null ? image.rectTransform.sizeDelta : Vector2.zero;
    }

    Vector2 GetPosition(Image image)
    {
        return image != null ? image.rectTransform.anchoredPosition : Vector2.zero;
    }

    void UpdateAllStatus()
    {
        UpdateStatusGroup(topItems);
        UpdateStatusGroup(bottomItems);
        UpdateStatusGroup(hairItems);
        UpdateStatusGroup(accessoryItems);
    }

    void UpdateStatusGroup(ClosetPreviewItem[] items)
    {
        if (items == null)
            return;

        foreach (ClosetPreviewItem item in items)
        {
            if (item == null)
                continue;

            Image target = GetTargetImage(item.part);

            bool isEquipped =
                target != null &&
                item.itemSprite != null &&
                target.sprite == item.itemSprite;

            item.SetStatus(isEquipped);
        }
    }

    Image GetTargetImage(ClosetPart part)
    {
        switch (part)
        {
            case ClosetPart.Hair:
                return equippedHair;

            case ClosetPart.Top:
                return equippedTop;

            case ClosetPart.Bottom:
                return equippedBottom;

            case ClosetPart.Accessory:
                return equippedAccessory;

            default:
                return null;
        }
    }

    void RestoreImage(Image image, Sprite sprite, Vector2 size, Vector2 position)
    {
        if (image == null)
            return;

        image.sprite = sprite;
        image.rectTransform.sizeDelta = size;
        image.rectTransform.anchoredPosition = position;

        SetImageVisible(image);
    }

    void SetImageVisible(Image image)
    {
        if (image == null)
            return;

        image.color = image.sprite == null
            ? new Color(1, 1, 1, 0)
            : new Color(1, 1, 1, 1);
    }
}