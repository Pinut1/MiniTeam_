using MiniTeam.Core;
using UnityEngine;

public class MiniGameTrigger : MonoBehaviour
{
    [Header("연결될 미니게임 씬의 정확한 이름")]
    [SerializeField] private string targetSceneName = "MiniGame_1";
    private bool isTriggered = false;

    // 플레이어의 Collider가 이 문의 Trigger Collider에 닿았을 때 실행
    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered)
        {
            return;
        }

        
        // 부딪힌 대상이 플레이어인지 확인 (플레이어 오브젝트에 "Player" 태그 필수)
        if (other.CompareTag("Player"))
        {
            isTriggered = true;
            Debug.Log($"{targetSceneName} 씬으로 이동합니다!");
            MiniGameManager.Instance.EnterMiniGame(targetSceneName);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 씬이 복구된 후, 플레이어가 문 범위 밖으로 한 걸음이라도 나가면 다시 문을 열어줌
        if (other.CompareTag("Player"))
        {
            isTriggered = false;
        }
    }
}
