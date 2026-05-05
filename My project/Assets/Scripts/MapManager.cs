using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public void OnClickLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void OnClickReturn()
    {
        SceneManager.LoadScene("home"); // 改成你們大廳的 Scene 名稱
    }
}
