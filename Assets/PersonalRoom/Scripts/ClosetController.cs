using UnityEngine;

public class ClosetCategoryController : MonoBehaviour
{
    [Header("Item Cards")]
    public GameObject[] topItems;
    public GameObject[] bottomItems;
    public GameObject[] hairItems;
    public GameObject[] accessoryItems;

    void Start()
    {
        ShowTop();
    }

    void HideAllItems()
    {
        foreach (GameObject item in topItems)
            item.SetActive(false);

        foreach (GameObject item in bottomItems)
            item.SetActive(false);

        foreach (GameObject item in hairItems)
            item.SetActive(false);

        foreach (GameObject item in accessoryItems)
            item.SetActive(false);
    }

    public void ShowTop()
    {
        HideAllItems();

        foreach (GameObject item in topItems)
            item.SetActive(true);
    }

    public void ShowBottom()
    {
        HideAllItems();

        foreach (GameObject item in bottomItems)
            item.SetActive(true);
    }

    public void ShowHair()
    {
        HideAllItems();

        foreach (GameObject item in hairItems)
            item.SetActive(true);
    }

    public void ShowAccessory()
    {
        HideAllItems();

        foreach (GameObject item in accessoryItems)
            item.SetActive(true);
    }
}