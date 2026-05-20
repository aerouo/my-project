using TMPro;
using UnityEngine;

public class PlayerNameManager : MonoBehaviour
{
    [Header("玩家名稱顯示")]
    public TMP_Text txtPlayerName;

    [Header("修改名字面板")]
    public GameObject panelEditName;

    [Header("名字輸入框")]
    public TMP_InputField inputFieldName;

    private const int maxNameLength = 8;

    void Start()
    {
        // 預設名稱
        if (txtPlayerName != null && string.IsNullOrWhiteSpace(txtPlayerName.text))
        {
            txtPlayerName.text = "User";
        }

        // 限制最多 8 字
        if (inputFieldName != null)
        {
            inputFieldName.characterLimit = maxNameLength;
        }

        // 一開始關閉修改面板
        if (panelEditName != null)
        {
            panelEditName.SetActive(false);

            LoadPlayerName();
        }
    }

    public void OpenEditNamePanel()
    {
        if (panelEditName != null)
        {
            panelEditName.SetActive(true);
        }

        if (inputFieldName != null && txtPlayerName != null)
        {
            inputFieldName.text = txtPlayerName.text;
            inputFieldName.Select();
            inputFieldName.ActivateInputField();
        }
    }

    public void ConfirmName()
    {
        if (inputFieldName == null || txtPlayerName == null)
            return;

        string newName = inputFieldName.text.Trim();

        // 空白不修改
        if (string.IsNullOrEmpty(newName))
            return;

        // 保險：超過 8 字就裁掉
        if (newName.Length > maxNameLength)
        {
            newName = newName.Substring(0, maxNameLength);
        }

        txtPlayerName.text = newName;

        // 儲存名字到 Firebase
        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SaveUsername(newName);
        }

        if (panelEditName != null)
        {
            panelEditName.SetActive(false);
        }
    }

    public void CancelEditName()
    {
        if (panelEditName != null)
        {
            panelEditName.SetActive(false);
        }
    }
    public void LoadPlayerName()
    {
        if (txtPlayerName == null)
            return;

        if (FirestoreManager.Instance == null)
        {
            txtPlayerName.text = "user";
            return;
        }

        FirestoreManager.Instance.LoadUsername((username) =>
        {
            txtPlayerName.text = username;

            if (inputFieldName != null)
                inputFieldName.text = username;
        });
    }
}