using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// PuzzleSlot.cs
/// 掛在 final group 底下每個格子（11_f ~ 33_f）
/// </summary>
public class PuzzleSlot : MonoBehaviour, IDropHandler
{
    [Header("格子 ID（對應 PuzzlePiece 的 pieceID）")]
    public string slotID;

    [Header("放對後換成這張圖")]
    public Sprite correctSprite;

    [Header("管理腳本")]
    public Puzzle manager;

    public bool isPlaced = false;

    private Image slotImage;

    void Awake()
    {
        slotImage = GetComponent<Image>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (isPlaced) return;

        PuzzlePiece piece = eventData.pointerDrag?.GetComponent<PuzzlePiece>();
        if (piece == null) return;

        if (piece.pieceID == slotID)
        {
            isPlaced = true;
            if (correctSprite != null && slotImage != null)
                slotImage.sprite = correctSprite;

            piece.PlaceCorrect();
            manager?.OnPiecePlaced();  // 通知管理腳本
        }
        else
        {
            piece.ReturnToOrigin();
        }
    }
}