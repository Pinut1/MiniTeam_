using UnityEngine;
using TMPro;

public class WarningUIController : MonoBehaviour
{
    public static WarningUIController Instance { get; private set; }

    [SerializeField] private GameObject warningUI;
    [SerializeField] private TMP_Text warningText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ToggleWarningUI(bool isOn, string message = "")
    {
        if (warningUI != null) warningUI.SetActive(isOn);

        if (isOn && !string.IsNullOrEmpty(message) && warningText != null)
        {
            warningText.text = message;
        }
    }
}
