using System.Collections;
using TMPro;
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

    [Header("紙鶴金額")]
    public int money = 9999999;
    public TMP_Text txtMoney;

    [Header("購買確認視窗")]
    public GameObject panelBuyConfirm;
    public TMP_Text txtBuyMessage;
    public Button btnBuyYes;
    public Button btnBuyNo;

    [Header("連身衣設定")]
    public ClosetPreviewItem top01OnePiece;
    public ClosetPreviewItem top02OnePiece;
    public ClosetPreviewItem defaultTopItem; // 拖 Top06

    private ClosetPreviewItem selectedItem;

    private OutfitState savedHair;
    private OutfitState savedTop;
    private OutfitState savedBottom;
    private OutfitState savedAccessory;

    [System.Serializable]
    private class OutfitState
    {
        public Sprite sprite;
        public Color color;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector3 localScale;
        public Quaternion localRotation;
    }

    void Start()
    {
        SaveCurrentOutfit();
        UpdateMoneyUI();

        if (panelBuyConfirm != null)
            panelBuyConfirm.SetActive(false);

        if (btnBuyYes != null)
        {
            btnBuyYes.onClick.RemoveListener(ConfirmBuy);
            btnBuyYes.onClick.AddListener(ConfirmBuy);
        }

        if (btnBuyNo != null)
        {
            btnBuyNo.onClick.RemoveListener(CancelBuy);
            btnBuyNo.onClick.AddListener(CancelBuy);
        }

        RefreshAllPriceText();
        HideEmptyEquippedImages();

        StartCoroutine(DelayUpdateStatus());
    }

    IEnumerator DelayUpdateStatus()
    {
        yield return null;

        HideEmptyEquippedImages();
        UpdateAllStatus();
    }

    void OnDisable()
    {
        CancelPreview();
    }

    public void ClickItem(ClosetPreviewItem item)
    {
        if (item == null || item.itemSprite == null)
            return;

        if (!item.isOwned)
        {
            OpenBuyConfirm(item);
            return;
        }

        PreviewItem(item);
    }

    void OpenBuyConfirm(ClosetPreviewItem item)
    {
        selectedItem = item;

        if (txtBuyMessage != null)
            txtBuyMessage.text = "是否花費 " + item.price + "\n紙鶴購買？";

        if (panelBuyConfirm != null)
            panelBuyConfirm.SetActive(true);
    }

    void ConfirmBuy()
    {
        if (selectedItem == null)
            return;

        if (money < selectedItem.price)
        {
            if (txtBuyMessage != null)
                txtBuyMessage.text = "紙鶴不足\n無法購買";
            return;
        }

        money -= selectedItem.price;
        selectedItem.SetOwned(true);

        UpdateMoneyUI();

        if (panelBuyConfirm != null)
            panelBuyConfirm.SetActive(false);

        EquipItem(selectedItem);

        selectedItem = null;
    }

    void EquipItem(ClosetPreviewItem item)
    {
        if (item == null || item.itemSprite == null)
            return;

        ForceEquipItem(item);
        HandleSpecialOutfitRule(item);
        HideEmptyEquippedImages();
        UpdateAllStatus();
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
            ForceEquipItem(item);
            HandleSpecialOutfitRule(item);
        }

        HideEmptyEquippedImages();
        UpdateAllStatus();
    }

    void ForceEquipItem(ClosetPreviewItem item)
    {
        if (item == null || item.itemSprite == null)
            return;

        Image target = GetTargetImage(item.part);
        if (target == null)
            return;

        target.sprite = item.itemSprite;
        target.color = new Color(1, 1, 1, 1);

        ApplyItemTransform(target, item);
    }

    void HandleSpecialOutfitRule(ClosetPreviewItem item)
    {
        // 穿 Top01 / Top02 連身衣時，自動取消下衣
        if (item.part == ClosetPart.Top && IsOnePieceTop(item))
        {
            ClearEquippedImage(equippedBottom);
            return;
        }

        // 穿下衣時，如果目前上衣是連身衣，就換回 Top06
        if (item.part == ClosetPart.Bottom && IsCurrentTopOnePiece())
        {
            if (defaultTopItem != null)
                ForceEquipItem(defaultTopItem);
        }
    }

    bool IsOnePieceTop(ClosetPreviewItem item)
    {
        return item == top01OnePiece || item == top02OnePiece;
    }

    bool IsCurrentTopOnePiece()
    {
        return IsSpriteMatch(equippedTop, top01OnePiece) ||
               IsSpriteMatch(equippedTop, top02OnePiece);
    }

    bool IsSpriteMatch(Image image, ClosetPreviewItem item)
    {
        return image != null &&
               item != null &&
               image.sprite != null &&
               image.sprite == item.itemSprite;
    }

    void ClearEquippedImage(Image image)
    {
        if (image == null)
            return;

        image.sprite = null;
        image.color = new Color(1, 1, 1, 0);
    }

    void HideEmptyEquippedImages()
    {
        HideIfEmpty(equippedHair);
        HideIfEmpty(equippedTop);
        HideIfEmpty(equippedBottom);
        HideIfEmpty(equippedAccessory);
    }

    void HideIfEmpty(Image image)
    {
        if (image == null)
            return;

        if (image.sprite == null)
            image.color = new Color(1, 1, 1, 0);
    }

    void CancelBuy()
    {
        selectedItem = null;

        if (panelBuyConfirm != null)
            panelBuyConfirm.SetActive(false);
    }

    void ApplyItemTransform(Image target, ClosetPreviewItem item)
    {
        RectTransform targetRect = target.GetComponent<RectTransform>();

        if (targetRect == null)
            return;

        targetRect.anchoredPosition = item.wearAnchoredPosition;
        targetRect.sizeDelta = item.wearSizeDelta;
        targetRect.localScale = item.wearScale;
        targetRect.localEulerAngles = item.wearRotation;
    }

    public void ApplyOutfit()
    {
        SaveCurrentOutfit();
        UpdateAllStatus();
    }

    public void CancelPreview()
    {
        RestoreSavedOutfit();
        HideEmptyEquippedImages();
        UpdateAllStatus();
    }

    void SaveCurrentOutfit()
    {
        savedHair = SaveOutfitState(equippedHair);
        savedTop = SaveOutfitState(equippedTop);
        savedBottom = SaveOutfitState(equippedBottom);
        savedAccessory = SaveOutfitState(equippedAccessory);
    }

    OutfitState SaveOutfitState(Image image)
    {
        if (image == null)
            return null;

        RectTransform rect = image.GetComponent<RectTransform>();

        OutfitState state = new OutfitState();
        state.sprite = image.sprite;
        state.color = image.color;

        if (rect != null)
        {
            state.anchoredPosition = rect.anchoredPosition;
            state.sizeDelta = rect.sizeDelta;
            state.localScale = rect.localScale;
            state.localRotation = rect.localRotation;
        }

        return state;
    }

    void RestoreSavedOutfit()
    {
        RestoreOutfitState(equippedHair, savedHair);
        RestoreOutfitState(equippedTop, savedTop);
        RestoreOutfitState(equippedBottom, savedBottom);
        RestoreOutfitState(equippedAccessory, savedAccessory);
    }

    void RestoreOutfitState(Image image, OutfitState state)
    {
        if (image == null || state == null)
            return;

        image.sprite = state.sprite;
        image.color = state.color;

        RectTransform rect = image.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchoredPosition = state.anchoredPosition;
            rect.sizeDelta = state.sizeDelta;
            rect.localScale = state.localScale;
            rect.localRotation = state.localRotation;
        }

        if (image.sprite == null)
            image.color = new Color(1, 1, 1, 0);
    }

    Image GetTargetImage(ClosetPart part)
    {
        switch (part)
        {
            case ClosetPart.Top:
                return equippedTop;

            case ClosetPart.Bottom:
                return equippedBottom;

            case ClosetPart.Hair:
                return equippedHair;

            case ClosetPart.Accessory:
                return equippedAccessory;

            default:
                return null;
        }
    }

    void UpdateMoneyUI()
    {
        if (txtMoney != null)
            txtMoney.text = money.ToString();
    }

    void RefreshAllPriceText()
    {
        RefreshGroupPriceText(topItems);
        RefreshGroupPriceText(bottomItems);
        RefreshGroupPriceText(hairItems);
        RefreshGroupPriceText(accessoryItems);
    }

    void RefreshGroupPriceText(ClosetPreviewItem[] items)
    {
        if (items == null)
            return;

        foreach (ClosetPreviewItem item in items)
        {
            if (item != null)
                item.RefreshPriceText();
        }
    }

    void UpdateAllStatus()
    {
        UpdateItemGroupStatus(topItems);
        UpdateItemGroupStatus(bottomItems);
        UpdateItemGroupStatus(hairItems);
        UpdateItemGroupStatus(accessoryItems);
    }

    void UpdateItemGroupStatus(ClosetPreviewItem[] items)
    {
        if (items == null)
            return;

        foreach (ClosetPreviewItem item in items)
        {
            if (item == null)
                continue;

            Image target = GetTargetImage(item.part);
            bool isEquipped = item.isOwned && target != null && target.sprite == item.itemSprite;

            item.UpdateStatus(isEquipped);
        }
    }
}