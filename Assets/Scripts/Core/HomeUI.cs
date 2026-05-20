using UnityEngine;

public class HomeUI : MonoBehaviour
{
    void Start()
    {
        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.LoadAchievementCache();
        }
    }
    public void OnClickTraining()
    {   
        SceneLoader.Instance.GoToWithLoading(SceneName.Training);
    }

    public void OnClickCardBattle()
    {   
        SceneLoader.Instance.GoToWithLoading(SceneName.CardBattle);
    }

    public void OnClickPlayerHouse()
    {
        SceneLoader.Instance.GoTo(SceneName.PlayerHouse);
    }
}