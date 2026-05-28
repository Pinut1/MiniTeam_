using MiniTeam.Core;
using UnityEngine;

public class MiniGameTrigger : MonoBehaviour
{
    [Header("진입할 미니게임 씬 이름")]
    [Scene]
    [SerializeField] private string targetSceneName;
    private bool isTriggered = false;

    private void OnEnable()
    {
        isTriggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered)
        {
            return;
        }

        
        // �ε��� ����� �÷��̾����� Ȯ�� (�÷��̾� ������Ʈ�� "Player" �±� �ʼ�)
        if (other.CompareTag("Player"))
        {
            isTriggered = true;
            Debug.Log($"{targetSceneName} ������ �̵��մϴ�!");
            MiniGameManager.Instance.EnterMiniGame(targetSceneName);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ���� ������ ��, �÷��̾ �� ���� ������ �� �����̶� ������ �ٽ� ���� ������
        if (other.CompareTag("Player"))
        {
            isTriggered = false;
        }
    }
}
