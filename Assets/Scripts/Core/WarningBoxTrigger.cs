using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WarningBoxTrigger : MonoBehaviour
{

    public string customWarningMessage = "이 너머로 가도 볼 건 없을 것 같다...";
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WarningUIController.Instance?.ToggleWarningUI(true, customWarningMessage);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WarningUIController.Instance?.ToggleWarningUI(false);
        }
    }
}