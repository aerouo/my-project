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

    [Header("穿戴到角色上的設定")]
    public Vector2 equippedSize = new Vector2(150, 150);
    public Vector2 equippedPosition = Vector2.zero;

    [Header("狀態顯示")]
    public GameObject imgStatus;
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
        if (imgStatus != null)
            imgStatus.SetActive(isEquipped);

        if (txtStatus != null)
            txtStatus.text = isEquipped ? "已穿戴" : "";
    }
}