using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ClosetPart
{
    Hair,
    Top,
    Bottom,
    Accessory
}

public class ClosetPreviewItem : MonoBehaviour
{
    [Header("物品資料")]
    public ClosetPart part;
    public Sprite itemSprite;
    public TMP_Text txtStatus;

    [Header("管理器")]
    public ClosetPreviewManager manager;

    public void PreviewItem()
    {
        if (manager != null)
        {
            manager.PreviewItem(this);
        }
    }

    public void SetStatus(bool isEquipped)
    {
        if (txtStatus != null)
            txtStatus.text = isEquipped ? "已穿戴" : "";
    }
}