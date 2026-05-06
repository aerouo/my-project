using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMap : MonoBehaviour
{
    public void OnClick()
    {
        SceneManager.LoadScene("quest-map");
    }
}