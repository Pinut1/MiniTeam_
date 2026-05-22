using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WarningBoxTrigger : MonoBehaviour
{

    public string customWarningMessage = "default Warning Message";
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HubUIManager.Instance?.ToggleWarningUI(true, customWarningMessage);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HubUIManager.Instance?.ToggleWarningUI(false);
        }
    }
}