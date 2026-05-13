using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WarningBoxTrigger : MonoBehaviour
{
    // CharacterController도 물리 벽은 무시하지만, 
    // Trigger 영역에 들어가고 나가는 것은 완벽하게 정상 감지합니다!
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HubUIManager.Instance?.ToggleWarningUI(true);
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