using UnityEngine;

public class HomeUI : MonoBehaviour
{
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