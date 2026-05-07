using UnityEngine;
using UnityEngine.UI;

public class ClosetPreviewManager : MonoBehaviour
{
    [Header("角色穿戴部位")]
    public Image equippedHair;
    public Image equippedTop;
    public Image equippedBottom;
    public Image equippedAccessory;

    [Header("目前所有衣櫃物品")]
    public ClosetPreviewItem[] allItems;

    private Sprite savedHair;
    private Sprite savedTop;
    private Sprite savedBottom;
    private Sprite savedAccessory;

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
        Image target = GetTargetImage(item.part);
        if (target == null) return;

        if (target.sprite == item.itemSprite)
        {
            target.sprite = null;
            target.color = new Color(1, 1, 1, 0);
        }
        else
        {
            target.sprite = item.itemSprite;
            target.color = new Color(1, 1, 1, 1);
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
        equippedHair.sprite = savedHair;
        equippedTop.sprite = savedTop;
        equippedBottom.sprite = savedBottom;
        equippedAccessory.sprite = savedAccessory;

        SetImageVisible(equippedHair);
        SetImageVisible(equippedTop);
        SetImageVisible(equippedBottom);
        SetImageVisible(equippedAccessory);

        UpdateAllStatus();
        Debug.Log("已取消預覽");
    }

    void SaveCurrentOutfit()
    {
        savedHair = equippedHair.sprite;
        savedTop = equippedTop.sprite;
        savedBottom = equippedBottom.sprite;
        savedAccessory = equippedAccessory.sprite;
    }

    void UpdateAllStatus()
    {
        foreach (ClosetPreviewItem item in allItems)
        {
            Image target = GetTargetImage(item.part);
            bool isEquipped = target != null && target.sprite == item.itemSprite;
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

    void SetImageVisible(Image image)
    {
        if (image == null) return;

        image.color = image.sprite == null
            ? new Color(1, 1, 1, 0)
            : new Color(1, 1, 1, 1);
    }
}