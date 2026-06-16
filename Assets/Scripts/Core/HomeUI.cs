using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeUI : MonoBehaviour
{
    public void OnClickTraining()
    {
        StartCoroutine(LoadSceneDelay("SampleScene"));
    }

    public void OnClickCardBattle()
    {
        StartCoroutine(LoadSceneDelay("quest-map"));
    }

    public void OnClickPlayerHouse()
    {
        StartCoroutine(LoadSceneDelay("PersonalRoom"));
    }

    private IEnumerator LoadSceneDelay(string sceneName)
    {
        yield return new WaitForSeconds(0.4f);
        SceneManager.LoadScene(sceneName);
    }
}