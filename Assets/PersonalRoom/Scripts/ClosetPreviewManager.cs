using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClosetPreviewManager : MonoBehaviour
{
    [Header("角色穿戴部位")]
    public Image equippedHair;
    public Image equippedTop;
    public Image equippedBottom;

    [Header("配件穿戴部位")]
    public Image equippedGlasses;
    public Image equippedGloves;
    public Image equippedShoes;
    public Image equippedHandItem;

    [Header("上衣物品")]
    public ClosetPreviewItem[] topItems;

    [Header("下衣物品")]
    public ClosetPreviewItem[] bottomItems;

    [Header("頭髮物品")]
    public ClosetPreviewItem[] hairItems;

    [Header("配件物品")]
    public ClosetPreviewItem[] accessoryItems;

    [Header("紙鶴金額")]
    public int defaultCoins = 10000;
    public int coins = 10000;
    public TMP_Text txtMoney;

    [Header("購買確認視窗")]
    public GameObject panelBuyConfirm;
    public TMP_Text txtBuyMessage;
    public Button btnBuyYes;
    public Button btnBuyNo;

    [Header("連身衣設定")]
    public ClosetPreviewItem top01OnePiece;
    public ClosetPreviewItem top02OnePiece;
    public ClosetPreviewItem defaultTopItem;
    public ClosetPreviewItem defaultBottomItem;

    [Header("讀取狀態")]
    public GameObject loadingPanel;

    private ClosetPreviewItem selectedItem;

    private OutfitState savedHair;
    private OutfitState savedTop;
    private OutfitState savedBottom;

    private OutfitState savedGlasses;
    private OutfitState savedGloves;
    private OutfitState savedShoes;
    private OutfitState savedHandItem;

    private Coroutine loadCoroutine;
    private bool firebaseLoaded = false;

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
        SetupButtons();

        if (panelBuyConfirm != null)
            panelBuyConfirm.SetActive(false);

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        ClearAllEquippedImages();
        UpdateCoinsUI();
        RefreshAllPriceText();
    }

    void OnEnable()
    {
        firebaseLoaded = false;

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (loadCoroutine != null)
            StopCoroutine(loadCoroutine);

        loadCoroutine = StartCoroutine(InitClosetFromFirebase());
    }

    void OnDisable()
    {
        CancelBuy();

        if (firebaseLoaded)
            CancelPreview();
    }

    IEnumerator InitClosetFromFirebase()
    {
        yield return null;

        float timer = 0f;

        while (FirestoreManager.Instance == null && timer < 5f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (FirestoreManager.Instance == null)
        {
            Debug.LogWarning("找不到 FirestoreManager，使用本地預設衣櫃");
            LoadDefaultCloset();
            firebaseLoaded = true;

            if (loadingPanel != null)
                loadingPanel.SetActive(false);

            yield break;
        }

        Debug.Log("開始讀取 Firebase 衣櫃資料");

        FirestoreManager.Instance.LoadCloset((dataDict, itemsDict, equippedDict) =>
        {
            Debug.Log("Firebase 衣櫃資料讀取成功，開始套用到 Unity");

            ApplyFirebaseCloset(dataDict, itemsDict, equippedDict);

            firebaseLoaded = true;

            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        });
    }

    void ApplyFirebaseCloset(
        Dictionary<string, object> dataDict,
        Dictionary<string, object> itemsDict,
        Dictionary<string, object> equippedDict)
    {
        LoadCoins(dataDict);

        ResetAllItemsOwned();
        LoadOwnedItems(itemsDict);
        EnsureDefaultItemsOwned();

        ClearAllEquippedImages();
        LoadEquippedItems(equippedDict);

        HideEmptyEquippedImages();
        UpdateCoinsUI();
        UpdateAllStatus();
        RefreshAllPriceText();
        SaveCurrentOutfit();

        Debug.Log("衣櫃 Firebase 資料已完整套用");
    }

    void LoadDefaultCloset()
    {
        coins = defaultCoins;

        ResetAllItemsOwned();
        ClearAllEquippedImages();

        HideEmptyEquippedImages();
        UpdateCoinsUI();
        UpdateAllStatus();
        RefreshAllPriceText();
        SaveCurrentOutfit();
    }

    void SetupButtons()
    {
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
    }

    public void ClickItem(ClosetPreviewItem item)
    {
        if (!firebaseLoaded)
        {
            Debug.LogWarning("衣櫃資料尚未讀取完成，暫時不能操作");
            return;
        }

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

        bool isAchievementOnly = item != null && item.achievementOnly;

        string itemType = "服飾";

        switch (item.part)
        {
            case ClosetPart.Top:
                itemType = "上衣";
                break;

            case ClosetPart.Bottom:
                itemType = "下衣";
                break;

            case ClosetPart.Hair:
                itemType = "髮型";
                break;

            case ClosetPart.Accessory:
                itemType = "配件";
                break;
        }

        if (txtBuyMessage != null)
        {
            if (isAchievementOnly)
            {
                txtBuyMessage.text =
                    "此" + itemType +
                    "為成就獎勵\n完成指定成就後\n即可解鎖";
            }
            else
            {
                txtBuyMessage.text =
                    "是否花費 " +
                    item.price +
                    " 紙鶴\n購買此" +
                    itemType +
                    "？";
            }
        }

        if (btnBuyYes != null)
        {
            btnBuyYes.gameObject.SetActive(true);

            TMP_Text btnText = btnBuyYes.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = isAchievementOnly ? "確認" : "購買";
        }

        if (btnBuyNo != null)
            btnBuyNo.gameObject.SetActive(!isAchievementOnly);

        if (panelBuyConfirm != null)
            panelBuyConfirm.SetActive(true);
    }

    void ConfirmBuy()
    {
        if (selectedItem == null)
            return;

        if (selectedItem.achievementOnly)
        {
            selectedItem = null;

            if (btnBuyNo != null)
                btnBuyNo.gameObject.SetActive(true);

            if (panelBuyConfirm != null)
                panelBuyConfirm.SetActive(false);

            return;
        }

        if (coins < selectedItem.price)
        {
            if (txtBuyMessage != null)
                txtBuyMessage.text = "紙鶴不足\n\n無法解鎖此服飾";

            return;
        }

        coins -= selectedItem.price;
        selectedItem.SetOwned(true);

        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveWardrobeItem(selectedItem.itemID, true);
            FirestoreManager.Instance.SaveCoins(coins);
        }

        if (panelBuyConfirm != null)
            panelBuyConfirm.SetActive(false);

        if (btnBuyYes != null)
            btnBuyYes.gameObject.SetActive(true);

        if (btnBuyNo != null)
            btnBuyNo.gameObject.SetActive(true);

        PreviewItem(selectedItem);

        UpdateCoinsUI();
        UpdateAllStatus();
        RefreshAllPriceText();

        selectedItem = null;
    }

    void CancelBuy()
    {
        selectedItem = null;

        if (btnBuyYes != null)
            btnBuyYes.gameObject.SetActive(true);

        if (btnBuyNo != null)
            btnBuyNo.gameObject.SetActive(true);

        if (panelBuyConfirm != null)
            panelBuyConfirm.SetActive(false);
    }

    void EquipItem(ClosetPreviewItem item)
    {
        ForceEquipItem(item);
        HandleSpecialOutfitRule(item);
        HideEmptyEquippedImages();
        UpdateAllStatus();
    }

    public void PreviewItem(ClosetPreviewItem item)
    {
        if (item == null || item.itemSprite == null)
            return;

        Image target = GetTargetImage(item);

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

        Image target = GetTargetImage(item);

        if (target == null)
            return;

        target.sprite = item.itemSprite;
        target.color = Color.white;

        ApplyItemTransform(target, item);
    }

    void HandleSpecialOutfitRule(ClosetPreviewItem item)
    {
        if (item == null)
            return;

        if (item.part == ClosetPart.Top && IsOnePieceTop(item))
        {
            ClearEquippedImage(equippedBottom);
            return;
        }

        if (item.part == ClosetPart.Top && !IsOnePieceTop(item))
        {
            if (equippedBottom != null && equippedBottom.sprite == null && defaultBottomItem != null)
                ForceEquipItem(defaultBottomItem);

            return;
        }

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

    void ApplyItemTransform(Image target, ClosetPreviewItem item)
    {
        RectTransform rect = target.GetComponent<RectTransform>();

        if (rect == null)
            return;

        rect.anchoredPosition = item.wearAnchoredPosition;
        rect.sizeDelta = item.wearSizeDelta;
        rect.localScale = item.wearScale;
        rect.localEulerAngles = item.wearRotation;
    }

    public void ApplyOutfit()
    {
        SaveCurrentOutfit();
        UpdateAllStatus();
        SaveEquippedToFirebase();
    }

    public void CancelPreview()
    {
        RestoreSavedOutfit();
        HideEmptyEquippedImages();
        UpdateAllStatus();
    }
    public void CloseCloset()
    {
        if (firebaseLoaded)
            CancelPreview();

        CancelBuy();
    }

    void SaveEquippedToFirebase()
    {
        if (FirestoreManager.Instance == null)
            return;

        SaveEquippedPart("top", equippedTop);
        SaveEquippedPart("bottom", equippedBottom);
        SaveEquippedPart("hair", equippedHair);

        SaveEquippedPart("glasses", equippedGlasses);
        SaveEquippedPart("gloves", equippedGloves);
        SaveEquippedPart("shoes", equippedShoes);
        SaveEquippedPart("handItem", equippedHandItem);

        FirestoreManager.Instance.SaveCoins(coins);
    }

    void SaveEquippedPart(string part, Image equippedImage)
    {
        if (FirestoreManager.Instance == null)
            return;

        if (equippedImage == null || equippedImage.sprite == null)
        {
            FirestoreManager.Instance.SaveClosetEquip(part, "");
            return;
        }

        ClosetPreviewItem item = FindItemBySprite(equippedImage.sprite);

        if (item != null)
            FirestoreManager.Instance.SaveClosetEquip(part, item.itemID);
        else
            FirestoreManager.Instance.SaveClosetEquip(part, "");
    }

    void LoadCoins(Dictionary<string, object> dataDict)
    {
        coins = defaultCoins;

        if (dataDict == null || dataDict.Count == 0)
        {
            Debug.LogWarning("Firebase user data 是空的，使用預設紙鶴：" + defaultCoins);
            return;
        }

        if (dataDict.TryGetValue("coins", out object coinsObj))
        {
            coins = int.Parse(coinsObj.ToString());
            Debug.Log("Firebase 載入 coins：" + coins);
            return;
        }

        Debug.LogWarning("Firebase 找不到 coins，使用預設紙鶴：" + defaultCoins);
    }

    void LoadOwnedItems(Dictionary<string, object> itemsDict)
    {
        LoadOwnedGroup(topItems, itemsDict);
        LoadOwnedGroup(bottomItems, itemsDict);
        LoadOwnedGroup(hairItems, itemsDict);
        LoadOwnedGroup(accessoryItems, itemsDict);
    }

    void LoadOwnedGroup(ClosetPreviewItem[] items, Dictionary<string, object> data)
    {
        if (items == null)
            return;

        foreach (ClosetPreviewItem item in items)
        {
            if (item == null)
                continue;

            if (string.IsNullOrEmpty(item.itemID))
            {
                Debug.LogWarning("衣服沒有填 itemID：" + item.name);
                item.SetOwned(false);
                continue;
            }

            bool owned = false;

            if (data != null && data.TryGetValue(item.itemID, out object ownedObj))
                owned = bool.Parse(ownedObj.ToString());

            item.SetOwned(owned);

            Debug.Log("套用購買狀態：" + item.itemID + " = " + owned);
        }
    }

    void LoadEquippedItems(Dictionary<string, object> equippedDict)
    {
        if (equippedDict == null)
            equippedDict = new Dictionary<string, object>();

        LoadEquippedPart("top", equippedDict, topItems);
        LoadEquippedPart("bottom", equippedDict, bottomItems);
        LoadEquippedPart("hair", equippedDict, hairItems);

        LoadEquippedPart("glasses", equippedDict, accessoryItems);
        LoadEquippedPart("gloves", equippedDict, accessoryItems);
        LoadEquippedPart("shoes", equippedDict, accessoryItems);
        LoadEquippedPart("handItem", equippedDict, accessoryItems);

        if (equippedTop != null && equippedTop.sprite == null && defaultTopItem != null)
            ForceEquipItem(defaultTopItem);

        if (equippedBottom != null && equippedBottom.sprite == null && defaultBottomItem != null)
            ForceEquipItem(defaultBottomItem);
    }

    void LoadEquippedPart(string part, Dictionary<string, object> equippedDict, ClosetPreviewItem[] items)
    {
        if (items == null)
            return;

        if (!equippedDict.TryGetValue(part, out object itemIDObj))
            return;

        string itemID = itemIDObj.ToString();

        if (string.IsNullOrEmpty(itemID))
            return;

        foreach (ClosetPreviewItem item in items)
        {
            if (item == null)
                continue;

            if (item.itemID == itemID)
            {
                if (!item.isOwned && !IsDefaultItem(item))
                {
                    Debug.LogWarning("Firebase 穿戴的衣服尚未購買，略過：" + part + " = " + itemID);
                    return;
                }

                ForceEquipItem(item);
                HandleSpecialOutfitRule(item);

                Debug.Log("套用穿戴成功：" + part + " = " + itemID);
                return;
            }
        }

        Debug.LogWarning("Firebase 有穿戴資料，但 Unity 找不到 itemID：" + part + " = " + itemID);
    }

    bool IsDefaultItem(ClosetPreviewItem item)
    {
        return item == defaultTopItem || item == defaultBottomItem;
    }

    void EnsureDefaultItemsOwned()
    {
        if (defaultTopItem != null)
            defaultTopItem.SetOwned(true);

        if (defaultBottomItem != null)
            defaultBottomItem.SetOwned(true);
    }

    void ResetAllItemsOwned()
    {
        ResetOwnedGroup(topItems);
        ResetOwnedGroup(bottomItems);
        ResetOwnedGroup(hairItems);
        ResetOwnedGroup(accessoryItems);
    }

    void ResetOwnedGroup(ClosetPreviewItem[] items)
    {
        if (items == null)
            return;

        foreach (ClosetPreviewItem item in items)
        {
            if (item != null)
                item.SetOwned(false);
        }
    }

    ClosetPreviewItem FindItemBySprite(Sprite sprite)
    {
        ClosetPreviewItem item;

        item = FindItemInGroup(topItems, sprite);
        if (item != null) return item;

        item = FindItemInGroup(bottomItems, sprite);
        if (item != null) return item;

        item = FindItemInGroup(hairItems, sprite);
        if (item != null) return item;

        item = FindItemInGroup(accessoryItems, sprite);
        if (item != null) return item;

        return null;
    }

    ClosetPreviewItem FindItemInGroup(ClosetPreviewItem[] items, Sprite sprite)
    {
        if (items == null || sprite == null)
            return null;

        foreach (ClosetPreviewItem item in items)
        {
            if (item != null && item.itemSprite == sprite)
                return item;
        }

        return null;
    }

    void SaveCurrentOutfit()
    {
        savedHair = SaveOutfitState(equippedHair);
        savedTop = SaveOutfitState(equippedTop);
        savedBottom = SaveOutfitState(equippedBottom);

        savedGlasses = SaveOutfitState(equippedGlasses);
        savedGloves = SaveOutfitState(equippedGloves);
        savedShoes = SaveOutfitState(equippedShoes);
        savedHandItem = SaveOutfitState(equippedHandItem);
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

        RestoreOutfitState(equippedGlasses, savedGlasses);
        RestoreOutfitState(equippedGloves, savedGloves);
        RestoreOutfitState(equippedShoes, savedShoes);
        RestoreOutfitState(equippedHandItem, savedHandItem);
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

        HideIfEmpty(image);
    }

    Image GetTargetImage(ClosetPreviewItem item)
    {
        if (item == null)
            return null;

        switch (item.part)
        {
            case ClosetPart.Hair:
                return equippedHair;

            case ClosetPart.Top:
                return equippedTop;

            case ClosetPart.Bottom:
                return equippedBottom;

            case ClosetPart.Accessory:

                switch (item.accessoryType)
                {
                    case AccessoryType.Glasses:
                        return equippedGlasses;

                    case AccessoryType.Gloves:
                        return equippedGloves;

                    case AccessoryType.Shoes:
                        return equippedShoes;

                    case AccessoryType.HandItem:
                        return equippedHandItem;
                }

                break;
        }

        return null;
    }

    void ClearAllEquippedImages()
    {
        ClearEquippedImage(equippedTop);
        ClearEquippedImage(equippedBottom);
        ClearEquippedImage(equippedHair);

        ClearEquippedImage(equippedGlasses);
        ClearEquippedImage(equippedGloves);
        ClearEquippedImage(equippedShoes);
        ClearEquippedImage(equippedHandItem);
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

        HideIfEmpty(equippedGlasses);
        HideIfEmpty(equippedGloves);
        HideIfEmpty(equippedShoes);
        HideIfEmpty(equippedHandItem);
    }

    void HideIfEmpty(Image image)
    {
        if (image == null)
            return;

        if (image.sprite == null)
            image.color = new Color(1, 1, 1, 0);
    }

    void UpdateCoinsUI()
    {
        if (txtMoney != null)
            txtMoney.text = coins.ToString();
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

            Image target = GetTargetImage(item);
            bool isEquipped = item.isOwned && target != null && target.sprite == item.itemSprite;

            item.UpdateStatus(isEquipped);
        }
    }
}