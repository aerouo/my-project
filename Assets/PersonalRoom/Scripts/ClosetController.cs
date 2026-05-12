using UnityEngine;
using UnityEngine.UI;

public class ClosetCategoryController : MonoBehaviour
{
    [Header("Buttons")]
    public Image btnTop;
    public Image btnBottom;
    public Image btnHair;
    public Image btnAccessory;

    [Header("Top Sprites")]
    public Sprite topNormal;
    public Sprite topSelected;

    [Header("Bottom Sprites")]
    public Sprite bottomNormal;
    public Sprite bottomSelected;

    [Header("Hair Sprites")]
    public Sprite hairNormal;
    public Sprite hairSelected;

    [Header("Accessory Sprites")]
    public Sprite accessoryNormal;
    public Sprite accessorySelected;

    [Header("Items")]
    public GameObject[] topItems;
    public GameObject[] bottomItems;
    public GameObject[] hairItems;
    public GameObject[] accessoryItems;

    void Start()
    {
        ShowTop();
    }

    void ResetButtons()
    {
        btnTop.sprite = topNormal;
        btnBottom.sprite = bottomNormal;
        btnHair.sprite = hairNormal;
        btnAccessory.sprite = accessoryNormal;
    }

    void HideAllItems()
    {
        SetItemsActive(topItems, false);
        SetItemsActive(bottomItems, false);
        SetItemsActive(hairItems, false);
        SetItemsActive(accessoryItems, false);
    }

    void SetItemsActive(GameObject[] items, bool active)
    {
        if (items == null)
            return;

        foreach (GameObject item in items)
        {
            if (item != null)
                item.SetActive(active);
        }
    }

    public void ShowTop()
    {
        ResetButtons();
        HideAllItems();

        btnTop.sprite = topSelected;
        SetItemsActive(topItems, true);
    }

    public void ShowBottom()
    {
        ResetButtons();
        HideAllItems();

        btnBottom.sprite = bottomSelected;
        SetItemsActive(bottomItems, true);
    }

    public void ShowHair()
    {
        ResetButtons();
        HideAllItems();

        btnHair.sprite = hairSelected;
        SetItemsActive(hairItems, true);
    }

    public void ShowAccessory()
    {
        ResetButtons();
        HideAllItems();

        btnAccessory.sprite = accessorySelected;
        SetItemsActive(accessoryItems, true);
    }
}