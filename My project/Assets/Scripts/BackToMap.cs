using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMap : MonoBehaviour
{
    public GameObject confirmPanel; // 拖入確認視窗物件

    public void OnClick()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(true);
    }

    public void OnConfirm()
    {
        SceneManager.LoadScene("quest-map");
    }

    public void OnCancel()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }
}