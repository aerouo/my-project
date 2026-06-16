using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PasswordVisibilityToggle : MonoBehaviour
{
    [Header("控制的密碼輸入框")]
    public TMP_InputField passwordInput;

    [Header("眼睛圖示")]
    public Image eyeImage;
    public Sprite eyeOpenSprite;
    public Sprite eyeClosedSprite;

    private bool isVisible = false;

    void Start()
    {
        SetPasswordVisible(false);
    }

    public void TogglePasswordVisible()
    {
        SetPasswordVisible(!isVisible);
    }

    private void SetPasswordVisible(bool visible)
    {
        isVisible = visible;

        if (passwordInput != null)
        {
            passwordInput.contentType = visible
                ? TMP_InputField.ContentType.Standard
                : TMP_InputField.ContentType.Password;

            passwordInput.ForceLabelUpdate();
        }

        if (eyeImage != null)
        {
            eyeImage.sprite = visible ? eyeOpenSprite : eyeClosedSprite;
        }
    }
}