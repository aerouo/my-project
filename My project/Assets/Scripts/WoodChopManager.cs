using UnityEngine;

public class WoodChopManager : MonoBehaviour
{
    [Header("設定")]
    public int totalWoods = 6;          // 總共幾棵樹

    [Header("完成後銜接")]
    public Level4Manager level4Manager; // 拉入 Level4Manager

    private int choppedCount = 0;

    public void OnWoodChopped()
    {
        choppedCount++;
        if (choppedCount >= totalWoods)
        {
            if (level4Manager != null)
                level4Manager.OnWoodComplete();
        }
    }
}