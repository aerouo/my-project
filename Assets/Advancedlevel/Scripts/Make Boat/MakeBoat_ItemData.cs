using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "FoxQuiz/Item")]
public class MakeBoat_ItemData : ScriptableObject
{
    [Header("道具名稱")]
    public string itemName;
    

    [Header("道具圖片")]
    public Sprite sprite;

    [Header("選到這個道具得到的分數（可為負數）")]
    public int scoreValue = 10;
}