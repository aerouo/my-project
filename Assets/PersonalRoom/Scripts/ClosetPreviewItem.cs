using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ClosetPart
{
    Top,
    Bottom,
    Hair,
    Accessory
}

public class ClosetPreviewItem : MonoBehaviour
{
    [Header("物品資料")]
    public ClosetPart part;
    public Sprite itemSprite;
    public int price = 500;
    public bool isOwned = false;

    [Header("UI")]
    public Image imgIcon;

    [Header("穿戴狀態")]
    public GameObject imgStatus;
    public TMP_Text txtStatus;

    [Header("價格顯示")]
    public GameObject imgPrice;
    public TMP_Text txtPrice;

    [Header("穿戴位置與大小")]
    public Vector2 wearAnchoredPosition;
    public Vector2 wearSizeDelta = new Vector2(100, 100);
    public Vector3 wearScale = Vector3.one;
    public Vector3 wearRotation;

    private ClosetPreviewManager manager;

    void Start()
    {
        manager = FindAnyObjectByType<ClosetPreviewManager>();
        RefreshPriceText();
    }

    public void OnClickItem()
    {
        if (manager == null)
            manager = FindAnyObjectByType<ClosetPreviewManager>();

        if (manager != null)
            manager.ClickItem(this);
    }

    public void SetOwned(bool owned)
    {
        isOwned = owned;
    }

    public void RefreshPriceText()
    {
        if (txtPrice != null)
            txtPrice.text = price.ToString();
    }

    public void UpdateStatus(bool isEquipped)
    {
        RefreshPriceText();

        if (!isOwned)
        {
            if (imgPrice != null) imgPrice.SetActive(true);
            if (imgStatus != null) imgStatus.SetActive(false);
        }
        else if (isEquipped)
        {
            if (imgPrice != null) imgPrice.SetActive(false);
            if (imgStatus != null) imgStatus.SetActive(true);

            if (txtStatus != null)
                txtStatus.text = "已穿戴";
        }
        else
        {
            if (imgPrice != null) imgPrice.SetActive(false);
            if (imgStatus != null) imgStatus.SetActive(false);
        }
    }
}